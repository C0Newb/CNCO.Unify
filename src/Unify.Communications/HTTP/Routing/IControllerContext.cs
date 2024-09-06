namespace CNCO.Unify.Communications.Http.Routing {
    /// <summary>
    /// Controller context for an incoming request.
    /// </summary>
    public interface IControllerContext {
        /// <summary>
        /// The incoming web request.
        /// </summary>
        public IWebRequest WebRequest { get; }

        /// <summary>
        /// The outgoing web response.
        /// </summary>
        public IWebResponse WebResponse { get; }
    }
}
