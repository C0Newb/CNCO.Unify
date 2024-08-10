using System.Diagnostics.CodeAnalysis;

namespace CNCO.Unify.Communications.Http.Routing {
    /// <summary>
    /// Template for a controller route.
    /// </summary>
    public interface IRouteTemplate {
        /// <summary>
        /// Route path template.
        /// </summary>
        /// <remarks>
        /// You can include url parameters by wrapping the parameter in either semicolons (<c>:</c>) or curly-braces (<c>{}</c>).
        /// <br/>
        /// For example, if you wanted to pass an id in a url and use said id, you may set your route template to <c>/contact/{id}/photo</c>.
        /// Then, in your request you'd call something like <c>/contact/123/photo</c> and the parameter <c>id</c> will be set to the value <c>123</c>.
        /// </remarks>
        [StringSyntax("Route")]
        string Template { get; }
    }
}