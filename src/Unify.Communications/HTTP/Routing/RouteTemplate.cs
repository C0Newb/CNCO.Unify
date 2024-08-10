using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace CNCO.Unify.Communications.Http.Routing {
    /// <summary>
    /// URL pathing.
    /// </summary>
    public sealed class RouteTemplate : IRouteTemplate {
        /// <summary>
        /// Creates a new <see cref="RouteAttribute"/> with the given route template.
        /// </summary>
        /// <param name="template">The route template. May not be null.</param>
        public RouteTemplate([StringSyntax("Route")] string template) {
            Template = template ?? throw new ArgumentNullException(nameof(template));
            RequestRoute = template;
        }

        public RouteTemplate([StringSyntax("Route")] string template, string requestRoute) : this(template) {
            RequestRoute = requestRoute ?? throw new ArgumentNullException(nameof(requestRoute));

            ExtractPathParameters();
        }

        /// <inheritdoc/>
        public string Template { get; }

        /// <summary>
        /// Unmodified request route.
        /// </summary>
        public string RequestRoute { get; }

        /// <summary>
        /// Parameters in the route (request URL).
        /// </summary>
        public IEnumerable<RouteParameter> RouteParameters { get; private set; } = [];

        /// <summary>
        /// A mapping of route parameter names to their values.
        /// </summary>
        public Dictionary<string, RouteParameter> Parameters {
            get {
                var parameters = new Dictionary<string, RouteParameter>();
                foreach (RouteParameter parameter in RouteParameters) {
                    parameters.Add(parameter.Name, parameter);
                }
                return parameters;
            }
        }

        /// <summary>
        /// Pulls the url parameters out of the request url by using the original request path.
        /// </summary>
        private void ExtractPathParameters() {
            var parameters = new List<RouteParameter>();

            string[] requestPathParts = RequestRoute.Trim('/').Split('/');
            string[] originalPathParts = Template.Trim('/').Split('/');

            if (requestPathParts.Length != originalPathParts.Length)
                return;

            for (int i = 0; i < originalPathParts.Length; i++) {
                var part = originalPathParts[i];
                if (
                    !(part.StartsWith(':') && part.EndsWith(':')) &&
                    !(part.StartsWith('{') && part.EndsWith('}'))
                ) {
                    continue;
                }

                // path parameter
                var routeParameter = new RouteParameter(part[1..^1], requestPathParts[i]);
                parameters.Add(routeParameter); // Get the parameter name and value
            }

            RouteParameters = parameters;
        }
    }
}
