using System.Globalization;

namespace CNCO.Unify.Logging {
    public class LogFormatter : ILogFormatter {
        private readonly string _sectionName;

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
        public virtual string FormatMessage(string message, LogLevel? level = null, string? section = null) {
            section ??= SectionName;

            message = message.Replace("%datetime%", DateTime.Now.ToString("o", CultureInfo.InvariantCulture));
            message = message.Replace("%dateTimeUTC%", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            message = message.Replace("%section%", SectionName ?? string.Empty);

            string prefix = $"[{DateTime.Now.ToString("s", CultureInfo.InvariantCulture) + "Z"}]";
            if (level.HasValue)
                prefix += $"[{level.Value}]";
            if (!string.IsNullOrEmpty(section))
                prefix += $"[{section}]";


            message = prefix + " " + message;

            return message;
        }

        public virtual string FormatMessageWithAnsiCodes(string message, LogLevel? level = null, string? section = null)
            => FormatMessage(message, level, section);
    }
}
