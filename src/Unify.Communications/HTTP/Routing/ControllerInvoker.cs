using System.Collections.Concurrent;
using System.Reflection;

namespace CNCO.Unify.Communications.Http.Routing {
    /// <summary>
    /// Used by <see cref="Router"/> to invoke a method within a <see cref="Controller"/>.
    /// </summary>
    internal class ControllerInvoker {
        // !! VERIFY this is ok to do !!
        private static readonly ConcurrentDictionary<Type, object> controllerInstancesCache = new ConcurrentDictionary<Type, object>();

        private Type ControllerType { get; }
        public IEnumerable<HttpMethodAttribute> HttpMethodAttributes { get; }
        public IEnumerable<WebSocketAttribute> WebSocketAttributes { get; }
        public MethodInfo MethodInfo { get; }

        public ControllerInvoker(Type controller, IEnumerable<HttpMethodAttribute>? httpMethodAttributes, IEnumerable<WebSocketAttribute>? webSocketAttributes, MethodInfo methodInfo) {
            ControllerType = controller;
            HttpMethodAttributes = httpMethodAttributes?.Where(x => x != null) ?? [];
            WebSocketAttributes = webSocketAttributes?.Where(x => x != null) ?? [];
            MethodInfo = methodInfo;
        }

        /// <summary>
        /// Invokes the controller's method
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <exception cref="TargetParameterCountException"></exception>
        public void Invoke(IWebRequest request, IWebResponse? response, IWebSocket? webSocket) {
            ControllerContext controllerContext = new ControllerContext(request, response, webSocket);

            // Get controller class instance
            if (controllerInstancesCache.TryGetValue(ControllerType, out object? classInstance) || classInstance == null) {
                // Create new instance
                classInstance = Activator.CreateInstance(ControllerType);
                if (classInstance != null) // we'll throw the error later
                    controllerInstancesCache.TryAdd(ControllerType, classInstance);
            }

            if (classInstance != null && classInstance is Controller controllerInstance) {
                controllerInstance.Context = controllerContext;
            } else {
                CommunicationsRuntime.Current.RuntimeLog.Warning(
                    GetTag(nameof(Invoke)),
                    $"{nameof(classInstance)} could not be casted to type ${ControllerType.Name}! " +
                    $"{nameof(classInstance)} type? {classInstance?.GetType().FullName} - base? {classInstance?.GetType().BaseType?.FullName ?? "UNKNOWN"}"
                );
                return;
            }

            // Process parameters
            var methodParameters = MethodInfo.GetParameters();
            if (methodParameters.Length == 0) { // Ayy, no parameters!
                MethodInfo.Invoke(classInstance, []);
                return;
            }

            if (methodParameters.Length != request.RouteTemplate?.RouteParameters.Count()) {
                // ? what
                CommunicationsRuntime.Current.RuntimeLog.Alert(
                    "Router::ControllerMethod::Callback",
                    $"{nameof(classInstance)}::{MethodInfo.Name}() has an incorrect parameter count for the current request template! " +
                    $"Request has {request.RouteTemplate?.RouteParameters.Count() ?? 0} parameters, but the method requires {methodParameters.Length}."
                );
                throw new TargetParameterCountException($"Request has {request.RouteTemplate?.RouteParameters.Count() ?? 0} parameters, but the method requires {methodParameters.Length}.");
            }

            // try to inject the parameters :)
            var parameters = new List<object?>(methodParameters.Length);
            for (int i = 0; i < methodParameters.Length; i++) {
                var parameter = request.RouteTemplate?.RouteParameters.ElementAt(i);

                if (parameter == null) {
                    parameters.Add(null);
                    continue;
                }

                if (methodParameters[i].ParameterType == typeof(string)) {
                    parameters.Add(parameter?.ToStringValue());
                    continue;
                }

                object? value = parameter?.Value ?? null;
                if (parameter?.Type != methodParameters[i].ParameterType) {
                    // _try_ casting
                    try {
                        value = Convert.ChangeType(parameter?.Value, methodParameters[i].ParameterType);
                    } catch (Exception e) {
                        // nope
                        string methodTag = $"{ControllerType.FullName}::{MethodInfo.Name}({string.Join(", ", methodParameters.Select(x => x.ParameterType))})";
                        CommunicationsRuntime.Current.RuntimeLog.Warning(
                            "Router::ControllerMethod::Callback",
                            $"Cannot call {methodTag}) as the request parameter type {parameter?.Type.FullName ?? "NULL_PARAMETER"} " +
                            $"does not match the method parameter type {methodParameters[i].ParameterType.FullName}! " +
                            $"An attempt to convert via Convert.ChangeType() failed with the following message: {e.Message}"
                        );
                        return;
                    }
                }

                parameters.Add(value);
            }

            // Call the controller's method
            MethodInfo.Invoke(classInstance, parameters.ToArray());
        }

        private string GetTag(string method) => $"{nameof(ControllerInvoker)}({ControllerType.Name})::{method}()";
    }
}
