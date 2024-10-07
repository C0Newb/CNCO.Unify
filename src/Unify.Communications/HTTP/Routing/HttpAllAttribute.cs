using System.Diagnostics.CodeAnalysis;

namespace CNCO.Unify.Communications.Http.Routing {
    public sealed class HttpAllAttribute : HttpMethodAttribute {
        private static readonly IEnumerable<HttpVerb> _method = [
            HttpVerb.Connect,
            HttpVerb.Delete,
            HttpVerb.Get,
            HttpVerb.Head,
            HttpVerb.Options,
            HttpVerb.Patch,
            HttpVerb.Put,
            HttpVerb.Trace,
        ];

        public HttpAllAttribute() : base(_method) { }

        public HttpAllAttribute([StringSyntax("Route")] string template) : base(_method, template) {
            ArgumentNullException.ThrowIfNull(template);
        }
    }
}
