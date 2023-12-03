using System.Text;

namespace CNCO.Unify.Communications.HTTP {
    /// <summary>
    /// Represents an HTTP cookie.
    /// </summary>
    public class HttpCookie {
        #region Properties
        private bool _isDirty = true;

        private string _name;
        private string _value;
        private string? _domain;
        private string? _path;
        private DateTime? _expires;
        private bool _secure = false;
        private bool _httpOnly = false;
        private SameSiteType _sameSite;
        private string? _customProperties;


        /// <summary>
        /// Gets or sets the name of the cookie.
        /// </summary>
        public string Name {
            get => _name;
            set {
                _name = value;
                _isDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the value of the cookie.
        /// </summary>
        public string Value {
            get => _value;
            set {
                _value = value;
                _isDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the domain of the cookie.
        /// </summary>
        public string? Domain {
            get => _domain;
            set {
                _domain = value;
                _isDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the path of the cookie.
        /// </summary>
        public string? Path {
            get => _path;
            set {
                _path = value;
                _isDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the expiration date and time of the cookie.
        /// </summary>
        public DateTime? Expires {
            get => _expires;
            set {
                _expires = value;
                _isDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the cookie should only be sent over secure connections.
        /// </summary>
        public bool Secure {
            get => _secure;
            set {
                _secure = value;
                _isDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the cookie is accessible only through HTTP requests.
        /// </summary>
        public bool HttpOnly {
            get => _httpOnly;
            set {
                _httpOnly = value;
                _isDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the SameSite attribute of the cookie.
        /// </summary>
        public SameSiteType SameSite {
            get => _sameSite;
            set {
                _sameSite = value;
                _isDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets additional custom properties to be appended to the cookie string.
        /// </summary>
        public string? CustomProperties {
            get => _customProperties;
            set {
                _customProperties = value;
                _isDirty = true;
            }
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpCookie"/> class with the specified name and value.
        /// </summary>
        /// <param name="name">The name of the cookie.</param>
        /// <param name="value">The value of the cookie.</param>
        public HttpCookie(string name, string value) {
            _name = name;
            _value = value;

        }

        /// <inheritdoc cref="HttpCookie(string, string)"/>
        /// <param name="domain">The domain of the cookie.</param>
        public HttpCookie(string name, string value, string domain) : this(name, value) {
            Domain = domain;
        }

        /// <inheritdoc cref="HttpCookie(string, string, string)"/>
        /// <param name="path">The path of the cookie.</param>
        public HttpCookie(string name, string value, string domain, string path) : this(name, value, domain) => Path = path;



        /// <summary>
        /// Sets the cookie as a "Session only" cookie by removing the expiration time.
        /// </summary>
        public void SetAsSessionCookie() => Expires = null;

        /// <summary>
        /// Makes the cookie "secure" or "private" by setting HttpOnly, SameSite to Strict, and Secure.
        /// </summary>
        public void MakeSecure() {
            SameSite = SameSiteType.Strict; // same site
            Secure = true;      // only over HTTPS
            HttpOnly = true;    // no JS access
        }

        /// <summary>
        /// Returns whether any changes have been made to the cookie.
        /// </summary>
        /// <returns><c>true</c> if the cookie has been marked as "dirty"; otherwise, <c>false</c>.</returns>
        public bool IsDirty() => _isDirty;

        /// <summary>
        /// Marks the cookie as "clean" indicating that any changes have been pushed out.
        /// </summary>
        public void MarkAsClean() => _isDirty = false;


        /// <summary>
        /// Converts the cookie to its string representation for sending in the "Set-Cookie" header.
        /// </summary>
        /// <returns>The string representation of the cookie.</returns>
        public override string ToString() {
            var cookieBuilder = new StringBuilder();

            cookieBuilder.Append($"{Name}={Uri.EscapeDataString(Value)}");

            if (!string.IsNullOrEmpty(Domain))
                cookieBuilder.Append($"; Domain={Domain}");

            if (!string.IsNullOrEmpty(Path))
                cookieBuilder.Append($"; Path={Path}");

            if (Expires.HasValue)
                cookieBuilder.Append($"; Expires={Expires.Value:R}");

            if (Secure)
                cookieBuilder.Append("; Secure");

            if (HttpOnly)
                cookieBuilder.Append("; HttpOnly");

            if (SameSite != SameSiteType.None)
                cookieBuilder.Append($"; SameSite={SameSite.ToString()}");

            if (!string.IsNullOrEmpty(CustomProperties))
                cookieBuilder.Append(CustomProperties);

            return cookieBuilder.ToString();
        }
    }

    public enum SameSiteType {
        Strict,
        Lax,
        None
    }
}
