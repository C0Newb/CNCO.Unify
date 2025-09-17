using CNCO.Unify.Logging;
using CNCO.Unify.Notifications.Push;
using CNCO.Unify.Storage;

namespace CNCO.Unify.Notifications {
    [LinkRuntime(typeof(UnifyRuntime))]
    public class NotificationRuntime : Runtime, IRuntime {
        private static NotificationRuntime? _instance;
        private static INotificationManager? _notificationManager;
        private static ILocalFileStorage? _imageFileStore;
        private static bool _hookedAdded = false;

        #region Locks
        // Lock used when initializing this class.
        private static new readonly object _initializationLock = new object();
        #endregion

        internal new ILogger RuntimeLog {
            get => base.RuntimeLog;
        }

        /// <summary>
        /// Current, shared <see cref="IPlatformNotificationManager"/>.
        /// </summary>
        public static INotificationManager NotificationManager {
            get {
                if (_notificationManager == null) {
                    lock (_initializationLock) {
                        _notificationManager ??= NotificationManagerFactory.GetPlatformNotificationManager();
                    }
                }
                return _notificationManager;
            }
        }

        public static ILocalFileStorage ImageFileStore {
            get {
                if (_imageFileStore == null) {
                    lock (_initializationLock) {
                        _imageFileStore ??= new LocalFileStorage(Path.Combine(UnifyRuntime.FileStorage.Directory, "notification_images"));
                    }
                }
                return _imageFileStore;
            }
        }

        public static NotificationRuntime Current {
            get {
                if (_instance == null) { // Null?
                    lock (_initializationLock) { // Should only hit one time.
                        _instance ??= new NotificationRuntime();
                    }
                }
                return _instance;
            }
        }


        public NotificationRuntime() {
            if (_instance != null)
                return;

            lock (_initializationLock) {
                if (_instance != null)
                    return;
                _instance = this;
            }

            // realistically this should be true if the instance is already created .. but double check! We really don't want to do this twice!
            if (_hookedAdded)
                return;

            lock (_initializationLock) {
                if (_hookedAdded)
                    return;

                // This will be ran once UnifyRuntime.Initialize() is invoked.
                var hookAction = new Action<UnifyRuntime>(runtimeTarget => {
                    try {
                        // need to do this somewhere .. guess that'll be here :)
                        NotificationManager.Register();
                    } catch (Exception ex) {
                        Current.RuntimeLog.Error(
                            $"{nameof(NotificationRuntime)}::{nameof(NotificationManager)}::{nameof(NotificationManager.Register)}()",
                            "Unable to register platform notification manager!",
                            ex
                        );
                    }
                });
                RuntimeHook notificationRegisterHook = new RuntimeHook($"{GetType().Name}${nameof(NotificationManager)}-Register", hookAction);
                AddHook(notificationRegisterHook);
                _hookedAdded = true;
            }
        }

        public static NotificationRuntime Create() => Current;
    }
}
