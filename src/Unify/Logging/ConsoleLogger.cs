namespace CNCO.Unify.Logging {
    /// <summary>
    /// Logs to the <see cref="Console"/>.
    /// </summary>
    public sealed class ConsoleLogger : Logger {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleLogger"/> class.
        /// </summary>
        public ConsoleLogger() : base() { }

        /// <inheritdoc cref="ConsoleLogger()"/>
        /// <inheritdoc cref="Logger(string)"/>
        public ConsoleLogger(string sectionName) : base(sectionName) { }

        /// <inheritdoc cref="ConsoleLogger()"/>
        /// <inheritdoc cref="Logger(ILogFormatter)"/>
        public ConsoleLogger(ILogFormatter formatter) : base(formatter) { }

        public override void Log(LogLevel logLevel, string section, string message) {
            message = LogFormatter.FormatMessageWithAnsiCodes(message, logLevel, section);
            Console.WriteLine(message);
        }
    }
}
