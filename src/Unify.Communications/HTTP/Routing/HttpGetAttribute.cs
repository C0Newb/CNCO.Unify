using System.Diagnostics.CodeAnalysis;

namespace CNCO.Unify.Communications.Http.Routing {
    public sealed class HttpGetAttribute : HttpMethodAttribute {
        private static readonly IEnumerable<HttpVerb> _method = [HttpVerb.Get];

        public HttpGetAttribute() : base(_method) { }

        public HttpGetAttribute([StringSyntax("Route")] string template) : base(_method, template) {
            ArgumentNullException.ThrowIfNull(template);
        }
    }
}
