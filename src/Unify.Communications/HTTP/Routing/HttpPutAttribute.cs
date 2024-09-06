using System.Diagnostics.CodeAnalysis;

namespace CNCO.Unify.Communications.Http.Routing {
    public class HttpPutAttribute : HttpMethodAttribute {
        private static readonly IEnumerable<HttpVerb> _method = [HttpVerb.Put];

        public HttpPutAttribute() : base(_method) { }

        public HttpPutAttribute([StringSyntax("Route")] string template) : base(_method, template) {
            ArgumentNullException.ThrowIfNull(template);
        }
    }
}
