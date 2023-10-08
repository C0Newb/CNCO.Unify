namespace CNCO.Unify.Logging {
    /// <summary>
    /// Logs to the <see cref="System.Diagnostics"/> namespace.
    /// If on Android, logs to LogCat.
    /// </summary>
    public class DiagnosticsLogger : Logger {

        public DiagnosticsLogger() : base() { }
        public DiagnosticsLogger(string section) : base(section) { }


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
