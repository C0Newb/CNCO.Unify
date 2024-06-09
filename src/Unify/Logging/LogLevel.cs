namespace CNCO.Unify.Logging {
    /// <summary>
    /// Level of the log.
    /// </summary>
    public enum LogLevel {
        /// <summary>
        /// Yapping messages.
        /// </summary>
        Verbose = 0,

        /// <summary>
        /// Debug-level messages.
        /// </summary>
        Debug = 1,

        /// <summary>
        /// Informational messages.
        /// </summary>
        Info = 2,

        /// <summary>
        /// Normal but significant condition.
        /// </summary>
        Notice = 3,

        /// <summary>
        /// Warning conditions.
        /// </summary>
        Warning = 4,

        /// <summary>
        /// Error conditions.
        /// </summary>
        Error = 5,

        /// <summary>
        /// Action must be taken immediately.
        /// </summary>
        Alert = 6,

        /// <summary>
        /// System is unstable. Dire stuff.
        /// </summary>
        Emergency = 7,
    }
}
