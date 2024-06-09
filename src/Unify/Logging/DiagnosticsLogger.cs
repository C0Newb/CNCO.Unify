namespace CNCO.Unify.Logging {
    /// <summary>
    /// Logs to the <see cref="System.Diagnostics"/> namespace.
    /// If on Android, logs to LogCat.
    /// </summary>
    public sealed class DiagnosticsLogger : Logger {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticsLogger"/> class.
        /// </summary>
        public DiagnosticsLogger() : base() { }

        /// <inheritdoc cref="DiagnosticsLogger()"/>
        /// <inheritdoc cref="Logger(string)"/>
        public DiagnosticsLogger(string sectionName) : base(sectionName) { }

        /// <inheritdoc cref="DiagnosticsLogger()"/>
        /// <inheritdoc cref="Logger(ILogFormatter)"/>
        public DiagnosticsLogger(ILogFormatter formatter) : base(formatter) { }


        public override void Log(LogLevel logLevel, string section, string message) {
            message = FormatMessage(message, logLevel, section);

#if ANDROID
            string tag = $"Unify-{GetType().Name}";

            switch (logLevel) {
                case LogLevel.Verbose:
                    Android.Util.Log.Verbose(tag, message);
                    break;

                case LogLevel.Debug:
                    Android.Util.Log.Debug(tag, message);
                    break;

                case LogLevel.Notice:
                case LogLevel.Warning:
                    Android.Util.Log.Warn(tag, message);
                    break;

                case LogLevel.Error:
                case LogLevel.Alert:
                    Android.Util.Log.Error(tag, message);
                    break;

                case LogLevel.Emergency:
                    Android.Util.Log.Wtf(tag, message);
                    break;

                case LogLevel.Info:
                default:
                    Android.Util.Log.Info(tag, message);
                    break;
            }
#else
            System.Diagnostics.Debug.WriteLine(message);
#endif
        }
    }
}
