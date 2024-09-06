namespace CNCO.Unify.Communications.Http.Routing {
    [Controller]
    public abstract class Controller {
        private readonly object _lockObject = new object();

        private IControllerContext? _controllerContext;

        public IWebRequest Request => Context.WebRequest;
        public IWebResponse Response => Context.WebResponse;


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
