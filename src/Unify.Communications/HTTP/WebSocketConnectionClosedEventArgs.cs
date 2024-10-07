using System.Net.WebSockets;

namespace CNCO.Unify.Communications.Http {
    /// <summary>
    /// Event args when a <see cref="WebSocket"/> connection is closed.
    /// </summary>
    public class WebSocketConnectionClosedEventArgs : EventArgs {
        /// <summary>
        /// WebSocket that was closed.
        /// </summary>
        public WebSocket WebSocket { get; }

#if false
        /// <summary>
        /// HTTP request that initiated the WebSocket connection.
        /// </summary>
        public IWebRequest? WebRequest { get; }

        /// <summary>
        /// HTTP response for the request <see cref="WebRequest"/>.
        /// </summary>
        public IWebResponse? WebResponse { get; }
#endif

        /// <summary>
        /// Indicates the reason why the remote endpoint initiated the close handshake.
        /// </summary>
        public WebSocketCloseStatus? CloseStatus { get; }

        /// <summary>
        /// The optional description has to why the remote endpoint initiated the close handshake.
        /// </summary>
        public string? CloseStatusDescription { get; }

        /// <summary>
        /// Initiates a new instance of <see cref="WebSocketConnectionClosedEventArgs"/>.
        /// </summary>
        /// <param name="webSocket">WebSocket that was closed.</param>
        /// <param name="webSocketReceiveResult">Receive results that contain the closed reasoning.</param>
        public WebSocketConnectionClosedEventArgs(WebSocket webSocket, WebSocketReceiveResult webSocketReceiveResult) {
            WebSocket = webSocket;
            CloseStatus = webSocketReceiveResult.CloseStatus;
            CloseStatusDescription = webSocketReceiveResult.CloseStatusDescription;
        }

#if false
        /// <inheritdoc cref="WebSocketConnectionClosedEventArgs(WebSocket, WebSocketReceiveResult)"/>
        /// <param name="webRequest">HTTP request that initiated the WebSocket connection.</param>
        /// <param name="webResponse">HTTP response to the HTTP request, <paramref name="webRequest"/>.</param>
        public WebSocketConnectionClosedEventArgs(WebSocket webSocket, WebSocketReceiveResult webSocketReceiveResult, IWebRequest webRequest, IWebResponse webResponse) : this(webSocket, webSocketReceiveResult) {
            WebRequest = webRequest;
            WebResponse = webResponse;
        }
#endif
    }
}
