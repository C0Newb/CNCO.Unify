using System.Diagnostics.CodeAnalysis;

namespace CNCO.Unify.Communications.Http.Routing {
    public class HttpPostAttribute : HttpMethodAttribute {
        private static readonly IEnumerable<HttpMethod> _method = [HttpMethod.Post];

        public HttpPostAttribute() : base(_method) { }

        public HttpPostAttribute([StringSyntax("Route")] string template) : base(_method, template) {
            ArgumentNullException.ThrowIfNull(template);
        }
    }
}
