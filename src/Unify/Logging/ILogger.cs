namespace CNCO.Unify.Logging {
    /// <summary>
    /// Logging interface.
    /// </summary>
    public interface ILogger {
        /// <summary>
        /// Logs a message at the <see cref="LogLevel.Info"/> level.
        /// <inheritdoc cref="LogLevel.Info"/>
        /// </summary>
        /// <param name="message">Message to log.</param>
        void Log(string message);

        /// <inheritdoc cref="Log(string)"/>
        /// <param name="section">Section name this message belongs to.</param>
        void Log(string section, string message);

        /// <inheritdoc cref="Log(string, string)"/>
        /// <summary>
        /// Logs a message at the <paramref name="logLevel"/> level.
        /// </summary>
        /// <param name="logLevel">Level of the message.</param>
        void Log(LogLevel logLevel, string section, string message);


        /// <summary>
        /// Logs a message at the <see cref="LogLevel.Verbose"/> level.
        /// <inheritdoc cref="LogLevel.Verbose"/>
        /// </summary>
        /// <param name="message">Message to log.</param>
        void Verbose(string message);

        /// <inheritdoc cref="Verbose(string)"/>
        /// <param name="section">Section name this message belongs to.</param>
        void Verbose(string section, string message);


        /// <summary>
        /// Logs a message at the <see cref="LogLevel.Debug"/> level.
        /// <inheritdoc cref="LogLevel.Debug"/>
        /// </summary>
        /// <param name="message">Message to log.</param>
        void Debug(string message);

        /// <inheritdoc cref="Debug(string)"/>
        /// <param name="section">Section name this message belongs to.</param>
        void Debug(string section, string message);


        /// <summary>
        /// Logs a message at the <see cref="LogLevel.Info"/> level.
        /// </summary>
        /// <param name="message">Message to log.</param>
        void Info(string message);

        /// <inheritdoc cref="Info(string)"/>
        /// <param name="section">Section name this message belongs to.</param>
        void Info(string section, string message);


        /// <summary>
        /// Logs a message at the <see cref="LogLevel.Notice"/> level.
        /// <inheritdoc cref="LogLevel.Notice"/>
        /// </summary>
        /// <param name="message">Message to log.</param>
        void Notice(string message);

        /// <inheritdoc cref="Notice(string)"/>
        /// <param name="section">Section name this message belongs to.</param>
        void Notice(string section, string message);


        /// <summary>
        /// Logs a message at the <see cref="LogLevel.Warning"/> level.
        /// <inheritdoc cref="LogLevel.Warning"/>
        /// </summary>
        /// <param name="message">Message to log.</param>
        void Warning(string message);

        /// <inheritdoc cref="Warning(string)"/>
        /// <param name="section">Section name this message belongs to.</param>
        void Warning(string section, string message);


        /// <summary>
        /// Logs a message at the <see cref="LogLevel.Error"/> level.
        /// <inheritdoc cref="LogLevel.Error"/>
        /// </summary>
        /// <param name="message">Message to log.</param>
        void Error(string message);

        /// <inheritdoc cref="Error(string)"/>
        /// <param name="section">Section name this message belongs to.</param>
        void Error(string section, string message);


        /// <summary>
        /// Logs a message at the <see cref="LogLevel.Alert"/> level.
        /// <inheritdoc cref="LogLevel.Alert"/>
        /// </summary>
        /// <param name="message">Message to log.</param>
        void Alert(string message);

        /// <inheritdoc cref="Alert(string)"/>
        /// <param name="section">Section name this message belongs to.</param>
        void Alert(string section, string message);


        /// <summary>
        /// Logs a message at the <see cref="LogLevel.Emergency"/> level.
        /// <inheritdoc cref="LogLevel.Emergency"/>
        /// </summary>
        /// <param name="message">Message to log.</param>
        void Emergency(string message);

        /// <inheritdoc cref="Emergency(string)"/>
        /// <param name="section">Section name this message belongs to.</param>
        void Emergency(string section, string message);
    }

    /// <summary>
    /// Level of the log.
    /// </summary>
    public enum LogLevel {

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
