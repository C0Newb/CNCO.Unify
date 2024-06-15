using CNCO.Unify.Logging;
using System.Reflection;

namespace CNCO.Unify {
    /// <summary>
    /// Base class for <see cref="IRuntime"/>
    /// </summary>
    public abstract class Runtime : IRuntime {
        protected readonly List<RuntimeHook> _runtimeHooks = new List<RuntimeHook>();
        protected readonly List<IRuntime> _runtimeLinks = new List<IRuntime>();
        private bool _initialized = false;
        private SectionLogger? _runtimeLogger;

        #region Locks
        // Lock used when adding runtime hooks or links.
        protected readonly object _addToListLock = new object();
        // Lock used when initializing this class.
        protected readonly object _initializationLock = new object();
        #endregion

        #region Properties
        protected ILogger RuntimeLog {
            get {
                if (_runtimeLogger == null) { // Null?
                    lock (_initializationLock) { // Lock.
                        // Null? -> Create
                        _runtimeLogger ??= new SectionLogger(UnifyRuntime.ApplicationLog, GetType().Name);
                    }
                }
                return _runtimeLogger;
            }
        }

        /// <summary>
        /// Whether the static instance has been initialized or not.
        /// </summary>
        public bool Initialized {
            get => _initialized;
            protected set => _initialized = value;
        }
        #endregion

        #region Runtime hooks
        /// <summary>
        /// Adds a <see cref="RuntimeHook"/> to the list of current runtime hooks.
        /// Hooks are called whenever <see cref="Initialize"/> is called.
        /// Hook will not be added, nor called, if <see cref="Initialized"/> is <see langword="true"/>.
        /// </summary>
        /// <param name="hook">Runtime hook to be added.</param>
        public void AddHook(RuntimeHook hook) {
            lock (_addToListLock) {
                if (!_initialized && !_runtimeHooks.Contains(hook))
                    _runtimeHooks.Add(hook);
            }
        }

        // Runs a hook.
        private bool RunHook(RuntimeHook hook) {
            try {
                string nameTruncated = hook.Name[..Math.Min(hook.Name.Length, 75)];
                string? descriptionTruncated = hook.Description?[..Math.Min(hook.Name.Length, 250)];

                RuntimeLog.Debug($"Calling hook {nameTruncated}");
                if (!string.IsNullOrEmpty(hook.Description))
                    RuntimeLog.Debug($"{nameTruncated}: {descriptionTruncated}");

                hook.Action(UnifyRuntime.Current);
                return true;
            } catch (Exception e) {
                RuntimeLog.Error("Error calling the previous hook: ");
                RuntimeLog.Error(e.Message);

                if (e.StackTrace != null)
                    RuntimeLog.Debug(e.StackTrace);
                else
                    RuntimeLog.Debug("No stack trace to report.");

                return false;
            }
        }
        #endregion

        #region RuntimeLinks
        /// <summary>
        /// Adds a <see cref="RuntimeLink"/>, such as Unify.SecurityRuntime
        /// </summary>
        /// <remarks>
        /// Allows Unify libraries to link into the <see cref="IRuntime"/>s of other Unify libraries without depending on those classes.
        /// </remarks>
        /// <param name="runtime">The runtime to add.</param>
        public void AddRuntimeLink(IRuntime runtime) {
            if (runtime != null && !ContainsRuntimeLink(runtime))
                _runtimeLinks.Add(runtime);
        }

        /// <summary>
        /// Checks whether a <see cref="RuntimeLink"/> has been added.
        /// </summary>
        /// <param name="link">Link to search for.</param>
        /// <returns>Whether <paramref name="link"/> has been added to the list of runtime links.</returns>
        public bool ContainsRuntimeLink(IRuntime runtime) {
            return _runtimeLinks.Where(x => x.Equals(runtime)).Any();
        }

        /// <summary>
        /// Removes a <see cref="RuntimeLink"/> from the list of links.
        /// </summary>
        /// <param name="link">The unique Runtime to remove.</param>
        public void RemoveRuntimeLink(IRuntime runtime) {
            _runtimeLinks.RemoveAll(x => x.Equals(runtime));
        }


        private static readonly string _assemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? "?";
        private bool IsLinkableRuntimePredicate(Type type) {
            LinkRuntimeAttribute? linkRuntimeAttribute = type.GetCustomAttribute<LinkRuntimeAttribute>();
            if (linkRuntimeAttribute == null)
                return false;

            return typeof(IRuntime).IsAssignableFrom(type) // Implements IRuntime.
                && typeof(UnifyRuntime).IsAssignableFrom(type) // Does not extend us.
                && linkRuntimeAttribute.RuntimeType == typeof(UnifyRuntime)
                && !type.IsAbstract; // Not abstract.
        }

        private bool ReferencesUnify(Assembly assembly) {
            return assembly.GetReferencedAssemblies().Any(x => x.Name == _assemblyName);
        }

        // this doesn't work :)
        private void DiscoverAndLinkRuntimes() {
            var runtimeTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(ReferencesUnify)
                .SelectMany(assembly => assembly.GetTypes())
                .Where(IsLinkableRuntimePredicate)
                .ToArray();
            foreach (var runtimeType in runtimeTypes) {
                var runtimeInstance = (IRuntime?)Activator.CreateInstance(runtimeType);
                if (runtimeInstance == null)
                    continue;
                AddRuntimeLink(runtimeInstance);
            }
        }
        #endregion


        #region Initialization
        /// <summary>
        /// Initializes this <see cref="IRuntime"/>, runs added <see cref="RuntimeHook"/>, and links dependent runtimes.
        /// </summary>
        public virtual void Initialize() {
            lock (_initializationLock) {
                if (_initialized)
                    return;

                _initialized = true;

                // initialize links
                DiscoverAndLinkRuntimes();
                foreach (var link in _runtimeLinks) {
                    link.Initialize();
                }


                // run hooks
                int hooksCount = _runtimeHooks.Count;
                int failedHooks = 0;
                foreach (var hook in _runtimeHooks) {
                    if (!RunHook(hook))
                        failedHooks++;
                }
                _runtimeHooks.Clear();

                // Only log if UnifyRuntime has an instance (it really should by now!)
                string section = GetType().FullName ?? GetType().Name;
                if (failedHooks > 0)
                    UnifyRuntime.ApplicationLog.Info(section, $"All hooks {hooksCount} called, {failedHooks} failed.");
                else
                    UnifyRuntime.ApplicationLog.Info(section, $"All hooks {hooksCount} called, 0 failed.");
            }
        }
        #endregion

        public override bool Equals(object? obj) {
            return obj != null
                && obj.GetType() == GetType(); // yeah probably
        }

        public override int GetHashCode() => base.GetHashCode();
    }
}
