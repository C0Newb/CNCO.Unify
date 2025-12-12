using CNCO.Unify.Communications.Http;

namespace CNCO.Unify.Communications;

/// <summary>
/// Configuration for the communications namespace.
/// </summary>
public sealed class CommunicationsRuntimeConfiguration : IRuntimeConfiguration
{
  public CommunicationsRuntimeConfiguration() { }

  /// <summary>
  /// Configuration options for the <see cref="Communications.Http"/> namespace.
  /// </summary>
  public HttpRuntimeConfiguration Http { get; set; } = new HttpRuntimeConfiguration();
}
