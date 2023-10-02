﻿namespace CNCO.Unify.Configuration {

    [Serializable]
    public class ConfigurationNotFoundException : ConfigurationException {
        public ConfigurationNotFoundException() { }
        public ConfigurationNotFoundException(string message) : base(message) { }
        public ConfigurationNotFoundException(string message, Exception inner) : base(message, inner) { }
        protected ConfigurationNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
