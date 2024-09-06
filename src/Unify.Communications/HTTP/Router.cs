using CNCO.Unify.Communications.Http.Routing;
using System.Collections.Specialized;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CNCO.Unify.Communications.Http {
    /// <summary>
    /// Routes incoming requests.
    /// </summary>
    public class Router : IRouter {
        private sealed class Listener {
            public HttpVerb Verb { get; private set; }
            public string Path { get; private set; }
            public string PathRegex { get; private set; }
            public Action<IWebRequest, IWebResponse> OnWebRequest { get; private set; }
            public Task? Task { get; set; }

            public Listener(HttpVerb verb, string path, Action<IWebRequest, IWebResponse> onWebRequest) {
                path = '/' + path.Trim('/'); // force only one '/' at the start :)

                Verb = verb;
                Path = path;
                PathRegex = Regex.Replace(path, @":.*?:|\{.*?\}", ".*");
                OnWebRequest = onWebRequest;
            }
        }

        private readonly Dictionary<string, List<Listener>> Listeners = new Dictionary<string, List<Listener>>();

        // Use this to go from paths to paths with parameter names (/my/path/to/blahBlah/doc -> /my/path/to/:docPath:/doc)
        private readonly Dictionary<string, string> PathRegexLookup = new Dictionary<string, string>();


        private bool _log = true;

        public Router(bool initializeRoutes = true) {
            if (initializeRoutes) {
                InitializeRoutes();
            }
        }

        public void SetLogging(bool enabled) => _log = enabled;

        #region Listeners
        private void AddListener(Listener listener) {
            List<Listener>? currentListeners = null;
            if (Listeners.TryGetValue(listener.Path, out List<Listener>? value))
                currentListeners = value;

            currentListeners ??= new List<Listener>(1);
            currentListeners.Add(listener);
            Listeners[listener.Path] = currentListeners;

            if (
                listener.Path != listener.PathRegex &&
                !PathRegexLookup.ContainsKey(listener.PathRegex)
            ) {
                PathRegexLookup.Add(listener.PathRegex, listener.Path);
            }
        }

        public void All(string path, Action<IWebRequest, IWebResponse> callback) => AddListener(new Listener(HttpVerb.Any, path, callback));
        public void Connect(string path, Action<IWebRequest, IWebResponse> callback) => AddListener(new Listener(HttpVerb.Connect, path, callback));
        public void Delete(string path, Action<IWebRequest, IWebResponse> callback) => AddListener(new Listener(HttpVerb.Delete, path, callback));
        public void Get(string path, Action<IWebRequest, IWebResponse> callback) => AddListener(new Listener(HttpVerb.Get, path, callback));
        public void Head(string path, Action<IWebRequest, IWebResponse> callback) => AddListener(new Listener(HttpVerb.Head, path, callback));
        public void Options(string path, Action<IWebRequest, IWebResponse> callback) => AddListener(new Listener(HttpVerb.Options, path, callback));
        public void Patch(string path, Action<IWebRequest, IWebResponse> callback) => AddListener(new Listener(HttpVerb.Patch, path, callback));
        public void Post(string path, Action<IWebRequest, IWebResponse> callback) => AddListener(new Listener(HttpVerb.Post, path, callback));
        public void Put(string path, Action<IWebRequest, IWebResponse> callback) => AddListener(new Listener(HttpVerb.Put, path, callback));
        public void Trace(string path, Action<IWebRequest, IWebResponse> callback) => AddListener(new Listener(HttpVerb.Trace, path, callback));

        public void Remove(string path, Action<IWebRequest, IWebResponse>? callback, HttpVerb? httpVerb) {
            if (callback == null || !Listeners.TryGetValue(path, out List<Listener>? listeners)) {
                Listeners.Remove(path);
                return;
            }

            foreach (Listener listener in listeners) {
                if (listener.OnWebRequest == callback && (httpVerb == null || listener.Verb == httpVerb))
                    listeners.Remove(listener);
            }
            Listeners[path] = listeners;
        }
        #endregion

        /// <summary>
        /// Process an incoming request.
        /// </summary>
        /// <param name="request">Incoming request.</param>
        /// <param name="response">Incoming request's response.</param>
        public void Process(IWebRequest request, IWebResponse response) {
            if (string.IsNullOrEmpty(request.Path))
                return;

            Listeners.TryGetValue(request.Path.TrimEnd('/'), out List<Listener>? listenersForPath);

            if (listenersForPath == null || listenersForPath?.Count == 0) {
                // try to find it via regex
                foreach (var regexString in PathRegexLookup.Keys) {
                    Regex regex = new Regex(regexString);
                    var match = regex.Match(request.Path);
                    if (
                        !match.Success || // does not match
                        match.Captures[0].ToString().Split('/').Length != regexString.Split('/').Length // parameter count mismatch
                    ) {
                        continue;
                    }

                    // yep
                    var originalPath = PathRegexLookup[regexString];
                    Listeners.TryGetValue(originalPath, out List<Listener>? listenersForRegexPath);
                    if (listenersForRegexPath != null && listenersForRegexPath.Count > 0) {
                        listenersForPath = listenersForRegexPath;

                        request.RouteTemplate = new RouteTemplate(originalPath, request.Path);

                        break;
                    }
                }
            }

            if (listenersForPath == null || listenersForPath?.Count == 0) {
                response.Status(404);
                response.End();
                if (_log)
                    CommunicationsRuntime.Current.RuntimeLog.Warning($"{GetType().Name}::{nameof(Process)}", $"404: no listener found for path {request.Path}!");
                return;
            }

            bool listenerFired = false;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            foreach (Listener listener in listenersForPath!) {
                try {
                    if (listener.Verb == HttpVerb.Any || listener.Verb == request.Verb) {
                        // same path, same verb, send er...
                        listener.Task = new Task(() => listener.OnWebRequest(request, response), cancellationToken);
                        listener.Task.Start();
                        listenerFired = true;
                    }
                } catch (Exception ex) {
                    response.Status(500);
                    CommunicationsRuntime.Current.RuntimeLog.Error(
                        $"{GetType().Name}::{nameof(Process)}",
                        $"Error encountered calling listener for {listener.Verb} \"{listener.Path}\"" +
                        Environment.NewLine + "Error: " + ex.Message +
                        Environment.NewLine + "Stack: " + ex.StackTrace
                    );
                }
            }

            cancellationTokenSource.CancelAfter(CommunicationsRuntime.Current.Configuration.RuntimeHttpConfiguration.RouterListenerResponseTimeoutMilliseconds);
            foreach (Listener listener in listenersForPath) {
                try {
                    if (listener.Verb == HttpVerb.Any || listener.Verb == request.Verb) {
                        listener.Task?.Wait(cancellationToken); // it should be cancelling, but ..
                        listener.Task?.Dispose();
                    }
                } catch (OperationCanceledException) { }
            }

            if (!listenerFired) {
                response.Status(404);
                response.End();
                if (_log)
                    CommunicationsRuntime.Current.RuntimeLog.Warning($"{GetType().Name}::{nameof(Process)}", $"404: no listener found for path {request.Path}!");
                return;
            } else if (!response.HasEnded) {
                response.Status(CommunicationsRuntime.Current.Configuration.RuntimeHttpConfiguration.RouterNoResponseFromListenersStatusCode ?? 500);
                response.End();
                if (_log)
                    CommunicationsRuntime.Current.RuntimeLog.Warning($"{GetType().Name}::{nameof(Process)}", $"500: {listenersForPath?.Count ?? 0} listener(s) found for path {request.Path}, but none responded!");
            }
        }

        #region Route initialization
        /// <summary>
        /// Adds all routes in controllers to this router.
        /// </summary>
        public void InitializeRoutes() {
            IEnumerable<Type> controllers = GetControllers();

            foreach (var controller in controllers) {
                if (controller == typeof(Controller))
                    continue;

                string controllerRoute = GetControllerRoute(controller);
                var methods = GetMethods(controller);

                foreach (var method in methods) {
                    AddListener(controllerRoute, method);
                }
            }
        }

        private class ControllerMethod {
            private readonly Type Controller;

            public IEnumerable<HttpMethodAttribute> HttpMethodAttributes { get; }
            public MethodInfo MethodInfo { get; }

            public Action<IWebRequest, IWebResponse> Callback // this is what is called for this HttpMethodAttribute requests!
                => new Action<IWebRequest, IWebResponse>((request, response) => {
                    var controllerContext = new ControllerContext(request, response);
                    var classInstance = Activator.CreateInstance(Controller);
                    if (classInstance is Controller controllerInstance) {
                        // it should be ..
                        controllerInstance.Context = controllerContext;
                    } else {
                        CommunicationsRuntime.Current.RuntimeLog.Warning(
                            "Router::ControllerMethod::Callback",
                            $"{nameof(classInstance)} could not be casted to type ${typeof(Controller).Name}! " +
                            $"{nameof(classInstance)} type? {classInstance?.GetType().FullName} - base? {classInstance?.GetType().BaseType?.FullName ?? "UNKNOWN"}"
                        );
                    }

                    var methodParameters = MethodInfo.GetParameters();
                    if (methodParameters.Length == 0) {
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
                                string methodTag = $"{Controller.FullName}::{MethodInfo.Name}({string.Join(", ", methodParameters.Select(x => x.ParameterType))})";
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

                    MethodInfo.Invoke(classInstance, parameters.ToArray());
                });

            public ControllerMethod(Type controller, IEnumerable<HttpMethodAttribute> httpMethodAttributes, MethodInfo methodInfo) {
                Controller = controller;
                HttpMethodAttributes = httpMethodAttributes.Where(x => x != null);
                MethodInfo = methodInfo;
            }
        }
        private static IEnumerable<Type> GetControllers() {
            return from assemblies in AppDomain.CurrentDomain.GetAssemblies()
                   from types in assemblies.GetTypes()
                   where types.IsDefined(typeof(ControllerAttribute), true)
                   select types;
        }

        private static string GetControllerRoute(Type controller) {
            var routeAttribute = controller.GetCustomAttribute<RouteAttribute>(false);
            if (routeAttribute == null)
                return '/' + controller.Name.ToLower();
            var route = routeAttribute.Template.Replace("[controller]", controller.Name.ToLower());
            if (!route.StartsWith('/'))
                route = '/' + route;
            return route;
        }

        private static string GetMethodRoute(MethodInfo method, Type httpMethodType) {
            try {
                var httpMethodAttribute = (HttpMethodAttribute)method.GetCustomAttributes(httpMethodType, false)[0];
                if (httpMethodAttribute == null)
                    return string.Empty; //'/' + method.Name.ToLower();
                var route = httpMethodAttribute.Template ?? string.Empty;
                if (!route.StartsWith('/'))
                    route = '/' + route;
                return route;
            } catch (Exception e) {
                CommunicationsRuntime.Current.RuntimeLog.Error(
                    $"{typeof(Router).FullName}::{nameof(GetMethodRoute)}({method}, {httpMethodType})",
                    e.Message + Environment.NewLine + e.StackTrace
                );
                return string.Empty;
            }
        }

        private static IEnumerable<ControllerMethod> GetMethods(Type controller) {
            var controllerMethods = new List<ControllerMethod>();
            var methods = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var method in methods) {
                var httpMethodAttributes = method.GetCustomAttributes<HttpMethodAttribute>();
                if (httpMethodAttributes == null)
                    continue;

                controllerMethods.Add(new(controller, httpMethodAttributes, method));
            }

            return controllerMethods;
        }

        private void AddListener(string route, ControllerMethod method) {
            var callback = method.Callback;

            route = '/' + route.Trim('/');

            foreach (var httpMethodAttribute in method.HttpMethodAttributes) {
                string methodRoute = route + GetMethodRoute(method.MethodInfo, httpMethodAttribute.GetType());

                foreach (var httpMethod in httpMethodAttribute.HttpMethods) {
                    switch (httpMethod) {
                        case HttpVerb.Get:
                            Get(methodRoute, callback);
                            break;
                        case HttpVerb.Post:
                            Post(methodRoute, callback);
                            break;
                        case HttpVerb.Patch:
                            Patch(methodRoute, callback);
                            break;
                        case HttpVerb.Put:
                            Put(methodRoute, callback);
                            break;
                        case HttpVerb.Delete:
                            Delete(methodRoute, callback);
                            break;
                        case HttpVerb.Trace:
                            Trace(methodRoute, callback);
                            break;
                        case HttpVerb.Head:
                            Head(methodRoute, callback);
                            break;
                        case HttpVerb.Connect:
                            Connect(methodRoute, callback);
                            break;
                        case HttpVerb.Options:
                            Options(methodRoute, callback);
                            break;

                        default: // ? what
                            CommunicationsRuntime.Current.RuntimeLog.Alert(
                                $"{GetType()}::{nameof(AddListener)}",
                                $"Unknown HttpMethod was attempted to be added via a {typeof(HttpMethodAttribute).FullName}! Method: {httpMethod}. " +
                                $"Defaulting to {HttpVerb.Any}."
                            );
                            All(methodRoute, callback);
                            break;
                    }
                }
            }
        }
        #endregion
    }
}
