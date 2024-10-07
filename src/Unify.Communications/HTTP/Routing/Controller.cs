namespace CNCO.Unify.Communications.Http.Routing {
    [Controller]
    public abstract class Controller {
        private readonly object _lockObject = new object();

        private IControllerContext? _controllerContext;

        /// <inheritdoc cref="ControllerContext.WebRequest"/>
        public IWebRequest Request => Context.WebRequest;

        /// <inheritdoc cref="ControllerContext.WebResponse"/>
        public IWebResponse? Response => Context.WebResponse;

        /// <inheritdoc cref="ControllerContext.WebSocket"/>
        public IWebSocket? WebSocket => Context.WebSocket;


        public IControllerContext Context {
            get {
                if (_controllerContext == null) {
                    lock (_lockObject) {
                        _controllerContext ??= new ControllerContext();
                    }
                }
                return _controllerContext;
            }

            set {
                ArgumentNullException.ThrowIfNull(value);
                _controllerContext = value;
            }
        }
    }
}
