namespace CNCO.Unify.Communications.Http {
    public interface IWebClient : IDisposable {
        /// <summary>
        /// Sends an HTTP <c>CONNECT</c> request.
        /// </summary>
        /// <remarks>
        /// <c>CONNECT</c> starts two-way communications with the requested resource. It can be used to open a tunnel.
        /// </remarks>
        /// <param name="uri">The Uri the request is sent to.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task<HttpResponseMessage> ConnectAsync(string uri);
        /// <inheritdoc cref="ConnectAsync(string)"/>
        Task<HttpResponseMessage> ConnectAsync(Uri uri);



        /// <summary>
        /// Sends an HTTP <c>DELETE</c> request.
        /// </summary>
        /// <remarks>
        /// <c>DELETE</c> deletes the specified resource.
        /// </remarks>
        /// <param name="uri">The Uri the request is sent to.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task<HttpResponseMessage> DeleteAsync(string uri);
        /// <inheritdoc cref="DeleteAsync(string)"/>
        Task<HttpResponseMessage> DeleteAsync(Uri uri);



        /// <summary>
        /// Sends an HTTP <c>GET</c> request.
        /// </summary>
        /// <remarks>
        /// <c>GET</c> requests a representation of the specified resource.
        /// Requests using GET should only retrieve data and shouldn't include a body.
        /// </remarks>
        /// <param name="uri">The Uri the request is sent to.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task<HttpResponseMessage> GetAsync(string uri);
        
        /// <inheritdoc cref="GetAsync(string)"/>
        Task<HttpResponseMessage> GetAsync(Uri uri);

        /// <inheritdoc cref="GetAsync(string)"/>
        /// <summary>
        /// Sends an HTTP <c>GET</c> request and returns the response as a string.
        /// </summary>
        /// <returns>The server's response as a string.</returns>
        Task<string> GetStringAsync(string uri);
        
        /// <inheritdoc cref="GetStringAsync(string)"/>
        Task<string> GetStringAsync(Uri uri);



        /// <summary>
        /// Sends an HTTP <c>HEAD</c> request. Do not include a body.
        /// </summary>
        /// <remarks>
        /// <c>HEAD</c> requests the headers that would be returned if an HTTP <c>GET</c> request was used instead.
        /// </remarks>
        /// <param name="uri">The Uri the request is sent to.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task<HttpResponseMessage> HeadAsync(string uri);
        /// <inheritdoc cref="HeadAsync(string)"/>
        Task<HttpResponseMessage> HeadAsync(Uri uri);



        /// <summary>
        /// Sends an HTTP <c>OPTIONS</c> request.
        /// </summary>
        /// <remarks>
        /// <c>OPTIONS</c> requests a description of the communication options for the specified resource.
        /// </remarks>
        /// <param name="uri">The Uri the request is sent to.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task<HttpResponseMessage> OptionsAsync(string uri);
        /// <inheritdoc cref="OptionsAsync(string)"/>
        Task<HttpResponseMessage> OptionsAsync(Uri uri);



        /// <summary>
        /// Sends an HTTP <c>PATCH</c> request.
        /// </summary>
        /// <remarks>
        /// <c>PATCH</c> applies partial modifications to a resource.
        /// </remarks>
        /// <param name="uri">The Uri the request is sent to.</param>
        /// <param name="content">The HTTP request content sent to the server.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task<HttpResponseMessage> PatchAsync(string uri, HttpContent content);
        /// <inheritdoc cref="PatchAsync(string, HttpContent)"/>
        Task<HttpResponseMessage> PatchAsync(Uri uri, HttpContent content);



        /// <summary>
        /// Sends an HTTP <c>POST</c> request.
        /// </summary>
        /// <remarks>
        /// <c>POST</c> sends data to the server.
        /// </remarks>
        /// <param name="uri">The Uri the request is sent to.</param>
        /// <param name="content">The HTTP request content sent to the server.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task<HttpResponseMessage> PostAsync(string uri, HttpContent content);
        /// <inheritdoc cref="PostAsync(string, HttpContent)"/>
        Task<HttpResponseMessage> PostAsync(Uri uri, HttpContent content);



        /// <summary>
        /// Sends an HTTP <c>PUT</c> request.
        /// </summary>
        /// <remarks>
        /// <c>PUT</c> creates a new resource or updates a resource that already exists.
        /// </remarks>
        /// <param name="uri">The Uri the request is sent to.</param>
        /// <param name="content">The HTTP request content sent to the server.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task<HttpResponseMessage> PutAsync(string uri, HttpContent content);
        /// <inheritdoc cref="PutAsync(string, HttpContent)"/>
        Task<HttpResponseMessage> PutAsync(Uri uri, HttpContent content);



        /// <summary>
        /// Sends an HTTP <c>TRACE</c> request.
        /// </summary>
        /// <remarks>
        /// <c>TRACE</c> performs a message loop-back test along the path to the target resource.
        /// </remarks>
        /// <param name="uri">The Uri the request is sent to.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task<HttpResponseMessage> TraceAsync(string uri);
        /// <inheritdoc cref="TraceAsync(string)"/>
        Task<HttpResponseMessage> TraceAsync(Uri uri);
    }
}
