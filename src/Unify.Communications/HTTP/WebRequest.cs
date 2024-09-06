using CNCO.Unify.Communications.Http.Routing;
using System.Collections.Specialized;
using System.Net;
using System.Web;

namespace CNCO.Unify.Communications.Http {
    /// <summary>
    /// Represents the HTTP request.
    /// Contains the request query string, parameters, HTTP headers, cookies, and more.
    /// </summary>
    public class WebRequest : IWebRequest {

        #region Request path information
        public Protocol Protocol { get; set; } = Protocol.HTTP;

        private HttpVerb? _httpVerb;
        public HttpVerb? Verb {
            get => _httpVerb;
            set {
                if (_httpVerb == HttpVerb.Any) {
                    throw new InvalidOperationException("The HttpVerb Any cannot be used in requests.");
                }
                _httpVerb = value;
            }
        }

        public Uri? Uri { get; set; }

        public string? Domain {
            get => Uri?.Host;
        }
        
        public string? Path {
            get => Uri?.AbsolutePath;
        }

        public string? QueryString {
            get => Uri?.Query;
        }

        public NameValueCollection Query { get; set; } = new NameValueCollection();

        public RouteTemplate? RouteTemplate { get; set; }
        #endregion


        public readonly Version ProtocolVersion = new Version();

        #region Requestor data
        public NameValueCollection Headers { get; set; } = new NameValueCollection();

        public CookieCollection Cookies { get; set; } = new CookieCollection();

        public IPAddress? RemoteAddress { get; set; }

        public string? UserAgent => Headers["User-Agent"];

        public Stream BodyStream { get; set; } = Stream.Null;
        #endregion

        public WebRequest() { }

        public WebRequest(HttpListenerRequest request) {
            if (Enum.TryParse(request.HttpMethod, true, out HttpVerb verb))
                Verb = verb;

            Uri = request.Url;
            Query = HttpUtility.ParseQueryString(Uri?.Query ?? string.Empty);

            ProtocolVersion = request.ProtocolVersion;
            Protocol = request.IsSecureConnection ? Protocol.HTTPS : Protocol.HTTP;

            RemoteAddress = request.RemoteEndPoint.Address;
            Cookies = request.Cookies;
            Headers = request.Headers;

            BodyStream = request.InputStream;
        }


        /// <summary>
        /// Initializes a new instance of <see cref="WebRequest"/>.
        /// </summary>
        /// <param name="uri">The full URL, including the protocol, domain, path and query string.</param>
        /// <param name="cookies">Request cookies.</param>
        public WebRequest(Uri uri, Cookie[] cookies) {
            Uri = uri;
            if (uri.Scheme.Equals("http", StringComparison.CurrentCultureIgnoreCase))
                Protocol = Protocol.HTTP;
            else if (uri.Scheme.Equals("https", StringComparison.CurrentCultureIgnoreCase))
                Protocol = Protocol.HTTPS;

            Query = HttpUtility.ParseQueryString(uri.Query ?? string.Empty);

            Cookies = [.. cookies];


            RemoteAddress = IPAddress.Loopback;
            BodyStream = Stream.Null;
            ProtocolVersion = new Version(1, 1);
        }
    }
}
