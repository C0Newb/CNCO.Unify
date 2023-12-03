namespace CNCO.Unify.Logging {
    /// <summary>
    /// Allows you to layer the "section" tag on <see cref="ILogger"/>s.
    /// </summary>
    public class ProxyLogger : Logger {
        private readonly ILogger _parent;
        private readonly string _proxyName;
        private readonly bool _appendSectionName;

        /// <summary>
        /// Creates a new instance of <see cref="ProxyLogger"/> given a section name.
        /// All logs to this instance will be <c><![CDATA[<this section name>: <original section name>]]></c>.
        /// </summary>
        /// <param name="sectionName">Section name to add.</param>
        /// <param name="appendSectionName">Whether the section name should append the old one "<c>Section: New</c>" or prepend (<c>New: Section</c>).</param>
        public ProxyLogger(ILogger parent, string sectionName, bool appendSectionName = true) {
            _parent = parent;
            _proxyName = sectionName;
            _appendSectionName = appendSectionName;
        }

        public override void Log(LogLevel logLevel, string section, string message) {
            if (!string.IsNullOrEmpty(section)) {
                if (_appendSectionName)
                    section = $"{_proxyName}: {section}";
                else
                    section = $"{section}: {_proxyName}";
            } else {
                section = _proxyName;
            }

            _parent.Log(logLevel, section, message);
        }
    }
}
