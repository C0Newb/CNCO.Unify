using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace CNCO.Unify.Communications;

internal static class NetworkInterfaces
{
  public static IEnumerable<IPAddress> GetSystemIPAddresses(
    bool includeIPv4 = true,
    bool includeIPv6 = false
  ) =>
    GetNetworkInterfaces()
      .SelectMany(i => i.GetIPProperties().UnicastAddresses)
      .Select(u => u.Address)
      .Where(a =>
        (includeIPv4 && a.AddressFamily == AddressFamily.InterNetwork)
        || (includeIPv6 && a.AddressFamily == AddressFamily.InterNetworkV6 && a.IsIPv6LinkLocal)
      );

  public static IEnumerable<NetworkInterface> GetNetworkInterfaces()
  {
    var networkInterfaces = GetNetworkInterfaces(false);

    if (networkInterfaces.Any())
    {
      return networkInterfaces;
    }

    // Include loopbacks, I guess..
    return GetNetworkInterfaces(true);
  }

  public static IEnumerable<NetworkInterface> GetNetworkInterfaces(bool includeLoopback) =>
    NetworkInterface
      .GetAllNetworkInterfaces()
      .Where(nic =>
        nic.OperationalStatus == OperationalStatus.Up // is UP
        && (includeLoopback || nic.NetworkInterfaceType != NetworkInterfaceType.Loopback) // not a loopback
      );
}
