using System.Net;
using System.Net.NetworkInformation;

namespace CNCO.Unify.Communications {
    public static class InterfaceHelpers {

        public static IEnumerable<NetworkInterface> GetNetworkInterfaces(bool includeLoopback = false) => NetworkInterface.GetAllNetworkInterfaces()
            .Where(
                nic => nic.OperationalStatus == OperationalStatus.Up // is UP
                && (includeLoopback || nic.NetworkInterfaceType != NetworkInterfaceType.Loopback) // not a loopback
            ).ToArray();

        public static IEnumerable<IPAddress> GetIPAddresses(bool includeLoopback = false) => GetNetworkInterfaces()
            .SelectMany(nic => nic.GetIPProperties().UnicastAddresses)
            .Select(x => x.Address)
            .ToArray();

        public static IEnumerable<IPAddress> GetLocalIPAddresses(bool includeLoopback = false) => GetIPAddresses(includeLoopback)
            .Where(
                address => address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork // IPv4
                || (address.IsIPv6LinkLocal && address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6) // Local IPv6
            )
            .ToArray();
    }
}
