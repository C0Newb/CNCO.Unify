using System.Collections.Specialized;
using System.Net;
using System.Web;
using CNCO.Unify.Communications.Http.Routing;

namespace CNCO.Unify.Communications.Http;

/// <summary>
/// Represents the HTTP request.
/// Contains the request query string, parameters, HTTP headers, cookies, and more.
/// </summary>
public class WebRequest : IWebRequest
{
  private readonly HttpListenerContext? _listenerContext;

  #region Constructors
  /// <summary>
  /// Initializes a new instance of <see cref="WebRequest"/>.
  /// </summary>
  public WebRequest() => ProtocolVersion = new();

  /// <inheritdoc cref="WebRequest()"/>
  /// <param name="listenerContext">Listener context from <see cref="HttpListener.GetContext()"/>.</param>
  public WebRequest(HttpListenerContext listenerContext)
    : this(listenerContext.Request) => _listenerContext = listenerContext;

  private WebRequest(HttpListenerRequest request)
  {
    if (Enum.TryParse(request.HttpMethod, true, out HttpVerb verb))
    {
      Verb = verb;
    }

    ProtocolVersion = request.ProtocolVersion;
    Protocol = request.IsSecureConnection ? Protocol.HTTPS : Protocol.HTTP;

    RemoteAddress = request.RemoteEndPoint.Address;

    BodyStream = request.InputStream;
  }

  /// <inheritdoc cref="WebRequest()"/>
  /// <param name="uri">The full URL, including the protocol, domain, path and query string.</param>
  /// <param name="cookies">Request cookies.</param>
  public WebRequest(Uri uri, Cookie[] cookies)
  {
    Uri = uri;
    if (uri.Scheme.Equals("http", StringComparison.CurrentCultureIgnoreCase))
    {
      Protocol = Protocol.HTTP;
    }
    else if (uri.Scheme.Equals("https", StringComparison.CurrentCultureIgnoreCase))
    {
      Protocol = Protocol.HTTPS;
    }

    Cookies = [.. cookies];
    Headers = [];

    RemoteAddress = IPAddress.Loopback;
    BodyStream = Stream.Null;
    ProtocolVersion = new Version(1, 1);
  }
  #endregion

  #region Request path information
  public Protocol Protocol { get; init; }

  public Version ProtocolVersion { get; init; }

  public HttpVerb? Verb
  {
    get => field;
    init
    {
      if (value == HttpVerb.Any)
      {
        throw new InvalidOperationException("The HttpVerb Any cannot be used in requests.");
      }
      field = value;
    }
  }

  public Uri? Uri
  {
    // It's done in this manner to speed things up - only load ondemand.
    get => field ?? _listenerContext?.Request?.Url;
    init => field = value;
  }

  public string? Domain => Uri?.Host;

  public string? Path => Uri?.AbsolutePath;

  public string? QueryString => Uri?.Query;

  public NameValueCollection Query => HttpUtility.ParseQueryString(QueryString ?? string.Empty);

  public RouteTemplate? RouteTemplate { get; set; }
  #endregion


  #region Requestor data
  public NameValueCollection Headers
  {
    get => field ?? _listenerContext?.Request?.Headers ?? [];
    init => field = value;
  }

  public CookieCollection Cookies
  {
    get => field ?? _listenerContext?.Request?.Cookies ?? [];
    init => field = value;
  }

  public IPAddress? RemoteAddress
  {
    get => field ?? _listenerContext?.Request?.RemoteEndPoint.Address;
    init => field = value;
  }

  public string? UserAgent => Headers["User-Agent"];

  public Stream BodyStream { get; init; } = Stream.Null;

  public string Body
  {
    get
    {
      if (string.IsNullOrEmpty(field))
      {
        using var reader = new StreamReader(BodyStream, true);
        field = reader.ReadToEnd();
      }
      return field;
    }
  }
  #endregion

  public WebSocket? WebSocket { get; private set; }

  public async Task<WebSocket> CreateWebSocketConnectionAsync(
    string? subProtocol = null,
    int? receiveBufferSize = null,
    TimeSpan? keepAliveInterval = null
  )
  {
    if (_listenerContext == null)
    {
      throw new InvalidOperationException(
        "Cannot create WebSocket connection without a HttpListenerContext."
      );
    }

    WebSocket = await WebSocket.CreateWebSocketConnectionAsync(
      _listenerContext,
      this,
      subProtocol,
      receiveBufferSize,
      keepAliveInterval
    );
    return WebSocket;
  }
}
