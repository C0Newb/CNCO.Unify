namespace CNCO.Unify.Communications.Http.Routing {
    [Controller]
    public abstract class Controller {
        private readonly object _lockObject = new object();

        private ControllerContext? _controllerContext;

        public WebRequest Request => Context.WebRequest;
        public WebResponse Response => Context.WebResponse;


        public ControllerContext Context {
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
