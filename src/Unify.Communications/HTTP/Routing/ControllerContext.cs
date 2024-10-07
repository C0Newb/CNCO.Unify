namespace CNCO.Unify.Communications.Http.Routing {
    /// <summary>
    /// Controller context for an incoming request.
    /// </summary>
    public class ControllerContext : IControllerContext {
        private readonly object _lockObject = new object();

        private IWebRequest? _webRequest;

        public IWebRequest WebRequest {
            get {
                if (_webRequest == null) {
                    lock (_lockObject) {
                        _webRequest ??= new WebRequest();
                    }
                }
                return _webRequest;
            }
        }

        public IWebResponse? WebResponse { get; }

        public IWebSocket? WebSocket { get; }

        public ControllerContext() { }

        public ControllerContext(IWebRequest webRequest, IWebResponse? webResponse, IWebSocket? webSocket) {
            ArgumentNullException.ThrowIfNull(webRequest);
            if (webSocket == null)
                ArgumentNullException.ThrowIfNull(WebResponse);

            _webRequest = webRequest;
            WebResponse = webResponse;
            WebSocket = webSocket;
        }
    }
}
