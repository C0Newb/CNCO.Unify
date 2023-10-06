namespace CNCO.Unify.Logging {
    /// <summary>
    /// Logs to multiple <see cref="ILogger"/>'s
    /// </summary>
    public class MultiLogger : Logger {
        private readonly List<ILogger> _loggers = new List<ILogger>();

        /// <summary>
        /// Initializes a new <see cref="MultiLogger"/> instance.
        /// </summary>
        public MultiLogger() { }

        /// <summary>
        /// Initializes a new <see cref="MultiLogger"/> instance.
        /// </summary>
        /// <param name="logger">Adds a <see cref="ILogger"/> sink to log to.</param>
        public MultiLogger(ILogger logger) {
            _loggers.Add(logger);
        }

        /// <summary>
        /// Initializes a new <see cref="MultiLogger"/> instance.
        /// </summary>
        /// <param name="loggers">Different <see cref="ILogger"/> sinks to log to.</param>
        public MultiLogger(IEnumerable<ILogger> loggers) {
            _loggers.AddRange(loggers);
        }

        /// <summary>
        /// Adds a <see cref="ILogger"/> to the tracked logging sinks.
        /// </summary>
        /// <param name="logger">A <see cref="ILogger"/> sink to log to.</param>
        public void AddLogger(ILogger logger) => _loggers.Add(logger);

        /// <summary>
        /// Removes a <see cref="ILogger"/> from the tracked logging sinks.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> sink you wish to no longer log to.</param>
        public void RemoveLogger(ILogger logger) => _loggers.Remove(logger);


        /// <summary>
        /// Logs a message to all tracked <see cref="ILogger"/>'s.
        /// </summary>
        /// <inheritdoc cref="Logger.Log(LogLevel, string, string)"/>
        public override void Log(LogLevel logLevel, string section, string message) {
            foreach (var logger in _loggers) {
                logger.Log(logLevel, section, message);
            }
        }
    }
}
