using System.Text;

namespace CNCO.Unify.Logging {
    /// <summary>
    /// Logs to the <see cref="Console"/>.
    /// </summary>
    public sealed class ConsoleLogger : Logger {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleLogger"/> class.
        /// </summary>
        public ConsoleLogger() : base() {
            InitializeConsole();
        }

        /// <inheritdoc cref="ConsoleLogger()"/>
        /// <inheritdoc cref="Logger(string)"/>
        public ConsoleLogger(string sectionName) : base(sectionName) {
            InitializeConsole();
        }

        /// <inheritdoc cref="ConsoleLogger()"/>
        /// <inheritdoc cref="Logger(ILogFormatter)"/>
        public ConsoleLogger(ILogFormatter formatter) : base(formatter) {
            InitializeConsole();
        }

        public override void Log(LogLevel logLevel, string section, string message) {
#if !ANDROID && !IOS && !ANDROID21_0_OR_GREATER
            message = LogFormatter.FormatMessageWithAnsiCodes(message, logLevel, section);
            Console.WriteLine(message);
#endif
        }

        public void Clear() {
#if !ANDOID && !IOS && !ANDROID21_0_OR_GREATER
            Console.Clear();
#endif
        }

        private static void InitializeConsole() {
#if !ANDROID && !IOS && !ANDROID21_0_OR_GREATER
            try {
                var standardOut = Console.OpenStandardOutput();
                var con = new StreamWriter(standardOut, Encoding.ASCII) {
                    AutoFlush = true
                };
                Console.SetOut(con);
            } catch (Exception) { }
#endif
        }
    }
}
