using CNCO.Unify.Logging;

namespace CNCO.Unify.Communications {
    public class CommunicationsRuntime : IRuntime {
        private static CommunicationsRuntime? _instance;
        private static ProxyLogger? _log;


        internal static ProxyLogger Log {
            get {
                _log ??= new ProxyLogger(Runtime.ApplicationLog, "Unify-Communications");
                return _log;
            }
        }

        public static CommunicationsRuntime Current {
            get {
                if (_instance == null) {
                    _instance = new CommunicationsRuntime();
                    Runtime.AddRuntimeLink(new RuntimeLink(_instance));
                }
                return _instance;
            }
        }

        public CommunicationsRuntime() { }
        public void Initialize() { }
    }
}
