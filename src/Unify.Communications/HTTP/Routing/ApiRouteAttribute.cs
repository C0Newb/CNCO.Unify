using System.Diagnostics.CodeAnalysis;

namespace CNCO.Unify.Communications.Http.Routing {
    /// <summary>
    /// Applies a <see cref="RouteTemplate"/> to the <see cref="Controller"/> class but prefixes the route with <c>api/</c>.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="RouteAttribute"/> with the given route template.
    /// </remarks>
    /// <param name="template">The route template. May not be null.</param>
    public class ApiRouteAttribute([StringSyntax("Route")] string template)
        : RouteAttribute( // sorry this looks so ugly :/
            CommunicationsRuntime.Current.Configuration.RuntimeHttpConfiguration.ApiRouteTemplatePrefix.TrimEnd('/') +
            '/' +
            template.TrimStart('/')
        ) {
    }
}
