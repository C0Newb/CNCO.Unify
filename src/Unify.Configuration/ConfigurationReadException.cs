namespace CNCO.Unify.Configuration {

    [Serializable]
    public class ConfigurationReadException : ConfigurationException {
        public ConfigurationReadException() { }
        public ConfigurationReadException(string message) : base(message) { }
        public ConfigurationReadException(string message, Exception inner) : base(message, inner) { }
    }
}
