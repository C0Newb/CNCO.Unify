using System.Diagnostics.CodeAnalysis;

namespace CNCO.Unify.Communications.Http.Routing {
    public sealed class HttpAnyAttribute : HttpMethodAttribute {
        private static readonly IEnumerable<HttpVerb> _method = [HttpVerb.Any];

        public HttpAnyAttribute() : base(_method) { }

        public HttpAnyAttribute([StringSyntax("Route")] string template) : base(_method, template) {
            ArgumentNullException.ThrowIfNull(template);
        }
    }
}
