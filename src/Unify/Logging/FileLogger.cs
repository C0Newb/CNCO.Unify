using CNCO.Unify.Storage;

namespace CNCO.Unify.Logging {
    /// <summary>
    /// Logger that outputs to a <see cref="IFileStorage"/>
    /// </summary>
    public class FileLogger : Logger {
        private readonly string _fileName;
        private readonly IFileStorage _fileStorage;

        public string FileName {
            get => _fileName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileLogger"/> class logging to <paramref name="fileStorage"/>.
        /// </summary>
        /// <param name="fileStorage">The <see cref="IFileStorage"/> to log to.</param>
        public FileLogger(IFileStorage fileStorage, string logFileName) : base() {
            _fileStorage = fileStorage;
            _fileName = logFileName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileLogger"/> class logging to <paramref name="fileStorage"/>.
        /// </summary>
        /// <param name="fileStorage">The <see cref="IFileStorage"/> to log to.</param>
        /// <param name="sectionName">Name of the section.</param>
        public FileLogger(IFileStorage fileStorage, string logFileName, string sectionName) : base(sectionName) {
            _fileStorage = fileStorage;
            _fileName = logFileName;
        }


        public override void Log(LogLevel logLevel, string section, string message) {
            message = FormatMessage(message, logLevel, section) + Environment.NewLine;
            _fileStorage.Append(message, _fileName);
        }
    }
}