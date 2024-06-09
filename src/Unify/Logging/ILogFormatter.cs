namespace CNCO.Unify.Logging {
    public interface ILogFormatter {
        public string SectionName { get; }

        public string FormatMessage(string message);
        public string FormatMessage(string message, LogLevel level);
        public string FormatMessage(string message, LogLevel? level = null, string? section = null);
        public string FormatMessageWithAnsiCodes(string message, LogLevel? level = null, string? section = null);
    }
}