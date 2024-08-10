using CNCO.Unify.Communications.Http;

namespace CNCO.Unify.Communications {
    /// <summary>
    /// Configuration for the communications namespace.
    /// </summary>
    public sealed class CommunicationsRuntimeConfiguration : IRuntimeConfiguration {

        public CommunicationsRuntimeConfiguration() { }

        /// <summary>
        /// Configuration options for the <see cref="Http"/> namespace.
        /// </summary>
        public RuntimeHttpConfiguration RuntimeHttpConfiguration { get; set; } = new RuntimeHttpConfiguration();
    }
}
