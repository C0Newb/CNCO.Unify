using System.Diagnostics.CodeAnalysis;

namespace CNCO.Unify.Communications.Http.Routing {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ControllerAttribute : Attribute, IRouteTemplate {
        public ControllerAttribute() { }

        public ControllerAttribute([StringSyntax("Route")] string template) {
            ArgumentNullException.ThrowIfNull(nameof(template));

            template = template.TrimStart('/');

            string globalPrefix = CommunicationsRuntime.Current.Configuration.RuntimeHttpConfiguration.GlobalRouteAttributePrefix;
            if (!string.IsNullOrEmpty(globalPrefix)) {
                Template = globalPrefix.TrimEnd('/') + '/' + template;
            } else {
                Template = template;
            }
        }

        /// <inheritdoc/>
        [StringSyntax("Route")]
        public string? Template { get; }
    }
}
