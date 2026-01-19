using System.Collections.Specialized;
using System.Net;
using CNCO.Unify.Communications.Http.Routing;

namespace CNCO.Unify.Communications.Http;

/// <summary>
/// Represents the HTTP request.
/// Contains the request query string, parameters, HTTP headers, cookies, and more.
/// </summary>
public interface IWebRequest
{
  /// <summary>
  /// Incoming request stream.
  /// </summary>
  Stream BodyStream { get; init; }

  /// <summary>
  /// Gets the body content of the message as a string.
  /// This is the contents of <see cref="BodyStream"/>.
  /// </summary>
  /// <remarks>
  /// If possible, use the stream. Once read, the body string will remain in memory.
  /// </remarks>
  string Body { get; }

  /// <summary>
  /// List of cookies sent in the <see cref="WebRequest"/>.
  /// </summary>
  CookieCollection Cookies { get; init; }

  /// <summary>
  /// This will be the domain, or host, the request was sent to, such as <c>localhost:8008</c>.
  /// </summary>
  string? Domain { get; }

  /// <summary>
  /// Request headers.
  /// </summary>
  NameValueCollection Headers { get; init; }

  /// <summary>
  /// This is the page being hit, such as <c>/api/v1/getData</c>.
  /// </summary>
  string? Path { get; }

  /// <summary>
  /// This is the protocol used to make the request.
  /// </summary>
  Protocol Protocol { get; init; }

  /// <summary>
  /// Version of the <see cref="Protocol"/> used.
  /// </summary>
  Version ProtocolVersion { get; init; }

  /// <summary>
  /// The queries sent in the URL.
  /// </summary>
  NameValueCollection Query { get; }

  /// <summary>
  /// Gets the query string at the end of the URL, such as <c>?myParameter=abc123</c>.
  /// </summary>
  string? QueryString { get; }

  /// <summary>
  /// The remote IP of the requester.
  /// </summary>
  IPAddress? RemoteAddress { get; init; }

  /// <summary>
  /// Route information.
  /// </summary>
  RouteTemplate? RouteTemplate { get; set; }

  /// <summary>
  /// This is the full URL, such as <c>localhost:8008/api/v1/getData?myParameter=abc123</c>
  /// </summary>
  Uri? Uri { get; init; }

  /// <summary>
  /// Gets the value of the <c>User-Agent</c> HTTP header.
  /// </summary>
  string? UserAgent { get; }

  /// <summary>
  /// The HTTP verb used for this request.
  /// </summary>
  HttpVerb? Verb { get; init; }

  /// <summary>
  /// If <see cref="CreateWebSocketConnectionAsync(string?, int?, TimeSpan?)"/> is called, the newly created WebSocket will be stored here.
  /// </summary>
  WebSocket? WebSocket { get; }

  /// <summary>
  /// Completes the WebSocket handshake on this request and returns the newly created <see cref="Http.WebSocket"/>.
  /// </summary>
  /// <remarks>
  /// This will block execution while the WebSocket upgrade/connection handshake is being completed.
  /// </remarks>
  /// <param name="subProtocol">The supported WebSocket sub-protocol.</param>
  /// <param name="receiveBufferSize">The receive buffer size in bytes.</param>
  /// <param name="keepAliveInterval">The WebSocket protocol keep-alive interval in milliseconds.</param>
  /// <returns>Newly created WebSocket.</returns>
  public Task<WebSocket> CreateWebSocketConnectionAsync(
    string? subProtocol = null,
    int? receiveBufferSize = null,
    TimeSpan? keepAliveInterval = null
  );
}
