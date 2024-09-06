namespace CNCO.Unify.Communications.Http.Routing {
    /// <summary>
    /// Controller context for an incoming request.
    /// </summary>
    public class ControllerContext : IControllerContext {
        private readonly object _lockObject = new object();

        private IWebRequest? _webRequest;
        private IWebResponse? _webResponse;

        public IWebRequest WebRequest {
            get {
                if (_webRequest == null) {
                    lock (_lockObject) {
                        _webRequest ??= new WebRequest();
                    }
                }
                return _webRequest;
            }

            /*set {
                ArgumentNullException.ThrowIfNull(value);
                _webRequest = value;
            }*/
        }

        public IWebResponse WebResponse {
            get {
                if (_webResponse == null) {
                    lock (_lockObject) {
                        _webResponse ??= new WebResponse();
                    }
                }
                return _webResponse;
            }

            /*set {
                ArgumentNullException.ThrowIfNull(value);
                _webResponse = value;
            }*/
        }


        public ControllerContext() { }

        public ControllerContext(IWebRequest WebRequest, IWebResponse WebResponse) {
            ArgumentNullException.ThrowIfNull(WebRequest);
            ArgumentNullException.ThrowIfNull(WebResponse);
            _webRequest = WebRequest;
            _webResponse = WebResponse;
        }
    }
}
