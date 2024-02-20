namespace CNCO.Unify.Communications.Http {
    public enum HttpVerb {
        All,
        Get,
        Head,
        Post,
        Put,
        Delete,
        Connect,
        Options,
        Trace,
        Patch
    }

    /// <summary>
    /// Routes incoming requests.
    /// </summary>
    public interface IRouter {

        /// <summary>
        /// Process an incoming request, calls any listeners.
        /// </summary>
        /// <param name="request">The web request to process.</param>
        /// <param name="response">The response used for this request.</param>
        public void Process(WebRequest request, WebResponse response);


        /// <summary>
        /// Captures all incoming HTTP requests regardless of the request verb.
        /// </summary>
        /// <param name="path">
        /// To path to capture requests on. This is the path to your page, <c>/path/to/page</c>.
        /// It can include path parameters by surrounding the parameter using the <c>:</c> character, so <c>/path/to/:pageName:</c>. This would give you access to whatever the user entered for <c>:pageName:</c>.</param>
        /// <param name="callback">The action that is called when the path is hit.</param>
        public void All(string path, Action<WebRequest, WebResponse> callback);

        /// <summary>
        /// Deletes an optional listener for a given path. If no callback is provided, it removes all listeners for that path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="callback"></param>
        public void Remove(string path, Action<WebRequest, WebResponse>? callback, HttpVerb? httpVerb);


        /// <summary>
        /// Listens for incoming <c>GET</c> HTTP requests.
        /// </summary>
        /// <inheritdoc cref="All(string, Action{WebRequest, WebResponse})"/>
        public void Get(string path, Action<WebRequest, WebResponse> callback);

        /// <summary>
        /// Listens for incoming <c>HEAD</c> HTTP requests.
        /// </summary>
        /// <inheritdoc cref="All(string, Action{WebRequest, WebResponse})"/>
        public void Head(string path, Action<WebRequest, WebResponse> callback);

        /// <summary>
        /// Listens for incoming <c>POST</c> HTTP requests.
        /// </summary>
        /// <inheritdoc cref="All(string, Action{WebRequest, WebResponse})"/>
        public void Post(string path, Action<WebRequest, WebResponse> callback);

        /// <summary>
        /// Listens for incoming <c>PUT</c> HTTP requests.
        /// </summary>
        /// <inheritdoc cref="All(string, Action{WebRequest, WebResponse})"/>
        public void Put(string path, Action<WebRequest, WebResponse> callback);

        /// <summary>
        /// Listens for incoming <c>DELETE</c> HTTP requests.
        /// </summary>
        /// <inheritdoc cref="All(string, Action{WebRequest, WebResponse})"/>
        public void Delete(string path, Action<WebRequest, WebResponse> callback);

        /// <summary>
        /// Listens for incoming <c>CONNECT</c> HTTP requests.
        /// </summary>
        /// <inheritdoc cref="All(string, Action{WebRequest, WebResponse})"/>
        public void Connect(string path, Action<WebRequest, WebResponse> callback);

        /// <summary>
        /// Listens for incoming <c>OPTIONS</c> HTTP requests.
        /// </summary>
        /// <inheritdoc cref="All(string, Action{WebRequest, WebResponse})"/>
        public void Options(string path, Action<WebRequest, WebResponse> callback);

        /// <summary>
        /// Listens for incoming <c>TRACE</c> HTTP requests.
        /// </summary>
        /// <inheritdoc cref="All(string, Action{WebRequest, WebResponse})"/>
        public void Trace(string path, Action<WebRequest, WebResponse> callback);

        /// <summary>
        /// Listens for incoming <c>PATCH</c> HTTP requests.
        /// </summary>
        /// <inheritdoc cref="All(string, Action{WebRequest, WebResponse})"/>
        public void Patch(string path, Action<WebRequest, WebResponse> callback);


        /// <summary>
        /// Whether access logging should be enabled or not.
        /// </summary>
        /// <param name="enabled">Whether access logs are enabled.</param>
        public void SetLogging(bool enabled);
    }
}
