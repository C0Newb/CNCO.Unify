using System.Diagnostics.CodeAnalysis;

namespace CNCO.Unify.Communications.Http.Routing {
    /// <summary>
    /// Specifies a method in a <see cref="Controller"/> is to be activated on particular <see cref="HttpMethods"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class HttpMethodAttribute : Attribute, IRouteTemplate {
        private readonly List<HttpVerb> _httpMethods;

        public HttpMethodAttribute(IEnumerable<HttpVerb> httpMethods) : this(httpMethods, null) { }

        public HttpMethodAttribute(IEnumerable<HttpVerb> httpMethods, [StringSyntax("Route")] string? template) {
            ArgumentNullException.ThrowIfNull(httpMethods);
            _httpMethods = httpMethods.ToList();
            Template = template;
        }

        /// <summary>
        /// Http methods this method is to be activated on.
        /// </summary>
        public IEnumerable<HttpVerb> HttpMethods => _httpMethods;

        /// <inheritdoc cref="IRouteTemplate.Template"/>
        [StringSyntax("Route")]
        public string? Template { get; }
    }
}
