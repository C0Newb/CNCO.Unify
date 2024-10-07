using CNCO.Unify.Communications.Http.Routing;
using System.Collections.Specialized;
using System.Net;

namespace CNCO.Unify.Communications.Http {
    /// <summary>
    /// Represents the HTTP request.
    /// Contains the request query string, parameters, HTTP headers, cookies, and more.
    /// </summary>
    public interface IWebRequest {
        /// <summary>
        /// Incoming request stream.
        /// </summary>
        Stream BodyStream { get; set; }

        /// <summary>
        /// List of cookies sent in the <see cref="WebRequest"/>.
        /// </summary>
        CookieCollection Cookies { get; set; }

        /// <summary>
        /// This will be the domain, or host, the request was sent to, such as <c>localhost:8008</c>.
        /// </summary>
        string? Domain { get; }

        /// <summary>
        /// Request headers.
        /// </summary>
        NameValueCollection Headers { get; set; }

        /// <summary>
        /// This is the page being hit, such as <c>/api/v1/getData</c>.
        /// </summary>
        string? Path { get; }

        /// <summary>
        /// This is the protocol used to make the request.
        /// </summary>
        Protocol Protocol { get; set; }

        /// <summary>
        /// The queries sent in the URL.
        /// </summary>
        NameValueCollection Query { get; set; }

        /// <summary>
        /// Gets the query string at the end of the URL, such as <c>?myParameter=abc123</c>.
        /// </summary>
        string? QueryString { get; }

        /// <summary>
        /// The remote IP of the requester.
        /// </summary>
        IPAddress? RemoteAddress { get; set; }

        /// <summary>
        /// Route information.
        /// </summary>
        RouteTemplate? RouteTemplate { get; set; }

        /// <summary>
        /// This is the full URL, such as <c>localhost:8008/api/v1/getData?myParameter=abc123</c>
        /// </summary>
        Uri? Uri { get; set; }

        /// <summary>
        /// Gets the value of the <c>User-Agent</c> HTTP header.
        /// </summary>
        string? UserAgent { get; }

        /// <summary>
        /// The HTTP verb used for this request.
        /// </summary>
        HttpVerb? Verb { get; set; }

        /// <summary>
        /// If <see cref="CreateWebSocketConnection()"/> is called, the newly created WebSocket will be stored here.
        /// </summary>
        WebSocket? WebSocket { get; }


        /// <summary>
        /// Completes the WebSocket handshake on this request and returns the newly created <see cref="Http.WebSocket"/>.
        /// </summary>
        /// <remarks>
        /// This will block execution while the WebSocket upgrade/connection handshake is being completed.
        /// </remarks>
        /// <returns>Newly created WebSocket.</returns>
        public WebSocket CreateWebSocketConnection();
        /// <inheritdoc cref="CreateWebSocketConnection()"/>
        /// <param name="keepAliveInterval">The WebSocket protocol keep-alive interval in milliseconds.</param>
        public WebSocket CreateWebSocketConnection(string? subProtocol, TimeSpan? keepAliveInterval);

        /// <inheritdoc cref="CreateWebSocketConnection(string?, TimeSpan?)"/>
        /// <param name="receiveBufferSize">The receive buffer size in bytes.</param>
        public WebSocket CreateWebSocketConnection(string? subProtocol, int? receiveBufferSize, TimeSpan? keepAliveInterval);
    }
}