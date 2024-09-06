namespace CNCO.Unify.Communications.Http {
    /// <summary>
    /// HTTP methods.
    /// </summary>
    public enum HttpVerb {
        /// <summary>
        /// Used to accept any type of <see cref="HttpVerb"/>. Largely a placeholder.
        /// This is not a valid verb for web requests.
        /// </summary>
        Any,

        /// <summary>
        /// Requests a representation of the specified resource. Only used to retrieve data.
        /// </summary>
        Get,

        /// <summary>
        /// Submits an entity to the specified resource, often causing a change in state or side effects on the server.
        /// </summary>
        Post,

        /// <summary>
        /// Applies partial modifications to a resource.
        /// </summary>
        Patch,

        /// <summary>
        /// Replaces all current representations of the target resource with the request payload.
        /// </summary>
        Put,

        /// <summary>
        /// Deletes the specified resource.
        /// </summary>
        Delete,

        /// <summary>
        /// Performs a message loop-back test along the path to the target resource.
        /// </summary>
        Trace,

        /// <summary>
        /// Asks for response identical to a <see cref="Get"/> request, but without the response body.
        /// </summary>
        Head,

        /// <summary>
        /// Establishes a tunnel to the server identified by the target resource.
        /// </summary>
        Connect,

        /// <summary>
        /// Describes the communication options for the target resource.
        /// </summary>
        Options
    }
}
