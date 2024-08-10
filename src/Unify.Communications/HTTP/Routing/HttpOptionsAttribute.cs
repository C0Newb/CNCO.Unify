using System.Diagnostics.CodeAnalysis;

namespace CNCO.Unify.Communications.Http.Routing {
    public class HttpOptionsAttribute : HttpMethodAttribute {
        private static readonly IEnumerable<HttpMethod> _method = [HttpMethod.Options];

        public HttpOptionsAttribute() : base(_method) { }

        public HttpOptionsAttribute([StringSyntax("Route")] string template) : base(_method, template) {
            ArgumentNullException.ThrowIfNull(template);
        }
    }
}
