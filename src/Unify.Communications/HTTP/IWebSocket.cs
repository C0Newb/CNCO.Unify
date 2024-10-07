using System.Collections.Specialized;
using System.Net;
using System.Security.Principal;

namespace CNCO.Unify.Communications.Http {
    /// <summary>
    /// WebSockets.
    /// </summary>
    /// <remarks>
    /// This has identical properties to <see cref="System.Net.WebSockets.WebSocketContext"/>.
    /// </remarks>
    public interface IWebSocket {
        /// <summary>
        /// The cookies that were passed to the server during the opening handshake.
        /// </summary>
        CookieCollection CookieCollection { get; }

        /// <summary>
        /// The HTTP headers that were sent to the server during the opening handshake.
        /// </summary>
        NameValueCollection Headers { get; }

        /// <summary>
        /// Whether the WebSocket client is authenticated.
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Whether the WebSocket client connected from the local machine.
        /// </summary>
        bool IsLocal { get; }

        /// <summary>
        /// Whether the WebSocket connection is secured using Secure Sockets Layer (SSL).
        /// </summary>
        bool IsSecureConnection { get; }

        /// <summary>
        /// The value of the Origin HTTP header included in the opening handshake.
        /// </summary>
        string Origin { get; }

        /// <summary>
        /// The URI requested by the WebSocket client.
        /// </summary>
        Uri RequestUri { get; }

        /// <summary>
        /// The value of the <c>Sec-WebSocket-Key</c> HTTP header included in the opening handshake.
        /// </summary>
        string SecWebSocketKey { get; }

        /// <summary>
        /// The list of subprotocols requested by the WebSocket client.
        /// </summary>
        IEnumerable<string> SecWebSocketProtocols { get; }

        /// <summary>
        /// Which version of WebSocket is being used for this connection.
        /// </summary>
        /// <remarks>
        /// The value of the <c>Sec-WebSocket-Version</c> HTTP header included in the opening handshake.
        /// </remarks>
        string SecWebSocketVersion { get; }

        /// <summary>
        /// An object used to obtain identity, authentication information, and security roles for the WebSocket client.
        /// </summary>
        IPrincipal? User { get; }

        /// <summary>
        /// The underlying WebSocket instance used to interact (send, receive, close, etc) with the WebSocket connection.
        /// </summary>
        System.Net.WebSockets.WebSocket Socket { get; }

        /// <summary>
        /// Indicates whether the WebSocket is connecting or connected. That is, the WebSocket is not, nor has been, closed.
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// Web request that initiated the WebSocket connection.
        /// </summary>
        IWebRequest WebRequest { get; }


        #region Events
        /// <summary>
        /// Fired whenever a new WebSocket message (either text or binary) is received.
        /// </summary>
        event EventHandler<WebSocketMessageReceivedEventArgs>? MessageReceived;

        /// <summary>
        /// Fired whenever the WebSocket connection has been closed.
        /// </summary>
        event EventHandler<WebSocketConnectionClosedEventArgs>? ConnectionClosed;
        #endregion


        /// <summary>
        /// Thread that is listening to and handling incoming WebSocket messages.
        /// </summary>
        Task ListenerTask { get; }
    }
}