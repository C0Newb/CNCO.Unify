namespace CNCO.Unify.Communications.Http {
    /// <summary>
    /// Configuration options for <see cref="Router"/>.
    /// </summary>
    public class RouterRuntimeConfiguration {
        /// <summary>
        /// Whether <see cref="Router"/> should respond with the default code and status message if not listeners respond.
        /// </summary>
        /// <remarks>
        /// The default response is only sent out if no listeners respond with <see cref="ResponseTimeoutMilliseconds"/>.
        /// </remarks>
        public bool EnableDefaultResponses { get; set; } = false;

        /// <summary>
        /// The default HTTP status code <see cref="Router"/> responds with if no listeners respond and <see cref="EnableDefaultResponses"/> is <see langword="true"/>
        /// </summary>
        public int? DefaultResponseStatusCode { get; set; } = 200;

        /// <summary>
        /// The default HTTP response body <see cref="Router"/> responds with if o listeners respond and <see cref="EnableDefaultResponses"/> is <see langword="true"/>
        /// </summary>
        public string? DefaultResponseBody { get; set; } = "";

        /// <summary>
        /// Amount time to wait for listeners to a HTTP request to respond before
        /// a 500 code or a <see cref="DefaultResponseStatusCode"/> (if <see cref="EnableDefaultResponses"/> is <see langword="true"/>) is sent.
        /// </summary>
        public int ResponseTimeoutMilliseconds { get; set; } = 15000;
    }
}
