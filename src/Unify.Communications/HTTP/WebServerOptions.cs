namespace CNCO.Unify.Communications.HTTP {

    /// <summary>
    /// <see cref="WebServer"/> settings.
    /// </summary>
    public class WebServerOptions {
        /// <summary>
        /// The base URL where the server will listen.
        /// </summary>
        public string[]? Endpoints { get; set; } = new string[1] { "http://*:8008" };

        /// <summary>
        /// Log accesses, not only errors (500).
        /// </summary>
        /// <remarks>
        /// I highly recommend this being off until the file logger can improve.
        /// Since every log entry is instantly flushed to disk, this murders performance.
        /// ~900rq/s on Windows with logging on, ~90k with it off.
        /// </remarks>
        public bool LogAccess = false;


        // HTTPS settings
        public bool UseHttps { get; set; } = false;
        public string? CertificatePath { get; set; }
        public string? CertificatePassword { get; set; }
    }
}
