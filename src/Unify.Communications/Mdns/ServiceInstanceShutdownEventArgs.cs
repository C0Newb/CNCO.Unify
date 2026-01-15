using Makaretu.Dns;

namespace CNCO.Unify.Communications.Mdns;

public record ServiceInstanceShutdownEventArgs : MulticastMessageEventArgs
{
  /// <summary>
  /// Fully qualified name of the service instance.
  /// </summary>
  /// <seealso cref="ServiceProfile.FullyQualifiedName"/>
  public required DomainName ServiceInstanceName { get; init; }
}
