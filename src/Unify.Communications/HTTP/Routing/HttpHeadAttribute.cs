using System.Diagnostics.CodeAnalysis;

namespace CNCO.Unify.Communications.Http.Routing {
    public class HttpHeadAttribute : HttpMethodAttribute {
        private static readonly IEnumerable<HttpMethod> _method = [HttpMethod.Head];

        public HttpHeadAttribute() : base(_method) { }

        public HttpHeadAttribute([StringSyntax("Route")] string template) : base(_method, template) {
            ArgumentNullException.ThrowIfNull(template);
        }
    }
}
