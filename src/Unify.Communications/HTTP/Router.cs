using CNCO.Unify.Communications.Http.Routing;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CNCO.Unify.Communications.Http {
    /// <summary>
    /// Routes incoming requests.
    /// </summary>
    public class Router : IRouter {
        private bool _log = true;

        public Router(bool initializeRoutes = true) {
            if (initializeRoutes) {
                InitializeRoutes();
            }
        }

        public void SetLogging(bool enabled) => _log = enabled;

        #region Listeners
        private sealed class Listener {
            public HttpVerb Verb { get; private set; }
            public string Path { get; private set; }
            public string PathRegex { get; private set; }
            public Action<IWebRequest, IWebResponse>? OnWebRequest { get; private set; }
            public Action<IWebSocket>? OnWebSocketRequest { get; private set; }
            public Task? Task { get; set; }
            public bool IsWebSocket { get; set; } = false;

            private Listener(HttpVerb verb, string path) {
                Verb = verb;
                Path = '/' + path.Trim('/'); // force only one '/' at the start :)
                PathRegex = Regex.Replace(path, @":.*?:|\{.*?\}", ".*");
            }

            public Listener(HttpVerb verb, string path, Action<IWebRequest, IWebResponse> onWebRequest) : this(verb, path) => OnWebRequest = onWebRequest;
            public Listener(string path, Action<IWebSocket> onWebSocketRequest) : this(HttpVerb.Any, path) {
                IsWebSocket = true;
                OnWebSocketRequest = onWebSocketRequest;
            }
        }

        private readonly Dictionary<string, List<Listener>> Listeners = new Dictionary<string, List<Listener>>();

        // Use this to go from paths to paths with parameter names (/my/path/to/blahBlah/doc -> /my/path/to/:docPath:/doc)
        private readonly Dictionary<string, string> PathRegexLookup = new Dictionary<string, string>();
        #endregion

        #region Listeners methods
        private void AddListener(Listener listener) {
            List<Listener>? currentListeners = null;
            if (Listeners.TryGetValue(listener.Path, out List<Listener>? value))
                currentListeners = value;

            currentListeners ??= new List<Listener>(1);
            currentListeners.Add(listener);
            Listeners[listener.Path] = currentListeners;

            if (
                (listener.Path != listener.PathRegex || listener.Path.EndsWith('*'))
                && !PathRegexLookup.ContainsKey(listener.PathRegex)
            ) {
                PathRegexLookup.Add(listener.PathRegex, listener.Path);
            }

            CommunicationsRuntime.Current.RuntimeLog.Verbose($"Added new {listener.Verb} route {listener.Path}");
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

        public void WebSocket(string path, Action<IWebSocket> callback) => AddListener(new Listener(path, callback));

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

            string requestPath = '/' + request.Path.Trim('/');
            Listeners.TryGetValue(requestPath, out List<Listener>? listenersForPath);

            // Find the listeners via regex?
            if (listenersForPath == null || listenersForPath?.Count == 0) {
                // yep, try to find any via regex
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
            cancellationTokenSource.CancelAfter(CommunicationsRuntime.Current.Configuration.RuntimeHttpConfiguration.RouterListenerResponseTimeoutMilliseconds);
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            // Activate listeners!
            bool hasActivatedWebSocket = false; // So we can throw a "hey, don't have HTTP listeners on the same route has WebSockets!" warning!
            bool hasActivatedWebSocketWarningEmitted = false; // ... but only once
            foreach (Listener listener in listenersForPath!) {
                try {
                    if (response.HasEnded) {
                        CommunicationsRuntime.Current.RuntimeLog.Verbose(
                            $"{GetType().Name}::{nameof(Process)}",
                             $"Error encountered calling listener for {listener.Verb} \"{listener.Path}\" as the response has ended."
                        );
                        continue;
                    }

                    if (listener.IsWebSocket) {
                        if (listener.OnWebSocketRequest == null) // What? How?
                            throw new NullReferenceException($"{nameof(listener.OnWebSocketRequest)} is null, no listener action to call!");

                        listener.Task = new Task(() => listener.OnWebSocketRequest(request.CreateWebSocketConnection()), TaskCreationOptions.LongRunning);
                        listener.Task.Start();
                        listenerFired = true;
                        hasActivatedWebSocket = true;

                    } else if (listener.Verb == HttpVerb.Any || listener.Verb == request.Verb) {
                        if (listener.OnWebRequest == null) // What? How?
                            throw new NullReferenceException($"{nameof(listener.OnWebRequest)} is null, no listener action to call!");

                        if (hasActivatedWebSocket && !hasActivatedWebSocketWarningEmitted) {
                            hasActivatedWebSocketWarningEmitted = true;
                            CommunicationsRuntime.Current.RuntimeLog.Warning(
                                $"{GetType().Name}::{nameof(Process)}",
                                "You have a HTTP listener on the same route as a WebSocket handler! You cannot do this as once the HTTP response closes, the WebSocket will close."
                                + Environment.NewLine + "You have been warned."
                            );
                        }

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

            // Wait for a response ...
            foreach (Listener listener in listenersForPath) {
                try {
                    //if (listener.IsWebSocket)
                    //    continue; // never mind here!

                    if (listener.IsWebSocket || listener.Verb == HttpVerb.Any || listener.Verb == request.Verb) {
                        listener.Task?.Wait(cancellationToken); // it should be cancelling, but ..

                        int attempt = 0;
                        while (!(listener.Task?.IsCompleted ?? false) && attempt < 25) {
                            Thread.Sleep(25);
                            attempt++;
                        }

                        try {
                            listener.Task?.Dispose();
                        } catch {
                            CommunicationsRuntime.Current.RuntimeLog.Warning($"{GetType().Name}::{nameof(Process)}", $"Listener task hang for {request.Path}!");
                        }
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
                if (request.WebSocket != null && request.WebSocket.IsOpen) {
                    // You cannot close the response stream, that would terminate the WebSocket connection!
                    return;
                }

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

        private static IEnumerable<Type> GetControllers() {
            return from assemblies in AppDomain.CurrentDomain.GetAssemblies()
                   from types in assemblies.GetTypes()
                   where types.IsDefined(typeof(ControllerAttribute), true)
                   select types;
        }

        private static string GetControllerRoute(Type controller) {
            string route = '/' + controller.Name.ToLower();
            var controllerAttribute = controller.GetCustomAttribute<ControllerAttribute>(false);
            if (controllerAttribute != null && controllerAttribute.Template != null) {
                route = controllerAttribute.Template.Replace("[controller]", controller.Name.ToLower());
                if (!route.StartsWith('/'))
                    route = '/' + route;
                return route;
            }

            var routeAttribute = controller.GetCustomAttribute<RouteAttribute>(false);
            if (routeAttribute == null)
                return route; // none defined, use controller name.

            route = routeAttribute.Template.Replace("[controller]", controller.Name.ToLower());
            if (!route.StartsWith('/'))
                route = '/' + route;
            return route;
        }

        private static string GetMethodRoute(MethodInfo method, Type routeAttributeType) {
            try {
                IRouteTemplate? methodAttribute = (IRouteTemplate)method.GetCustomAttributes(routeAttributeType, false)[0];

                if (methodAttribute == null)
                    return string.Empty; //'/' + method.Name.ToLower();

                var route = methodAttribute.Template ?? string.Empty;
                if (!route.StartsWith('/'))
                    route = '/' + route;

                return route;
            } catch (Exception e) {
                CommunicationsRuntime.Current.RuntimeLog.Error(
                    $"{typeof(Router).FullName}::{nameof(GetMethodRoute)}({method}, {routeAttributeType})",
                    e.Message + Environment.NewLine + e.StackTrace
                );
                return string.Empty;
            }
        }

        private static List<ControllerInvoker> GetMethods(Type controller) {
            var controllerMethods = new List<ControllerInvoker>();
            var methods = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var method in methods) {
                var httpMethodAttributes = method.GetCustomAttributes<HttpMethodAttribute>();
                var webSocketMethodAttributes = method.GetCustomAttributes<WebSocketAttribute>(false);
                if (httpMethodAttributes == null && webSocketMethodAttributes == null) // No listeners here!
                    continue;

                controllerMethods.Add(new(controller, httpMethodAttributes, webSocketMethodAttributes, method));
            }

            return controllerMethods;
        }

        private void AddListener(string route, ControllerInvoker method) {
            Action<IWebRequest, IWebResponse> httpCallback = (req, res) => method.Invoke(req, res, null);
            Action<IWebSocket> webSocketCallback = (socket) => method.Invoke(socket.WebRequest, null, socket);

            route = '/' + route.Trim('/');

            // HTTP requests
            foreach (var httpMethodAttribute in method.HttpMethodAttributes) {
                string methodRoute = route + GetMethodRoute(method.MethodInfo, httpMethodAttribute.GetType()).TrimStart('/');

                foreach (var httpMethod in httpMethodAttribute.HttpMethods) {
                    switch (httpMethod) {
                        case HttpVerb.Get:
                            Get(methodRoute, httpCallback);
                            break;
                        case HttpVerb.Post:
                            Post(methodRoute, httpCallback);
                            break;
                        case HttpVerb.Patch:
                            Patch(methodRoute, httpCallback);
                            break;
                        case HttpVerb.Put:
                            Put(methodRoute, httpCallback);
                            break;
                        case HttpVerb.Delete:
                            Delete(methodRoute, httpCallback);
                            break;
                        case HttpVerb.Trace:
                            Trace(methodRoute, httpCallback);
                            break;
                        case HttpVerb.Head:
                            Head(methodRoute, httpCallback);
                            break;
                        case HttpVerb.Connect:
                            Connect(methodRoute, httpCallback);
                            break;
                        case HttpVerb.Options:
                            Options(methodRoute, httpCallback);
                            break;

                        default: // ? what
                            CommunicationsRuntime.Current.RuntimeLog.Alert(
                                $"{GetType()}::{nameof(AddListener)}",
                                $"Unknown HttpMethod was attempted to be added via a {typeof(HttpMethodAttribute).FullName}! Method: {httpMethod}. " +
                                $"Defaulting to {HttpVerb.Any}."
                            );
                            All(methodRoute, httpCallback);
                            break;
                    }
                }
            }

            // WebSockets
            foreach (var webSocket in method.WebSocketAttributes) {
                string methodRoute = route + GetMethodRoute(method.MethodInfo, typeof(WebSocketAttribute));
                WebSocket(methodRoute, webSocketCallback);
            }
        }
        #endregion
    }
}
