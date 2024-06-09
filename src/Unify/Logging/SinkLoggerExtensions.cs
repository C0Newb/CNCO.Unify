using CNCO.Unify.Storage;

namespace CNCO.Unify.Logging {
    /// <summary>
    /// Extensions to the <see cref="SinkLogger"/> class to make adding sinks easier.
    /// Includes methods to add known Unify <see cref="ILogger"/>s.
    /// </summary>
    public static class SinkLoggerExtensions {
        #region ConsoleLogger
        public static SinkLogger UseConsoleLogger(
            this SinkLogger logger
        ) => UseConsoleLogger(logger, new ConsoleLogger());

        public static SinkLogger UseConsoleLogger(
            this SinkLogger logger,
            string section
        ) => UseConsoleLogger(logger, new ConsoleLogger(section));

        public static SinkLogger UseConsoleLogger(
            this SinkLogger logger,
            ILogFormatter formatter
        ) => UseConsoleLogger(logger, new ConsoleLogger(formatter));


        public static SinkLogger UseConsoleLogger(
            this SinkLogger logger,
            ConsoleLogger diagnosticsLogger
        ) {
            logger.AddLogger(diagnosticsLogger);
            return logger;
        }
        #endregion

        #region DiagnosticsLogger
        public static SinkLogger UseDiagnosticsLogger(
            this SinkLogger logger
        ) => UseDiagnosticsLogger(logger, new DiagnosticsLogger());

        public static SinkLogger UseDiagnosticsLogger(
            this SinkLogger logger,
            string section
        ) => UseDiagnosticsLogger(logger, new DiagnosticsLogger(section));

        public static SinkLogger UseDiagnosticsLogger(
            this SinkLogger logger,
            ILogFormatter formatter
        ) => UseDiagnosticsLogger(logger, new DiagnosticsLogger(formatter));


        public static SinkLogger UseDiagnosticsLogger(
            this SinkLogger logger,
            DiagnosticsLogger diagnosticsLogger
        ) {
            logger.AddLogger(diagnosticsLogger);
            return logger;
        }
        #endregion

        #region FileLogger
        /// <summary>
        /// Add a <see cref="FileLogger"/> to the logger.
        /// </summary>
        /// <param name="logger">Sink to add the file logger to.</param>
        /// <param name="fileLogger">File logger to add.</param>
        /// <returns>Sink.</returns>
        public static SinkLogger UsingFileLogger(
            this SinkLogger logger,
            FileLogger fileLogger
        ) {
            logger.AddLogger(fileLogger);
            return logger;
        }

        public static SinkLogger UsingFileLogger(
            this SinkLogger logger,
            IFileStorage fileStorage,
            string logFileName
        ) => logger.UsingFileLogger(new FileLogger(fileStorage, logFileName));

        public static SinkLogger UsingFileLogger(
            this SinkLogger logger,
            IFileStorage fileStorage,
            string logFileName,
            string sectionName
        ) => logger.UsingFileLogger(new FileLogger(fileStorage, logFileName, sectionName));

        public static SinkLogger UsingFileLogger(
            this SinkLogger logger,
            IFileStorage fileStorage,
            string logFileName,
            ILogFormatter formatter
        ) => logger.UsingFileLogger(new FileLogger(fileStorage, logFileName, formatter));
        #endregion
    }
}