using System.Globalization;

namespace CNCO.Unify.Logging {
    /// <summary>
    /// Logger skeleton
    /// </summary>
    public abstract class Logger : ILogger {
        private readonly ILogFormatter _formatter;

        public string SectionName {
            get => _formatter.SectionName;
        }

        public ILogFormatter LogFormatter {
            get => _formatter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        public Logger() : this(string.Empty) { }

        /// <inheritdoc cref="Logger()"/>
        /// <param name="sectionName">Name of the current section being logged.</param>
        public Logger(string? sectionName) {
            _formatter = new LogFormatter(sectionName);
        }

        /// <inheritdoc cref="Logger()"/>
        /// <param name="formatter">Message formatter to use.</param>
        public Logger(ILogFormatter formatter) {
            _formatter = formatter;
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
        public abstract void Log(LogLevel logLevel, string section, string message);


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
        protected string FormatMessage(string message, LogLevel? logLevel = null, string? section = null)
            => _formatter.FormatMessage(message, logLevel, section);

        /// <summary>
        /// Creates a new <see cref="SectionLogger"/> that appends a new section name to this <see cref="ILogger"/>.
        /// </summary>
        /// <param name="sectionName">New section.</param>
        /// <param name="appendSectionName">
        /// Whether the new section name should append <see cref="SectionName"/>, as in come after it.
        /// </param>
        /// <returns>New logger that logs the same as this one, but with a section tacked to <see cref="SectionName"/>.</returns>
        public virtual ILogger NewSection(string sectionName, bool appendSectionName = true)
            => new SectionLogger(this, sectionName, appendSectionName);
    }
}
