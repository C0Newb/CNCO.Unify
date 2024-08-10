using System.Diagnostics.CodeAnalysis;

namespace CNCO.Unify.Communications.Http.Routing {
    public class HttpPutAttribute : HttpMethodAttribute {
        private static readonly IEnumerable<HttpMethod> _method = [HttpMethod.Put];

        public HttpPutAttribute() : base(_method) { }

        public HttpPutAttribute([StringSyntax("Route")] string template) : base(_method, template) {
            ArgumentNullException.ThrowIfNull(template);
        }
    }
}
