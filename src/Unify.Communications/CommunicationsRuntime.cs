using CNCO.Unify.Communications.Http;
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

        private static IWebClient? _webClient;
        public static IWebClient WebClient {
            get {
                _webClient ??= new WebClient();
                return _webClient;
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
