using System.Collections.Specialized;
using System.Net;

namespace CNCO.Unify.Communications.Http;


/// <summary>
/// Configuration settings for the <see cref="Http"/> namespace.
/// </summary>
public sealed class HttpRuntimeConfiguration
{
  private string? _apiVersionedRouteTemplatePrefix;

  public HttpRuntimeConfiguration() { }

  #region Routing templates.
  /// <summary>
  /// Prefix to all <see cref="Routing.RouteAttribute"/>s.
  /// </summary>
  public string GlobalRouteAttributePrefix { get; set; } = string.Empty;

  /// <summary>
  /// Version used by <see cref="Routing.VersionedRouteAttribute"/> or <see cref="Routing.ApiVersionedRouteAttribute"/> if the passed version is invalid (less than 1).
  /// </summary>
  public int FallbackApiVersion { get; set; } = 1;

  /// <summary>
  /// Prefix used in <see cref="Routing.ApiRouteAttribute"/>.
  /// </summary>
  public string ApiRouteTemplatePrefix { get; set; } = "api/";

  /// <summary>
  /// Prefix used in <see cref="Routing.VersionedRouteAttribute"/>. <c>{0}</c> will be substituted by the version.
  /// </summary>
  public string VersionedRouteTemplatePrefix { get; set; } = "v{0}";

  /// <summary>
  /// Prefix used in <see cref="Routing.ApiVersionedRouteAttribute"/>. <c>{0}</c> will be substituted by the version.
  /// </summary>
  /// <remarks>
  /// Defaults to a combination of <see cref="ApiRouteTemplatePrefix"/> and <see cref="VersionedRouteTemplatePrefix"/>.
  /// Set this to null to use the default value.
  /// </remarks>
  public string ApiVersionedRouteTemplatePrefix
  {
    get
    {
      if (_apiVersionedRouteTemplatePrefix != null)
        return _apiVersionedRouteTemplatePrefix;

      return ApiRouteTemplatePrefix + VersionedRouteTemplatePrefix;
    }
    set => _apiVersionedRouteTemplatePrefix = value;
  }
  #endregion


  #region Router
  public RouterRuntimeConfiguration Router { get; set; } = new RouterRuntimeConfiguration();
  #endregion


  #region WebSockets

  #endregion


  #region Http server
  /// <summary>
  /// Response headers added to the <see cref="HttpListenerResponse"/> within <see cref="WebServer"/>.
  /// </summary>
  /// <remarks>
  /// That is, these headers are added, by default, to all responses to requests to <see cref="WebServer"/>.
  /// </remarks>
  public NameValueCollection DefaultWebServerResponseHeaders { get; set; } = [];
  #endregion
}