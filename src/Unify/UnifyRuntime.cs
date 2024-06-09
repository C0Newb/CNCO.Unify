using CNCO.Unify.Events;
using CNCO.Unify.Logging;
using CNCO.Unify.Storage;
using System.Reflection;

namespace CNCO.Unify {
    /// <summary>
    /// The main entry point and setup class for Unify. Similar to <c>Program.cs</c>.
    /// </summary>
    public sealed class UnifyRuntime : Runtime, IRuntime {
        private static UnifyRuntime? _instance;
        private EventEmitter? _eventEmitter;
        private SinkLogger? _applicationLog;

        #region Locks
        // Lock used when initializing this class.
        private static new readonly object _initializationLock = new object();
        #endregion


        #region Properties
        /// <summary>
        /// Internal <see cref="ILogger"/> for this runtime.
        /// </summary>
        internal new ILogger RuntimeLog {
            get => base.RuntimeLog;
        }

        /// <summary>
        /// Your application's id.
        /// </summary>
        public string ApplicationId { get; private set; }

        /// <summary>
        /// Current <see cref="UnifyRuntimeConfiguration"/>.
        /// </summary>
        public UnifyRuntimeConfiguration Configuration { get; private set; } = new UnifyRuntimeConfiguration();


        /// <summary>
        /// Unify's global <see cref="EventEmitter"/>.
        /// </summary>
        public static EventEmitter EventEmitter {
            get {
                Current._eventEmitter ??= new EventEmitter();
                return Current._eventEmitter;
            }
        }

        /// <summary>
        /// Application log. Do not use this until you've set the <see cref="ApplicationId"/>!
        /// </summary>
        public static Logger ApplicationLog {
            get {
                if (Current._applicationLog == null) {
                    Current._applicationLog = new SinkLogger();

                    // Add storage logger
                    IFileStorage? logFileStorage = Current.Configuration.ApplicationLogFileStorage;
                    if (logFileStorage == null && !Current.Configuration.ApplicationLogNoFileStorage) {
                        logFileStorage = new LocalFileStorage(Current.Configuration.ApplicationLogDirectory);
                    }
                    if (string.IsNullOrEmpty(Current.Configuration.ApplicationLogName)) {
                        Current.Configuration.ApplicationLogName = $"{Current.ApplicationId}.RuntimeLog.txt";
                    }
                    if (logFileStorage != null) {
                        logFileStorage.Delete(Current.Configuration.ApplicationLogName); // remove previous
                        var fileLogger = new FileLogger(logFileStorage, Current.Configuration.ApplicationLogName);
                        Current._applicationLog.AddLogger(fileLogger);
                    }

                    Current._applicationLog.AddLogger(new DiagnosticsLogger());

                    Current._applicationLog.Log("Unify", "Bonjour!"); // startup message
                }
                return Current._applicationLog;
            }
        }

        /// <summary>
        /// The current (static) application <see cref="UnifyRuntime"/>.
        /// </summary>
        public static UnifyRuntime Current {
            get {
                if (_instance == null) { // Null?
                    lock (_initializationLock) { // Should only hit one time.
                        _instance ??= new UnifyRuntime("Unify-Application"); // But let's be super, super sure!
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Constructors (private)
        private UnifyRuntime(string applicationId) : base() {
            lock (_initializationLock) {
                ApplicationId = applicationId;
                _instance = this;
            }
        }

        private UnifyRuntime(UnifyRuntimeConfiguration configuration) : base() {
            lock (_initializationLock) {
                ApplicationId = configuration.ApplicationId
                                ?? Assembly.GetExecutingAssembly().FullName
                                ?? Assembly.GetExecutingAssembly().GetName().FullName;
                Configuration = configuration;
                _instance = this;
            }
        }
        #endregion

        #region Initialization (create and initialize)
        public static UnifyRuntime Create(UnifyRuntimeConfiguration runtimeConfiguration) => new UnifyRuntime(runtimeConfiguration);

        /// <summary>
        /// Initializes a new instance of the <see cref="UnifyRuntime"/> class.
        /// Doing this sets <see cref="Current"/> to your new instance, creating the global instance.
        /// </summary>
        /// <remarks>
        /// If a global instance is already created, <see cref="Current"/>, this will return that instance.
        /// Use this to set the <see cref="ApplicationId"/> as probing <see cref="Current"/> automatically sets the application id to <c>Unify-Application</c>.
        /// </remarks>
        /// <param name="applicationId">Your application's id.</param>
        public static UnifyRuntime Create(string applicationId) => new UnifyRuntime(applicationId);

        /// <inheritdoc cref="Create(string)"/>
        /// <param name="runtimeConfiguration">Custom runtime configuration.</param>
        public static UnifyRuntime Create(string applicationId, UnifyRuntimeConfiguration runtimeConfiguration) {
            if (_instance != null)
                return _instance;
            lock (_initializationLock) {
                if (runtimeConfiguration != null) {
                    runtimeConfiguration.ApplicationId = applicationId;
                    _instance = new UnifyRuntime(runtimeConfiguration);
                } else {
                    _instance = new UnifyRuntime(applicationId);
                }
            }
            return _instance;
        }

        /// <summary>
        /// Initializes <see cref="Current"/> and any added <see cref="RuntimeHook"/>s.
        /// </summary>
        public override void Initialize() {
            // Add hooks from the configuration
            lock (_initializationLock) {
                _runtimeHooks.AddRange(Current.Configuration.Hooks);
            }

            // Initialize.
            base.Initialize();
        }
        #endregion
    }
}
