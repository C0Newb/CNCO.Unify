using System.Globalization;

namespace CNCO.Unify.Logging {
    /// <summary>
    /// Logger skeleton
    /// </summary>
    public class Logger : ILogger {
        private readonly string _sectionName = string.Empty;
        public string SectionName {
            get => _sectionName;
        }

        public Logger() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        /// <param name="sectionName">Name of the current section being logged.</param>
        public Logger(string sectionName) {
            _sectionName = sectionName;
        }


        public void Emergency(string message) => Log(LogLevel.Emergency, SectionName, message);
        public void Emergency(string section, string message) => Log(LogLevel.Emergency, section, message);

        public void Alert(string message) => Log(LogLevel.Alert, SectionName, message);
        public void Alert(string section, string message) => Log(LogLevel.Alert, section, message);

        public void Error(string message) => Log(LogLevel.Error, SectionName, message);
        public void Error(string section, string message) => Log(LogLevel.Error, section, message);

        public void Warning(string message) => Log(LogLevel.Warning, SectionName, message);
        public void Warning(string section, string message) => Log(LogLevel.Warning, section, message);

        public void Notice(string message) => Log(LogLevel.Notice, SectionName, message);
        public void Notice(string section, string message) => Log(LogLevel.Notice, section, message);

        public void Info(string message) => Log(LogLevel.Info, SectionName, message);
        public void Info(string section, string message) => Log(LogLevel.Info, section, message);

        public void Log(string message) => Log(LogLevel.Info, SectionName, message);
        public void Log(string section, string message) => Log(LogLevel.Info, section, message);

        public void Debug(string message) => Log(LogLevel.Debug, SectionName, message);
        public void Debug(string section, string message) => Log(LogLevel.Debug, section, message);

        public void Verbose(string message) => Log(LogLevel.Verbose, SectionName, message);
        public void Verbose(string section, string message) => Log(LogLevel.Verbose, section, message);



        /// <summary>
        /// Logs a message to the appropriate output (such as a file, console, etc).
        /// </summary>
        /// <param name="logLevel">The event level of the message.</param>
        /// <param name="message">What to log.</param>
        /// <param name="section">Temporary override of <see cref="SectionName"/>.</param>
        public virtual void Log(LogLevel logLevel, string section, string message) { }


        /// <summary>
        /// Replaces places holders in a log message.
        /// 
        /// <c>%datetime%</c>: Current local datetime stamp.
        /// <c>%datetimeUTC%</c>: Current ISO datetime.
        /// <c>%section%</c>: Current section.
        /// </summary>
        /// <param name="message">Message to log.</param>
        /// <param name="logLevel">Event level of this message.</param>
        /// <returns>Formatted message.</returns>
        protected string FormatMessage(string message, LogLevel? logLevel = null, string? section = null) {
            section ??= SectionName;

            message = message.Replace("%datetime%", DateTime.Now.ToString("o", CultureInfo.InvariantCulture));
            message = message.Replace("%dateTimeUTC%", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            message = message.Replace("%section%", _sectionName ?? string.Empty);

            string prefix = $"[{DateTime.Now.ToString("s", CultureInfo.InvariantCulture) + "Z"}]";
            if (logLevel != null)
                prefix += $"[{logLevel}]";
            if (!string.IsNullOrEmpty(section))
                prefix += $"[{section}]";


            message = prefix + " " + message;

            return message;
        }
    }
}
