using System.Diagnostics.CodeAnalysis;

namespace CNCO.Unify.Communications.Http.Routing {
    /// <summary>
    /// Specifies a route in a <see cref="Controller"/> is used to accept WebSocket requests.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class WebSocketAttribute : Attribute, IRouteTemplate {
        public WebSocketAttribute([StringSyntax("Route")] string? template) {
            ArgumentNullException.ThrowIfNull(template);
            Template = template;
        }

        /// <inheritdoc cref="IRouteTemplate.Template"/>
        [StringSyntax("Route")]
        public string? Template { get; }
    }
}
