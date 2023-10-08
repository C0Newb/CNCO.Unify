using CNCO.Unify.Logging;
using CNCO.Unify.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNCO.Unify {
    /// <summary>
    /// The main entry point and setup class for Unify. Similar to <c>Program.cs</c>.
    /// </summary>
    public class Runtime {
        private static Runtime? _instance;
        
        private readonly List<RuntimeHook> _hooks = new List<RuntimeHook>(0);
        private MultiLogger? applicationLog;
        internal ProxyLogger? unifyLog;

        #region Properties
        internal ProxyLogger UnifyLog {
            get {
                unifyLog ??= new ProxyLogger(ApplicationLog, "Unify");
                return unifyLog;
            }
        }

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
                _instance ??= new Runtime();
                return _instance;
            }
        }


        /// <summary>
        /// Application log
        /// </summary>
        public MultiLogger ApplicationLog {
            get {
                if (Current.applicationLog == null) {
                    Current.applicationLog = new MultiLogger();
                    
                    // Add storage logger
                    IFileStorage? logFileStorage = Current.Configuration.ApplicationLogFileStorage;
                    if (logFileStorage == null && !Current.Configuration.ApplicationLogNoFileStorage) {
                        logFileStorage = new LocalFileStorage(Current.Configuration.ApplicationLogDirectory);
                    }
                    if (logFileStorage != null) {
                        logFileStorage.Delete(Current.Configuration.ApplicationLogName); // remove previous
                        var fileLogger = new FileLogger(logFileStorage, Current.Configuration.ApplicationLogName);
                        Current.applicationLog.AddLogger(fileLogger);
                    }

                    Current.applicationLog.AddLogger(new DiagnosticsLogger());

                    Current.applicationLog.Log("Unify", "Bonjour!"); // startup message
                }
                return Current.applicationLog;
            }
        }
        #endregion

        public Runtime() {
            _instance = this;
        }

        public Runtime(RuntimeConfiguration configuration) {
            Configuration = configuration;
            _instance = this;
        }

        /// <summary>
        /// Adds a <see cref="RuntimeHook"/> to the list of current runtime hooks.
        /// Hook will not be added, nor called, if <see cref="Initialized"/> is <see langword="true"/>
        /// </summary>
        /// <param name="hook">Runtime hook to be added.</param>
        public static void AddHook(RuntimeHook hook) {
            if (!Current.Initialized)
                Current._hooks.Add(hook);
        }


        private bool RunHook(RuntimeHook hook) {
            try {
                string nameTruncated = hook.Name.Substring(0, Math.Min(hook.Name.Length, 75));
                string? descriptionTruncated = hook.Description?.Substring(0, Math.Min(hook.Name.Length, 250));

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
        /// The name of the application log file saved to <see cref="ApplicationLogFileStorage"/>
        /// </summary>
        public string ApplicationLogDirectory { get; set; } = string.Empty;
        /// <summary>
        /// If <see cref="ApplicationLogFileStorage"/> is <see langword="null"/>, the directory log files are saved to on disk.
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
        /// Hooks that will run with <see cref="Runtime.Initialize"/>.
        /// </summary>
        public RuntimeHook[] Hooks { get; set; } = Array.Empty<RuntimeHook>();
    }
}
