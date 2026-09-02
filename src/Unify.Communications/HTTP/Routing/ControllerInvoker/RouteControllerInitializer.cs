using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace CNCO.Unify.Communications.Http.Routing.ControllerInvoker;

internal static class RouteControllerInitializer
{
  public static void Initialize(IRouter router)
  {
    var controllers = GetControllers();
    foreach (var controller in controllers)
    {
      string controllerRoute = GetControllerRoute(controller);
      var controllerMethodInvokers = GetControllerMethodInvokers(controller);
      foreach (var invoker in controllerMethodInvokers)
      {
        AddControllerListener(router, controllerRoute, invoker);
      }
    }
  }

  private static IEnumerable<Type> GetControllers() =>
    from assemblies in AppDomain.CurrentDomain.GetAssemblies()
    from types in assemblies.GetTypes()
    where types.IsDefined(typeof(ControllerAttribute), true)
    select types;

  private static IEnumerable<ControllerMethodInvoker> GetControllerMethodInvokers(
    [DynamicallyAccessedMembers(
      DynamicallyAccessedMemberTypes.PublicConstructors
        | DynamicallyAccessedMemberTypes.PublicMethods
    )]
      Type controller
  )
  {
    var methods = controller.GetMethods(
      BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly
    );

    foreach (var method in methods)
    {
      var httpMethodAttributes = method.GetCustomAttributes<HttpMethodAttribute>();
      var webSocketMethodAttributes = method.GetCustomAttributes<WebSocketAttribute>(false);
      if (httpMethodAttributes != null || webSocketMethodAttributes != null) // Listeners!
      {
        yield return new ControllerMethodInvoker(
          controller,
          httpMethodAttributes,
          webSocketMethodAttributes,
          method
        );
      }
    }
  }

  private static void AddControllerListener(
    IRouter router,
    string route,
    ControllerMethodInvoker invoker
  )
  {
    route = '/' + route.Trim('/');

    // HTTP requests
    foreach (var httpMethodAttribute in invoker.HttpMethodAttributes)
    {
      string methodRoute = route + GetMethodRoute(invoker.MethodInfo, httpMethodAttribute);

      foreach (var httpMethod in httpMethodAttribute.HttpMethods)
      {
        switch (httpMethod)
        {
          case HttpVerb.Any:
            router.Any(methodRoute, invoker.CompileRequestInvoker(route));
            break;

          case HttpVerb.Get:
            router.Get(methodRoute, invoker.CompileRequestInvoker(route));
            break;
          case HttpVerb.Post:
            router.Post(methodRoute, invoker.CompileRequestInvoker(route));
            break;
          case HttpVerb.Patch:
            router.Patch(methodRoute, invoker.CompileRequestInvoker(route));
            break;
          case HttpVerb.Put:
            router.Put(methodRoute, invoker.CompileRequestInvoker(route));
            break;
          case HttpVerb.Delete:
            router.Delete(methodRoute, invoker.CompileRequestInvoker(route));
            break;
          case HttpVerb.Trace:
            router.Trace(methodRoute, invoker.CompileRequestInvoker(route));
            break;
          case HttpVerb.Head:
            router.Head(methodRoute, invoker.CompileRequestInvoker(route));
            break;
          case HttpVerb.Connect:
            router.Connect(methodRoute, invoker.CompileRequestInvoker(route));
            break;
          case HttpVerb.Options:
            router.Options(methodRoute, invoker.CompileRequestInvoker(route));
            break;

          default: // ? what
            CommunicationsRuntime.Current.RuntimeLog.Alert(
              $"{typeof(RouteControllerInitializer)}::{nameof(AddControllerListener)}",
              $"Unknown HttpMethod was attempted to be added via a {typeof(HttpMethodAttribute).FullName}! Method: {httpMethod}. "
                + $"Defaulting to {HttpVerb.Any}."
            );
            router.Any(methodRoute, invoker.CompileRequestInvoker(route));
            break;
        }
      }
    }

    // WebSockets
    foreach (var webSocket in invoker.WebSocketAttributes)
    {
      string methodRoute = route + GetMethodRoute(invoker.MethodInfo, webSocket);
      router.WebSocket(methodRoute, invoker.CompileWebSocketInvoker(route));
    }
  }

  private static string GetControllerRoute(Type controller)
  {
    string route = '/' + controller.Name.ToLower();
    var controllerAttribute = controller.GetCustomAttribute<ControllerAttribute>(false);
    if (controllerAttribute != null && controllerAttribute.Template != null)
    {
      route = controllerAttribute.Template.Replace("[controller]", controller.Name.ToLower());
      if (!route.StartsWith('/'))
      {
        route = '/' + route;
      }

      return route;
    }

    var routeAttribute = controller.GetCustomAttribute<RouteAttribute>(false);
    if (routeAttribute == null)
    {
      return route; // none defined, use controller name.
    }

    route = routeAttribute.Template.Replace("[controller]", controller.Name.ToLower());
    if (!route.StartsWith('/'))
    {
      route = '/' + route;
    }

    route = route.TrimEnd('/');
    return route;
  }

  private static string GetMethodRoute(MethodInfo method, IRouteTemplate routeAttribute)
  {
    try
    {
      if (routeAttribute == null)
      {
        return string.Empty; //'/' + method.Name.ToLower();
      }

      var route = routeAttribute.Template ?? string.Empty;
      if (!route.StartsWith('/'))
      {
        route = '/' + route;
      }

      route = route.TrimEnd('/');
      return route;
    }
    catch (Exception e)
    {
      CommunicationsRuntime.Current.RuntimeLog.Error(
        $"{typeof(Router).FullName}::{nameof(GetMethodRoute)}({method}, {routeAttribute})",
        "Failed to get method route!",
        e
      );
      return string.Empty;
    }
  }
}
