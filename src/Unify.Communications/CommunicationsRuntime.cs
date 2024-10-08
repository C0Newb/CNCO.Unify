﻿using CNCO.Unify.Communications.Http;
using CNCO.Unify.Logging;

namespace CNCO.Unify.Communications {
    [LinkRuntime(typeof(UnifyRuntime))]
    public class CommunicationsRuntime : Runtime, IRuntime {
        private static CommunicationsRuntime? _instance;

        #region Locks
        // Lock used when initializing this class.
        private static new readonly object _initializationLock = new object();
        #endregion

        internal new ILogger RuntimeLog {
            get => base.RuntimeLog;
        }

        private static IWebClient? _webClient;

        public CommunicationsRuntimeConfiguration Configuration { get; }

        /// <summary>
        /// Application wide, shared <see cref="IWebClient"/>.
        /// </summary>
        public static IWebClient WebClient {
            get {
                if (_webClient == null) {
                    lock (_initializationLock) {
                        _webClient ??= new WebClient();
                    }
                }
                return _webClient;
            }
        }

        public static CommunicationsRuntime Current {
            get {
                if (_instance == null) { // Null?
                    lock (_initializationLock) { // Should only hit one time.
                        _instance ??= new CommunicationsRuntime();
                    }
                }
                return _instance;
            }
        }

        public CommunicationsRuntime() : this(null) { }

        public CommunicationsRuntime(CommunicationsRuntimeConfiguration? runtimeConfiguration) {
            Configuration = runtimeConfiguration ?? new CommunicationsRuntimeConfiguration();

            if (_instance != null)
                return;

            lock (_initializationLock) {
                if (_instance != null)
                    return;
                _instance = this;
            }
        }

        public static CommunicationsRuntime Create(CommunicationsRuntimeConfiguration? runtimeConfiguration)
            => new CommunicationsRuntime(runtimeConfiguration);
    }
}
