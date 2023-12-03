using System.Collections.Specialized;
using System.Net;
using System.Web;

namespace CNCO.Unify.Communications.HTTP {
    /// <summary>
    /// Represents the HTTP request.
    /// Contains the request query string, parameters, HTTP headers, cookies, and more.
    /// </summary>
    public class WebRequest {

        #region Request path information
        /// <summary>
        /// This is the protocol used to make the request.
        /// </summary>
        public Protocol Protocol { get; set; } = Protocol.HTTP;

        /// <summary>
        /// The HTTP verb used for this request.
        /// </summary>
        public HttpVerb Verb { get; set; }

        /// <summary>
        /// This is the full URL, such as <c>localhost:8008/api/v1/getData?myParameter=abc123</c>
        /// </summary>
        public Uri? Uri { get; set; }

        /// <summary>
        /// This will be the domain, or host, the request was sent to, such as <c>localhost:8008</c>.
        /// </summary>
        public string? Domain {
            get => Uri?.Host;
        }

        /// <summary>
        /// This is the page being hit, such as <c>/api/v1/getData</c>.
        /// </summary>
        public string? Path {
            get => Uri?.AbsolutePath;
        }

        /// <summary>
        /// This is the query string at the end of the URL, such as <c>?myParameter=abc123</c>.
        /// </summary>
        public string? QueryString {
            get => Uri?.Query;
        }

        /// <summary>
        /// The queries sent in the URL.
        /// </summary>
        public NameValueCollection Query { get; set; } = new NameValueCollection();

        /// <summary>
        /// Parameters captured by the path.
        /// </summary>
        public NameValueCollection Parameters { get; set; } = new NameValueCollection();
        #endregion


        public readonly Version ProtocolVersion;

        #region Requestor data
        /// <summary>
        /// Request headers.
        /// </summary>
        public NameValueCollection Headers { get; set; } = new NameValueCollection();

        /// <summary>
        /// List of cookies sent in the <see cref="WebRequest"/>.
        /// </summary>
        public CookieCollection Cookies { get; set; }

        /// <summary>
        /// The remote IP of the requester.
        /// </summary>
        public IPAddress RemoteAddress { get; set; }

        public string? UserAgent => Headers["User-Agent"];


        public Stream BodyStream { get; set; }
        #endregion



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
            if (uri.Scheme.ToLower() == "http")
                Protocol = Protocol.HTTP;
            else if (uri.Scheme.ToLower() == "https")
                Protocol = Protocol.HTTPS;

            Query = HttpUtility.ParseQueryString(uri.Query ?? string.Empty);

            Cookies = new CookieCollection();
            foreach (var cookie in cookies) {
                Cookies.Add(cookie);
            }


            RemoteAddress = IPAddress.Loopback;
            BodyStream = Stream.Null;
            ProtocolVersion = new Version(1, 1);
        }

        /// <inheritdoc cref="WebRequest(Uri, Cookie[])"/>
        /// <param name="pathParameters">Parameters passed in the path.</param>
        public WebRequest(Uri uri, Cookie[] cookies, NameValueCollection pathParameters) : this(uri, cookies) {
            Parameters = pathParameters;
        }
    }
}
