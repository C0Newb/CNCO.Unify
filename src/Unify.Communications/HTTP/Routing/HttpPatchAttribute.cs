using System.Diagnostics.CodeAnalysis;

namespace CNCO.Unify.Communications.Http.Routing {
    public class HttpPatchAttribute : HttpMethodAttribute {
        private static readonly IEnumerable<HttpMethod> _method = [HttpMethod.Patch];

        public HttpPatchAttribute() : base(_method) { }

        public HttpPatchAttribute([StringSyntax("Route")] string template) : base(_method, template) {
            ArgumentNullException.ThrowIfNull(template);
        }
    }
}
