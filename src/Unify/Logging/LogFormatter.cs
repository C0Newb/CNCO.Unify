using System.Globalization;
using System.Text;

namespace CNCO.Unify.Logging {
    public class LogFormatter : ILogFormatter {
        private readonly string _sectionName;

        public string DateFormat { get; set; } = "s";

        public string SectionName {
            get => _sectionName;
        }

        public LogFormatter() : this(string.Empty) { }

        public LogFormatter(string? sectionName) {
            _sectionName = sectionName ?? string.Empty;
        }

        /// <summary>
        /// Replaces places holders in a log message.
        /// 
        /// <c>%datetime%</c>: Current local datetime stamp.
        /// <c>%datetimeUTC%</c>: Current ISO datetime.
        /// <c>%section%</c>: Current section.
        /// </summary>
        /// <param name="message">Message to log.</param>
        /// <returns>Formatted message.</returns>
        public string FormatMessage(string message) => FormatMessage(message, null, null);

        /// <inheritdoc cref="FormatMessage(string)"/>
        /// <param name="level">Event level of this message.</param>
        public string FormatMessage(string message, LogLevel level) => FormatMessage(message, level, null);

        /// <inheritdoc cref="FormatMessage(string, LogLevel)"/>
        /// <param name="section">Section title override.</param>
        public virtual string FormatMessage(string message, LogLevel? level = null, string? section = null)
            => FormatMessagePrivate(message, level, section);

        public virtual string FormatMessageWithAnsiCodes(string message, LogLevel? level = null, string? section = null)
            => FormatMessagePrivate(message, level, section, true);

        private string FormatMessagePrivate(string message, LogLevel? level = null, string? section = null, bool insertAnsiCode = false) {
            section ??= SectionName;

            message = message.Replace("%datetime%", DateTime.Now.ToString("o", CultureInfo.InvariantCulture));
            message = message.Replace("%dateTimeUTC%", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            message = message.Replace("%section%", SectionName ?? string.Empty);

            StringBuilder prefix = new StringBuilder();
            prefix.Append('[');
            prefix.Append(DateTime.Now.ToString(DateFormat, CultureInfo.InvariantCulture));
            prefix.Append(']');

            if (level.HasValue) {
                prefix.Append('[');
                if (insertAnsiCode) {
                    switch (level.Value) {
                        case LogLevel.Emergency:
                            prefix.Append("\x1b[91m");
                            break;
                        case LogLevel.Alert:
                        case LogLevel.Error:
                            prefix.Append("\x1b[31m");
                            break;

                        case LogLevel.Warning:
                            prefix.Append("\x1b[33m");
                            break;

                        case LogLevel.Notice:
                        case LogLevel.Info:
                            prefix.Append("\x1b[32m");
                            break;

                        case LogLevel.Debug:
                        case LogLevel.Verbose:
                            prefix.Append("\x1b[36m");
                            break;
                    }
                }
                prefix.Append(level.Value);

                if (insertAnsiCode)
                    prefix.Append("\x1b[0m");

                prefix.Append(']');
            }
            if (!string.IsNullOrEmpty(section))
                prefix.Append($"[{section}]");


            message = prefix + " " + message;

            return message;
        }
    }
}
