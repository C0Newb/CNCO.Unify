using System.Diagnostics.CodeAnalysis;

namespace CNCO.Unify.Communications.Http.Routing {
    public sealed class HttpHeadAttribute : HttpMethodAttribute {
        private static readonly IEnumerable<HttpVerb> _method = [HttpVerb.Head];

        public HttpHeadAttribute() : base(_method) { }

        public HttpHeadAttribute([StringSyntax("Route")] string template) : base(_method, template) {
            ArgumentNullException.ThrowIfNull(template);
        }
    }
}
