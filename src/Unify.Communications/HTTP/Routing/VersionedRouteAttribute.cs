﻿using System.Diagnostics.CodeAnalysis;

namespace CNCO.Unify.Communications.Http.Routing {
    /// <summary>
    /// Applies a <see cref="RouteTemplate"/> to the <see cref="Controller"/> class but with  prefixes the route with <c>api/</c>.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="RouteAttribute"/> with the given route template.
    /// </remarks>
    /// <param name="template">The route template. May not be null.</param>
    public class VersionedRouteAttribute([StringSyntax("Route")] string template, int version = -1)
        : RouteAttribute( // sorry this looks so ugly :/
            string.Format( // puts the actual version (either the fallback or valid) into the template prefix from the config.
                CommunicationsRuntime.Current.Configuration.RuntimeHttpConfiguration.VersionedRouteTemplatePrefix,
                version < 1 ? CommunicationsRuntime.Current.Configuration.RuntimeHttpConfiguration.FallbackApiVersion : version
            ).TrimEnd('/') + '/' + template.TrimStart('/')
        ) {
    }
}
