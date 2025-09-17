using CNCO.Unify.Logging;

namespace CNCO.Unify.Notifications.Windows {
    public class WindowsNotificationsRuntime : Runtime, IRuntime {
        private static WindowsNotificationsRuntime? _instance;
        private ILogger? _runtimeLog;

        #region Locks
        // Lock used when initializing this class.
        private static new readonly object _initializationLock = new object();
        #endregion

        internal new ILogger RuntimeLog {
            get => _runtimeLog ?? base.RuntimeLog;
            set => _runtimeLog = value;
        }

        public static WindowsNotificationsRuntime Current {
            get {
                if (_instance == null) { // Null?
                    lock (_initializationLock) { // Should only hit one time.
                        _instance ??= new WindowsNotificationsRuntime();
                    }
                }
                return _instance;
            }
        }


        public WindowsNotificationsRuntime(ILogger? runtimeLogger = null) {
            if (_instance != null)
                return;

            lock (_initializationLock) {
                if (_instance != null)
                    return;
                _instance = this;
            }

            if (runtimeLogger != null)
                _runtimeLog = runtimeLogger;
        }
    }
}
