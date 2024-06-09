using CNCO.Unify.Storage;

namespace CNCO.Unify {
    /// <summary>
    /// Used to configure the <see cref="UnifyRuntime"/> via the constructor.
    /// </summary>
    public sealed class UnifyRuntimeConfiguration : IRuntimeConfiguration {
        /// <summary>
        /// Your application's unique identifier.
        /// </summary>
        public string? ApplicationId { get; set; }

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
        /// Hooks that will run with <see cref="UnifyRuntime.Initialize"/>.
        /// </summary>
        public RuntimeHook[] Hooks { get; set; } = Array.Empty<RuntimeHook>();


        /// <summary>
        /// Whether Unify suppresses file system exceptions for Unify <see cref="IFileStorage"/> classes.
        /// </summary>
        public bool SuppressFileStorageExceptions { get; set; } = false;
    }
}
