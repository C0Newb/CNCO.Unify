namespace CNCO.Unify.Communications.Http.Routing {
    /// <summary>
    /// Controller context for an incoming request.
    /// </summary>
    public class ControllerContext {
        private readonly object _lockObject = new object();

        private WebRequest? _webRequest;
        private WebResponse? _webResponse;

        /// <summary>
        /// The incoming web request.
        /// </summary>
        public WebRequest WebRequest {
            get {
                if (_webRequest == null) {
                    lock (_lockObject) {
                        _webRequest ??= new WebRequest();
                    }
                }
                return _webRequest;
            }

            set {
                ArgumentNullException.ThrowIfNull(value);
                _webRequest = value;
            }
        }

        /// <summary>
        /// The outgoing web response.
        /// </summary>
        public WebResponse WebResponse {
            get {
                if (_webResponse == null) {
                    lock (_lockObject) {
                        _webResponse ??= new WebResponse();
                    }
                }
                return _webResponse;
            }

            set {
                ArgumentNullException.ThrowIfNull(value);
                _webResponse = value;
            }
        }


        public ControllerContext() { }

        public ControllerContext(WebRequest WebRequest, WebResponse WebResponse) {
            ArgumentNullException.ThrowIfNull(WebRequest);
            ArgumentNullException.ThrowIfNull(WebResponse);
            _webRequest = WebRequest;
            _webResponse = WebResponse;
        }
    }
}
