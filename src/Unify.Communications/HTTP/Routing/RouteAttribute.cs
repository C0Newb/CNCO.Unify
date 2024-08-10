using System.Diagnostics.CodeAnalysis;

namespace CNCO.Unify.Communications.Http.Routing {
    /// <summary>
    /// Applies a <see cref="RouteTemplate"/> to the <see cref="Controller"/> class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RouteAttribute : Attribute, IRouteTemplate {
        /// <summary>
        /// Creates a new <see cref="RouteAttribute"/> with the given route template.
        /// </summary>
        /// <param name="template">The route template. May not be null.</param>
        public RouteAttribute([StringSyntax("Route")] string template) {
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
        public string Template { get; }
    }
}
