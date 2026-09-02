using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Net.WebSockets;
using System.Reflection;
using System.Text.Json.Nodes;
using CNCO.Unify.Communications.Http.Routing;

namespace CNCO.Unify.Communications.Http.Routing.ControllerInvoker;

/// <summary>
/// Used by <see cref="Router"/> to invoke a method within a <see cref="Controller"/>.
/// </summary>
internal class ControllerMethodInvoker
{
  private readonly ParameterInfo[] _methodParameters;

  private delegate object? CompiledMethodInvoker(Controller controller, IWebRequest request);

  [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
  private Type ControllerType { get; }
  public IEnumerable<HttpMethodAttribute> HttpMethodAttributes { get; }
  public IEnumerable<WebSocketAttribute> WebSocketAttributes { get; }
  public MethodInfo MethodInfo { get; }

  public ControllerMethodInvoker(
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type controller,
    IEnumerable<HttpMethodAttribute>? httpMethodAttributes,
    IEnumerable<WebSocketAttribute>? webSocketAttributes,
    MethodInfo methodInfo
  )
  {
    ArgumentNullException.ThrowIfNull(controller);
    ArgumentNullException.ThrowIfNull(methodInfo);
    if (httpMethodAttributes == null && webSocketAttributes == null)
    {
      throw new ArgumentNullException(
        nameof(httpMethodAttributes),
        $"At least one of {nameof(httpMethodAttributes)} or {nameof(webSocketAttributes)} must be provided."
      );
    }

    ControllerType = controller;
    HttpMethodAttributes = httpMethodAttributes?.Where(x => x != null) ?? [];
    WebSocketAttributes = webSocketAttributes?.Where(x => x != null) ?? [];
    MethodInfo = methodInfo;
    _methodParameters = [.. MethodInfo.GetParameters().Where(x => x != null)];
  }

  public Action<IWebRequest, IWebResponse> CompileRequestInvoker(string route)
  {
    // TODO: compile an expression tree or something to invoke the method with parameters instead of using reflection every time

    /*
     * The idea here is to create a delegate that can be invoked directly rather than using reflection every time, which is slow.
     * The delegate would be created based on the method's parameters and would extract the necessary data from the request to pass to the method.
     * This would involve analyzing the method's parameters, determining how to extract the necessary data from the request (e.g., from route parameters, query parameters, headers, etc.),
     * and then creating a delegate that can be invoked with the request and response objects.
     * This would significantly improve performance for subsequent requests after the initial compilation of the delegate.
     */

    if (_methodParameters.Length == 0)
    {
      return (request, response) =>
      {
        var possibleResponse = MethodInfo.Invoke(
          CreateControllerInstance(request, response, null),
          []
        );
        if (possibleResponse != null)
        {
          ProcessResponse(possibleResponse, response);
        }
      };
    }

    var compiledMethodInvoker = CompileMethodInvoker();

    return (request, response) =>
    {
      var controller = CreateControllerInstance(request, response, null);

      // Call the compiled expression
      var result = compiledMethodInvoker(controller, request);

      // Handle response
      if (result != null)
      {
        ProcessResponse(result, response);
      }
    };
  }

  public Action<IWebSocket> CompileWebSocketInvoker(string route)
  {
    // TODO: compile an expression tree or something to invoke the method with parameters instead of using reflection every time

    return (webSocket) => Invoke(webSocket.WebRequest, null, webSocket);
  }

  private Controller CreateControllerInstance(
    IWebRequest request,
    IWebResponse? response,
    IWebSocket? webSocket
  )
  {
    ControllerContext controllerContext = new ControllerContext(request, response, webSocket);
    var classInstance = Activator.CreateInstance(ControllerType);

    if (classInstance is not Controller controllerInstance || controllerInstance == null)
    {
      CommunicationsRuntime.Current.RuntimeLog.Warning(
        GetTag(nameof(Invoke)),
        $"{nameof(classInstance)} could not be casted to type ${ControllerType.Name}! "
          + $"{nameof(classInstance)} type? {classInstance?.GetType().FullName} - base? {classInstance?.GetType().BaseType?.FullName ?? "UNKNOWN"}"
      );

      throw new InvalidCastException(
        $"Cannot create an instance of {ControllerType.FullName} as it does not inherit from {nameof(Controller)}."
      );
    }
    controllerInstance.Context = controllerContext;

    return controllerInstance;
  }

  private CompiledMethodInvoker CompileMethodInvoker()
  {
    // Used for when throwing nullability issues.
    var invalidOperationConstructor =
      typeof(InvalidOperationException).GetConstructor([typeof(string)])
      ?? throw new InvalidOperationException(
        "Missing InvalidOperationException(string) constructor."
      );

    // Build our expression tree
    var controllerParameter = Expression.Parameter(typeof(Controller), "controller");
    var requestParameter = Expression.Parameter(typeof(IWebRequest), "request");

    // Determine which parameters are required (non-nullable)
    var requiredParameterIndices = new List<int>(_methodParameters.Length);
    for (int i = 0; i < _methodParameters.Length; i++)
    {
      var targetType = _methodParameters[i].ParameterType;
      var isNullable = Nullable.GetUnderlyingType(targetType) != null || !targetType.IsValueType;
      if (!isNullable)
      {
        requiredParameterIndices.Add(i);
      }
    }

    var methodParametersExpressions = new Expression[_methodParameters.Length];
    var blockExpressions = new List<Expression>();
    var blockVariables = new List<ParameterExpression>();

    // If there are required (non-nullable) parameters, guard against null RouteTemplate
    AddRouteTemplateNullGuard(
      invalidOperationConstructor,
      requestParameter,
      requiredParameterIndices,
      blockExpressions
    );

    // Get the RouteParameters
    var routeParameters = Expression.Variable(
      typeof(IEnumerable<RouteParameter>),
      "routeParameters"
    );
    blockVariables.Add(routeParameters);
    //> $request.RouteTemplate
    var routeTemplateProperty = Expression.PropertyOrField(
      requestParameter,
      nameof(IWebRequest.RouteTemplate)
    );
    //> $routeParameters = $request.RouteTemplate?.RouteParameters
    var assignRouteParameters = Expression.Assign(
      routeParameters,
      Expression.Condition(
        Expression.Equal(routeTemplateProperty, Expression.Constant(null, typeof(RouteTemplate))),
        Expression.Constant(null, typeof(IEnumerable<RouteParameter>)),
        Expression.PropertyOrField(routeTemplateProperty, nameof(RouteTemplate.RouteParameters))
      )
    );
    blockExpressions.Add(assignRouteParameters);

    for (int i = 0; i < _methodParameters.Length; i++)
    {
      var paramName = _methodParameters[i].Name!;
      var targetType = _methodParameters[i].ParameterType;
      methodParametersExpressions[i] = HandleMethodParameter(
        invalidOperationConstructor,
        routeParameters,
        blockVariables,
        paramName,
        targetType
      );
    }

    // Cast to the actual controller type
    var castedController = Expression.Convert(controllerParameter, ControllerType);

    var methodCallExpression = Expression.Call(
      castedController,
      MethodInfo,
      methodParametersExpressions
    );

    // If method returns void, we need to handle that
    Expression bodyExpr =
      MethodInfo.ReturnType == typeof(void)
        ? Expression.Block(methodCallExpression, Expression.Constant(null))
        : Expression.Convert(methodCallExpression, typeof(object));

    blockExpressions.Add(bodyExpr);

    // Combine the null-check (if needed) with the method invocation
    var finalBodyExpr =
      blockExpressions.Count > 1
        ? Expression.Block(blockVariables, blockExpressions)
        : blockExpressions[0];

    var lambda = Expression.Lambda<CompiledMethodInvoker>(
      finalBodyExpr,
      controllerParameter,
      requestParameter
    );

    return lambda.Compile();
  }

  private BlockExpression HandleMethodParameter(
    ConstructorInfo invalidOperationConstructor,
    ParameterExpression routeParameters,
    List<ParameterExpression> blockVariables,
    string paramName,
    Type targetType
  )
  {
    var isNullable = Nullable.GetUnderlyingType(targetType) != null || !targetType.IsValueType;

    // Variables

    var routeParameter = Expression.Variable(typeof(RouteParameter), "routeParameter");
    var routeParameterValue = Expression.Variable(typeof(object), "routeParameterValue");
    blockVariables.Add(routeParameter);
    blockVariables.Add(routeParameterValue);

    // predicate: `p => p.Name == paramName`
    var paramVar = Expression.Parameter(typeof(RouteParameter), "p");
    var predicate = Expression.Lambda<Func<RouteParameter, bool>>(
      Expression.Equal(
        Expression.PropertyOrField(paramVar, nameof(RouteParameter.Name)),
        Expression.Constant(paramName)
      ),
      paramVar
    );

    //> $routeParameters.FirstOrDefault(p => p.Name == paramName)
    var firstOrDefaultCall = Expression.Call(
      typeof(Enumerable),
      nameof(Enumerable.FirstOrDefault),
      [typeof(RouteParameter)],
      routeParameters,
      predicate
    );

    //> $routeParameter = $routeParameters.FirstOrDefault(..) ?? null
    var assignRouteParameter = Expression.IfThenElse(
      Expression.Equal(
        routeParameters,
        Expression.Constant(null, typeof(IEnumerable<RouteParameter>))
      ),
      Expression.Assign(routeParameter, Expression.Constant(null, typeof(RouteParameter))),
      Expression.Assign(routeParameter, firstOrDefaultCall)
    );

    var blockForParameter = new List<Expression> { assignRouteParameter };

    //> $routeParameter == null
    var routeParameterIsNull = Expression.Equal(
      routeParameter,
      Expression.Constant(null, typeof(RouteParameter))
    );

    // Nullable method parameter? (can we provide null or should we throw?)
    if (isNullable)
    {
      //> $routeParameterValue = ($routeParameter == null)? null : (TypeCasting)$routeParameter.Value
      blockForParameter.Add(
        Expression.IfThenElse(
          routeParameterIsNull,
          Expression.Assign(routeParameterValue, Expression.Constant(null, typeof(object))),
          CreateValueExtractionAndConversion(routeParameter, targetType, routeParameterValue)
        )
      );
    }
    else
    {
      // Required parameter: throw if not found
      //> $routeParameter == null? throw new InvalidOperation("Required route param...")
      blockForParameter.Add(
        Expression.IfThen(
          routeParameterIsNull,
          Expression.Throw(
            Expression.New(
              invalidOperationConstructor,
              Expression.Constant(
                $"Required route parameter '{paramName}' not found for method {ControllerType.FullName ?? ControllerType.Name}::{MethodInfo.Name}(...)"
              )
            )
          )
        )
      );

      //> $routeParameterValue = (TypeCasting)$routeParameter.Value
      blockForParameter.Add(
        CreateValueExtractionAndConversion(routeParameter, targetType, routeParameterValue)
      );
    }

    //> (TypeCasting)$routeParameterValue
    blockForParameter.Add(Expression.Convert(routeParameterValue, targetType));

    // Create block and return it (the block's type will be targetType due to the final Convert expression)
    return Expression.Block(targetType, [routeParameter, routeParameterValue], blockForParameter);
  }

  private void AddRouteTemplateNullGuard(
    ConstructorInfo invalidOperationConstructor,
    ParameterExpression requestParameter,
    List<int> requiredParameterIndices,
    List<Expression> blockExpressions
  )
  {
    if (requiredParameterIndices.Count > 0)
    {
      var routeTemplateProperty = Expression.PropertyOrField(
        requestParameter,
        nameof(IWebRequest.RouteTemplate)
      );
      var routeParametersProperty = Expression.PropertyOrField(
        routeTemplateProperty,
        nameof(RouteTemplate.RouteParameters)
      );

      // Check if RouteTemplate or RouteParameters is null; throw if required params exist
      var nullCheckExpr = Expression.IfThen(
        Expression.OrElse(
          Expression.Equal(routeTemplateProperty, Expression.Constant(null, typeof(RouteTemplate))),
          Expression.Equal(routeParametersProperty, Expression.Constant(null))
        ),
        Expression.Throw(
          Expression.New(
            invalidOperationConstructor,
            Expression.Constant(
              $"Method ({ControllerType.FullName ?? ControllerType.Name}::{MethodInfo.Name}(...)) requires route parameters, but none were found in the request!"
            )
          )
        )
      );

      blockExpressions.Add(nullCheckExpr);
    }
  }

  /// <summary>
  /// Creates an expression that extracts and converts a route parameter value to the target type.
  /// Mirrors the logic in the Invoke() method: handles string via ToStringValue(),
  /// and uses Convert.ChangeType for other types.
  /// </summary>
  private static Expression CreateValueExtractionAndConversion(
    ParameterExpression foundParameter,
    Type targetType,
    ParameterExpression valueVariable
  )
  {
    var blockExpressions = new List<Expression>();

    // Get the RouteParameter.Value property
    var paramValue = Expression.PropertyOrField(foundParameter, nameof(RouteParameter.Value));

    // Check if target is string (or string?) type
    if (targetType == typeof(string) || Nullable.GetUnderlyingType(targetType) == typeof(string))
    {
      // Use ToStringValue() method if it exists
      var toStringValueMethod = typeof(RouteParameter).GetMethod(
        nameof(RouteParameter.ToString),
        BindingFlags.Public | BindingFlags.Instance,
        null,
        [],
        null
      );

      if (toStringValueMethod != null)
      {
        blockExpressions.Add(
          Expression.Assign(valueVariable, Expression.Call(foundParameter, toStringValueMethod))
        );
      }
      else
      {
        // Fallback: just cast Value to string
        blockExpressions.Add(
          Expression.Assign(valueVariable, Expression.Convert(paramValue, typeof(string)))
        );
      }
    }
    else
    {
      // For non-string types, check type match and convert if needed
      var paramTypeProperty = Expression.PropertyOrField(
        foundParameter,
        nameof(RouteParameter.Type)
      );
      var typeMatches = Expression.Equal(paramTypeProperty, Expression.Constant(targetType));

      // If types match, use value directly; otherwise attempt Convert.ChangeType
      var convertChangeTypeMethod = typeof(Convert).GetMethod(
        nameof(Convert.ChangeType),
        BindingFlags.Public | BindingFlags.Static,
        null,
        [typeof(object), typeof(Type)],
        null
      );

      if (convertChangeTypeMethod != null)
      {
        // Create a TryConvert helper or use ChangeType which throws on failure
        // For now, we'll use ChangeType and handle exceptions at runtime (caller responsibility)
        blockExpressions.Add(
          Expression.IfThenElse(
            typeMatches,
            Expression.Assign(valueVariable, paramValue),
            Expression.Assign(
              valueVariable,
              Expression.Call(convertChangeTypeMethod, paramValue, Expression.Constant(targetType))
            )
          )
        );
      }
      else
      {
        // Fallback: direct conversion attempt
        blockExpressions.Add(
          Expression.Assign(valueVariable, Expression.Convert(paramValue, targetType))
        );
      }
    }

    // Return the assignment expression (or block if multiple steps)
    return blockExpressions.Count == 1 ? blockExpressions[0] : Expression.Block(blockExpressions);
  }

  /// <summary>
  /// Invokes the controller's method
  /// </summary>
  /// <param name="request"></param>
  /// <param name="response"></param>
  /// <exception cref="TargetParameterCountException"></exception>
  public void Invoke(IWebRequest request, IWebResponse? response, IWebSocket? webSocket)
  {
    var controller = CreateControllerInstance(request, response, webSocket);

    // Process parameters
    var methodParameters = MethodInfo.GetParameters();
    if (methodParameters.Length == 0)
    { // Ayy, no parameters!
      MethodInfo.Invoke(controller, []);
      return;
    }

    var nonNullParametersCount = methodParameters.Length; /*(param =>
      Nullable.GetUnderlyingType(param.ParameterType) == null
      && !param.CustomAttributes.Any(attribute =>
        attribute.AttributeType.Name == "NullableAttribute"
      )
    );*/

    if (
      // No data in route, but required
      (request.RouteTemplate?.RouteParameters == null && nonNullParametersCount > 0)
      // OR, data in route is not equal to nonnull count
      || request.RouteTemplate?.RouteParameters.Count() < nonNullParametersCount
    )
    {
      // ? what
      CommunicationsRuntime.Current.RuntimeLog.Alert(
        "Router::ControllerMethod::Callback",
        $"{nameof(controller)}::{MethodInfo.Name}() has an incorrect parameter count for the current request template! "
          + $"Request has {request.RouteTemplate?.RouteParameters.Count() ?? 0} parameters, but the method requires {methodParameters.Length}."
      );
      throw new TargetParameterCountException(
        $"Request has {request.RouteTemplate?.RouteParameters.Count() ?? 0} parameters, but the method requires {methodParameters.Length}."
      );
    }

    // try to inject the parameters :)
    var parameters = new List<object?>(methodParameters.Length);
    for (int i = 0; i < methodParameters.Length; i++)
    {
      var parameter = request.RouteTemplate?.RouteParameters.ElementAt(i);

      if (parameter == null)
      {
        parameters.Add(null);
        continue;
      }

      if (methodParameters[i].ParameterType == typeof(string))
      {
        parameters.Add(parameter?.ToStringValue());
        continue;
      }

      object? value = parameter?.Value ?? null;
      if (parameter?.Type != methodParameters[i].ParameterType)
      {
        // _try_ casting
        try
        {
          value = Convert.ChangeType(parameter?.Value, methodParameters[i].ParameterType);
        }
        catch (Exception e)
        {
          // nope
          string methodTag =
            $"{ControllerType.FullName}::{MethodInfo.Name}({string.Join(", ", methodParameters.Select(x => x.ParameterType))})";
          CommunicationsRuntime.Current.RuntimeLog.Warning(
            "Router::ControllerMethod::Callback",
            $"Cannot call {methodTag}) as the request parameter type {parameter?.Type.FullName ?? "NULL_PARAMETER"} "
              + $"does not match the method parameter type {methodParameters[i].ParameterType.FullName}! "
              + $"An attempt to convert via Convert.ChangeType() failed with the following message: {e.Message}"
          );
          return;
        }
      }

      parameters.Add(value);
    }

    // Call the controller's method
    var possibleResponse = MethodInfo.Invoke(controller, [.. parameters]);
    if (possibleResponse != null)
    {
      ProcessResponse(possibleResponse, response);
    }
  }

  private static void ProcessResponse(object methodReturn, IWebResponse? response)
  {
    if (methodReturn is IWebResponse actualResponse)
    {
      actualResponse.End();
    }
    if (response == null)
    {
      return;
    }

    switch (methodReturn)
    {
      case Stream streamResponse:
        response.Send(streamResponse);
        return;
      case byte[] byteArrayResponse:
        response.Send(new MemoryStream(byteArrayResponse));
        return;
      case string stringResponse:
        response.Send(stringResponse);
        return;
      case JsonObject jsonResponse:
        response.SendJson(jsonResponse);
        return;
    }
  }

  private string GetTag(string method) =>
    $"{nameof(ControllerMethodInvoker)}({ControllerType.Name})::{method}()";
}
