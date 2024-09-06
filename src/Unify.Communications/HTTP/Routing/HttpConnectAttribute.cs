using System.Diagnostics.CodeAnalysis;

namespace CNCO.Unify.Communications.Http.Routing {
    public class HttpConnectAttribute : HttpMethodAttribute {
        private static readonly IEnumerable<HttpVerb> _method = [HttpVerb.Connect];

        public HttpConnectAttribute() : base(_method) { }

        public HttpConnectAttribute([StringSyntax("Route")] string template) : base(_method, template) {
            ArgumentNullException.ThrowIfNull(template);
        }
    }
}
