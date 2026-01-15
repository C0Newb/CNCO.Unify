using System.Net.NetworkInformation;

namespace CNCO.Unify.Communications.Mdns;

public class NetworkInterfaceEventArgs
{
  /// <summary>
  /// The collection of network interfaces that were discovered.
  /// </summary>
  public required IEnumerable<NetworkInterface> NetworkInterfaces { get; init; }
}
