using CNCO.Unify.Events;
using CNCO.Unify.Logging;
using CNCO.Unify.Storage;

namespace CNCO.Unify {
    /// <summary>
    /// The main entry point and setup class for Unify. Similar to <c>Program.cs</c>.
    /// </summary>
    public class Runtime : IRuntime {
        private static Runtime? _instance;
        private readonly List<RuntimeHook> _hooks = new List<RuntimeHook>(0);
        private EventEmitter? _eventEmitter;
        private MultiLogger? _applicationLog;
        internal ProxyLogger? _unifyLog;
        private readonly List<RuntimeLink> _runtimeLinks = new List<RuntimeLink>(3);

        #region Properties
        internal ProxyLogger UnifyLog {
            get {
                _unifyLog ??= new ProxyLogger(Runtime.ApplicationLog, "Unify");
                return _unifyLog;
            }
        }

        public string ApplicationId;

        /// <summary>
        /// Current <see cref="RuntimeConfiguration"/>.
        /// </summary>
        public RuntimeConfiguration Configuration { get; private set; } = new RuntimeConfiguration();

        /// <summary>
        /// Whether <see cref="Current"/> has been initialized or not.
        /// </summary>
        public bool Initialized { get; private set; }

        /// <summary>
        /// The current (static) application <see cref="Runtime"/>.
        /// </summary>
        public static Runtime Current {
            get {
                _instance ??= new Runtime("Unify-Application");
                return _instance;
            }
        }


        public static EventEmitter EventEmitter {
            get {
                Current._eventEmitter ??= new EventEmitter();
                return Current._eventEmitter;
            }
        }


        /// <summary>
        /// Application log
        /// </summary>
        public static MultiLogger ApplicationLog {
            get {
                if (Current._applicationLog == null) {
                    Current._applicationLog = new MultiLogger();

                    // Add storage logger
                    IFileStorage? logFileStorage = Current.Configuration.ApplicationLogFileStorage;
                    if (logFileStorage == null && !Current.Configuration.ApplicationLogNoFileStorage) {
                        logFileStorage = new LocalFileStorage(Current.Configuration.ApplicationLogDirectory);
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
        #endregion

        public Runtime(string applicationId) {
            ApplicationId = applicationId;
            _instance = this;
        }

        public Runtime(string applicationId, RuntimeConfiguration configuration) {
            ApplicationId = applicationId;
            Configuration = configuration;
            _instance = this;
        }

        /// <summary>
        /// Adds a <see cref="RuntimeHook"/> to the list of current runtime hooks.
        /// Hook will not be added, nor called, if <see cref="Initialized"/> is <see langword="true"/>
        /// </summary>
        /// <param name="hook">Runtime hook to be added.</param>
        public static void AddHook(RuntimeHook hook) {
            if (!Current.Initialized && !Current._hooks.Contains(hook))
                Current._hooks.Add(hook);
        }

        // Runs a hook.
        private bool RunHook(RuntimeHook hook) {
            try {
                string nameTruncated = hook.Name[..Math.Min(hook.Name.Length, 75)];
                string? descriptionTruncated = hook.Description?[..Math.Min(hook.Name.Length, 250)];

                Current.UnifyLog.Debug($"Calling hook {nameTruncated}");
                if (!string.IsNullOrEmpty(hook.Description))
                    Current.UnifyLog.Debug($"{nameTruncated}: {descriptionTruncated}");

                hook.Action(this);
                return true;
            } catch (Exception e) {
                Current.UnifyLog.Error("Error calling the previous hook: ");
                Current.UnifyLog.Error(e.Message);

                if (e.StackTrace != null)
                    Current.UnifyLog.Debug(e.StackTrace);
                else
                    Current.UnifyLog.Debug("No stack trace to report.");

                return false;
            }
        }

        /// <summary>
        /// Initializes <see cref="Current"/>.
        /// Calls all <see cref="RuntimeHook"/>s added.
        /// </summary>
        public void Initialize() {
            if (Initialized)
                return;
            Initialized = true;

            _hooks.AddRange(Current.Configuration.Hooks);

            int successfulHooks = 0;
            _hooks.ForEach(x => {
                if (RunHook(x))
                    successfulHooks++;
            });
            int failedHooks = _hooks.Count - successfulHooks;
            _hooks.Clear();

            if (failedHooks == 0)
                Current.UnifyLog.Info("All hooks called, 0 failed.");
            else
                Current.UnifyLog.Warning($"All hooks called, {failedHooks} failed.");
        }


        // Check if x matches link
        private static bool RuntimeLinkPredicate(RuntimeLink x, RuntimeLink link) {
            return x.GetType().Name.Equals(link.GetType().Name)
                || x.Instance.Equals(link.Instance);
        }

        /// <summary>
        /// Checks whether a <see cref="RuntimeLink"/> has been added.
        /// </summary>
        /// <param name="link">Link to search for.</param>
        /// <returns>Whether <paramref name="link"/> has been added to the list of runtime links.</returns>
        public static bool ContainsRuntimeLink(RuntimeLink link) {
            return Current._runtimeLinks.Where(x => RuntimeLinkPredicate(x, link)).Any();
        }


        /// <summary>
        /// Adds a <see cref="RuntimeLink"/>, such as Unify.SecurityRuntime
        /// </summary>
        /// <param name="link">The unique Runtime to remove.</param>
        public static void AddRuntimeLink(RuntimeLink link) {
            if (ContainsRuntimeLink(link))
                Current._runtimeLinks.Add(link);
        }

        /// <summary>
        /// Removes a <see cref="RuntimeLink"/> from the list of links.
        /// </summary>
        /// <param name="link">The unique Runtime to remove.</param>
        public static void RemoveRuntimeLink(RuntimeLink link) {
            if (ContainsRuntimeLink(link))
                Current._runtimeLinks.RemoveAll(x => RuntimeLinkPredicate(x, link));
        }
    }

    /// <summary>
    /// Hook that can run when <see cref="Runtime.Initialize"/> is called.
    /// </summary>
    public class RuntimeHook {
        /// <summary>
        /// Name for the hook that is logged before being executed.
        /// Truncated to 75 characters.
        /// </summary>
        public string Name;

        /// <summary>
        /// Friendly description of the hook that is logged before being executed.
        /// Truncated to 250 characters.
        /// </summary>
        public string? Description;

        /// <summary>
        /// The hook code that will be executed.
        /// </summary>
        public Action<Runtime> Action;

        public RuntimeHook(string name, Action<Runtime> action) {
            Name = name;
            Action = action;
        }
    }



    /// <summary>
    /// Used to configure the <see cref="Runtime"/> via the constructor.
    /// </summary>
    public class RuntimeConfiguration {
        /// <summary>
        /// Name of the directory <see cref="ApplicationLogName"/> is saved to.
        /// </summary>
        public string ApplicationLogDirectory { get; set; } = string.Empty;
        /// <summary>
        /// The name of the application log file saved to <see cref="ApplicationLogFileStorage"/>.
        /// </summary>
        public string ApplicationLogName { get; set; } = string.Empty;

        /// <summary>
        /// The <see cref="IFileStorage"/> used to store the log files.
        /// Defaults to <see cref="LocalFileStorage"/> using <see cref="ApplicationLogDirectory"/> unless you set <see cref="ApplicationLogNoFileStorage"/> to <see langword="false"/>.
        /// </summary>
        public IFileStorage? ApplicationLogFileStorage { get; set; }

        /// <summary>
        /// Disables creating a new <see cref="LocalFileStorage"/> instance to write the application log to.
        /// </summary>
        public bool ApplicationLogNoFileStorage { get; set; } = false;


        /// <summary>
        /// Location of the credentials file.
        /// This file will be encrypted with a key protected by the platform credential manager.
        /// All application credentials, however, will live in this file.
        /// </summary>
        public string CredentialFileName { get; set; } = "unify-credentials";

        /// <summary>
        /// Name of the directory <see cref="CredentialFileName"/> is saved to.
        /// </summary>
        public string CredentialFileParentDirectoryName { get; set; } = string.Empty;



        /// <summary>
        /// Hooks that will run with <see cref="Runtime.Initialize"/>.
        /// </summary>
        public RuntimeHook[] Hooks { get; set; } = Array.Empty<RuntimeHook>();


        /// <summary>
        /// Whether Unify suppresses file system exceptions for Unify <see cref="IFileStorage"/> classes.
        /// </summary>
        public bool SuppressFileStorageExceptions { get; set; } = false;
    }
}
