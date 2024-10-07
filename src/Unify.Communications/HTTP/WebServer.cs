using System.Net;

namespace CNCO.Unify.Communications.Http {
    public class WebServer : IWebServer {
        private readonly HttpListener _httpListener;
        private readonly Thread _listenerThread;
        private int _listenerThreadRestart = 0;
        private bool _runListenerThread = true;
        private IRouter? _router;

        private IRouter Router {
            get {
                _router ??= new Router();
                return _router;
            }
        }


        private bool _logAccess = false;

        public WebServer() {
            _httpListener = new HttpListener();

            _listenerThread = new Thread(() => {
                string tag = $"{GetType().Name}::${nameof(_listenerThread)}";
                while (_httpListener.IsListening && _runListenerThread) {
                    try {
                        // listen
                        var context = _httpListener.GetContext();
                        Task.Run(() => HandleRequest(context));

                    } catch (Exception ex) {
                        if (!_runListenerThread) {
                            CommunicationsRuntime.Current.RuntimeLog.Info(tag, "Listener thread stopped.");
                            return;
                        }

                        CommunicationsRuntime.Current.RuntimeLog.Alert(tag,
                            $"HTTP webserver listener thread exception: {ex.Message}"
                            + Environment.NewLine + "\t"
                            + ex.StackTrace ?? "No stack trace available.");

                        // restart the thread
                        _listenerThreadRestart++;
                        if (_listenerThreadRestart > 5) {
                            CommunicationsRuntime.Current.RuntimeLog.Emergency($"Listener thread has restarted too many times ({_listenerThreadRestart}). HTTP listener is disabled until the application restarts."); // sorta a lie.. but eh
                            break;
                        }

                        Thread.Sleep(1000 * _listenerThreadRestart);
                        CommunicationsRuntime.Current.RuntimeLog.Warning($"Attempting listener thread restart #{_listenerThreadRestart}"); // sorta a lie.. but eh
                    }
                }
            }) {
                Name = UnifyRuntime.Current.ApplicationId + "-WebServer#" + GetHashCode()
            };
        }

        public WebServer(WebServerOptions options) : this() {
            SetOptions(options);
        }
        public WebServer(IRouter router) : this() => _router = router;
        public WebServer(IRouter router, WebServerOptions options) : this(router) {
            SetOptions(options);
        }

        private void SetOptions(WebServerOptions options) {
            if (options.Endpoints != null) {
                foreach (var endpoint in options.Endpoints) {
                    Listen(endpoint);
                }
            }

            _logAccess = options.LogAccess;
            Router?.SetLogging(_logAccess);
        }

        /// <summary>
        /// Adds an endpoint to be listening on. Can only be done if the server is stopped.
        /// </summary>
        /// <param name="endpoint">Address to be listening on, such as <c>http://localhost:8008</c>.</param>
        public void Listen(string endpoint) {
            if (!endpoint.StartsWith("http://") && !endpoint.StartsWith("https://")) {
                endpoint = "http://" + endpoint;
            }
            if (!endpoint.EndsWith('/'))
                endpoint += "/";

            _httpListener.Prefixes.Add(endpoint);
        }

        public string[] GetEndpoints() {
            return _httpListener.Prefixes.ToArray();
        }

        #region Router method proxies
        public void All(string path, Action<IWebRequest, IWebResponse> callback) => Router!.All(path, callback);

        public void Connect(string path, Action<IWebRequest, IWebResponse> callback) => Router!.Connect(path, callback);

        public void Delete(string path, Action<IWebRequest, IWebResponse> callback) => Router!.Delete(path, callback);

        public void Get(string path, Action<IWebRequest, IWebResponse> callback) => Router!.Get(path, callback);

        public void Head(string path, Action<IWebRequest, IWebResponse> callback) => Router!.Head(path, callback);

        public void Options(string path, Action<IWebRequest, IWebResponse> callback) => Router!.Options(path, callback);

        public void Patch(string path, Action<IWebRequest, IWebResponse> callback) => Router!.Patch(path, callback);

        public void Post(string path, Action<IWebRequest, IWebResponse> callback) => Router!.Post(path, callback);

        public void Put(string path, Action<IWebRequest, IWebResponse> callback) => Router!.Put(path, callback);

        public void Trace(string path, Action<IWebRequest, IWebResponse> callback) => Router!.Trace(path, callback);
        #endregion

        private void HandleRequest(HttpListenerContext context) {
            try {
                if (Router == null)
                    return;

                try {
                    var defaultHeaders = CommunicationsRuntime.Current.Configuration.RuntimeHttpConfiguration.DefaultWebServerResponseHeaders;
                    if (defaultHeaders != null) {
                        foreach (var header in defaultHeaders.AllKeys) {
                            if (defaultHeaders[header] == null)
                                continue;

                            context.Response.Headers[header] = defaultHeaders[header];
                        }
                    }
                } catch (Exception ex) {
                    // Failed to apply default headers
                    CommunicationsRuntime.Current.RuntimeLog.Warning(
                        $"{GetType().Name}::{nameof(HandleRequest)}",
                        "Failed to apply default headers. Error: " +
                        ex.Message +
                        "Stack: " + ex.StackTrace
                     );
                }

                WebRequest request = new WebRequest(context);
                WebResponse response = new WebResponse(context.Response);

                if (_logAccess)
                    CommunicationsRuntime.Current.RuntimeLog.Debug($"{GetType().Name}::{nameof(HandleRequest)}", $"HTTP-{request.Verb} {request.Path}");

                Router.Process(request, response);

            } catch (Exception e) {
                string path = "UNKNOWN";
                try {
                    path = context.Request.Url?.AbsolutePath ?? "NULL";
                    if (context.Response != null) {
                        context.Response.StatusCode = 500;
                        context.Response.Close();
                    }
                } catch { }
                string tag = $"{GetType().Name}::{nameof(HandleRequest)}";

                CommunicationsRuntime.Current.RuntimeLog.Error(tag, $"Failed to process HTTP request {path}: {e}");
                if (!string.IsNullOrEmpty(e.StackTrace))
                    CommunicationsRuntime.Current.RuntimeLog.Error(tag, e.StackTrace);
            }
        }

        public void Abort() {
            _runListenerThread = false;
            _httpListener.Abort();
        }
        public void Dispose() {
            string tag = $"{GetType().Name}::{nameof(Dispose)}";
            try {
                _runListenerThread = false;
                _httpListener.Stop();
                _httpListener.Close();
            } catch (Exception e) {
                CommunicationsRuntime.Current.RuntimeLog.Error(tag, $"Error while disposing! {e.Message}");
                if (!string.IsNullOrEmpty(e.StackTrace))
                    CommunicationsRuntime.Current.RuntimeLog.Error(tag, e.StackTrace);
            }
            GC.SuppressFinalize(this);
        }

        public void Listen(Uri uri) {
            _httpListener.Prefixes.Add(uri.ToString());
        }

        public void Start() {
            _runListenerThread = true;
            _httpListener.Start();
            _listenerThread.Start();
        }

        public void Stop() {
            _runListenerThread = false;
            _httpListener.Stop();
        }

        public bool Running() {
            return _runListenerThread && _httpListener.IsListening;
        }

        public void Use(IRouter router) => _router = router;
    }
}
