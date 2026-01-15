using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace CNCO.Unify.Communications.Mdns;

/// <summary>
/// Logic to send and receive datagrams over multicast sockets.
/// </summary>
public class Multicast : IDisposable
{
  private bool _disposed = false;

  public static readonly int MulticastPort = 5353;

  private static readonly IPAddress _multicastIPv4Address = new IPAddress([224, 0, 0, 251]);
  private static readonly IPEndPoint _mdnsEndpointIPv4 = new IPEndPoint(
    _multicastIPv4Address,
    MulticastPort
  );

  private static readonly IPAddress _multicastIPv6Address = IPAddress.Parse("FF02::FB");
  private static readonly IPEndPoint _mdnsEndpointIPv6 = new IPEndPoint(
    _multicastIPv6Address,
    MulticastPort
  );

  private readonly List<UdpClient> _receivers = [];
  private readonly ConcurrentDictionary<IPAddress, UdpClient> _senders = new();

  public event EventHandler<UdpReceiveResult>? MessageReceived;

  public Multicast(
    IEnumerable<NetworkInterface> interfaces,
    bool? useIPv4 = true,
    bool? useIPv6 = false
  )
  {
    useIPv4 = useIPv4 ?? true;
    useIPv6 = useIPv6 ?? false;

    UdpClient? ipv4Client = null;
    UdpClient? ipv6Client = null;
    if (useIPv4 == true)
    {
      ipv4Client = CreateUdpClient(AddressFamily.InterNetwork, IPAddress.Any);
    }

    if (useIPv6 == true)
    {
      ipv6Client = CreateUdpClient(AddressFamily.InterNetworkV6, IPAddress.IPv6Any);
    }

    // Get the IPs for our interfaces
    var addresses = interfaces
      .SelectMany(GetInterfaceLocalAddresses)
      .Where(a =>
        (useIPv4 == true && a.AddressFamily == AddressFamily.InterNetwork)
        || (useIPv6 == true && a.AddressFamily == AddressFamily.InterNetworkV6)
      );

    foreach (var address in addresses)
    {
      if (_senders.ContainsKey(address))
      {
        continue;
      }

      BindToInterface(ipv4Client, ipv6Client, address);
    }

    foreach (var receiver in _receivers)
    {
      Listen(receiver);
    }
  }

  ~Multicast()
  {
    Dispose(false);
  }

  // This code added to correctly implement the disposable pattern.
  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing)
  {
    if (!disposing || _disposed)
    {
      return;
    }

    _disposed = true;

    MessageReceived = null;

    foreach (var receiver in _receivers)
    {
      try
      {
        receiver.Dispose();
      }
      catch
      {
        // Don't care
      }
    }

    foreach (var address in _senders.Keys)
    {
      if (!_senders.TryRemove(address, out var sender))
      {
        continue;
      }

      try
      {
        sender.Dispose();
      }
      catch
      {
        // Don't care
      }
    }
  }

  /// <summary>
  /// Sends a multicast message to all bound interfaces.
  /// </summary>
  /// <param name="message">Message to be sent.</param>
  /// <returns>Number of bytes sent.</returns>
  public async Task SendMessageAsync(byte[] message)
  {
    List<Task> sendTasks = [];
    foreach (var sender in _senders)
    {
      try
      {
        var endpoint =
          sender.Key.AddressFamily == AddressFamily.InterNetwork
            ? _mdnsEndpointIPv4
            : _mdnsEndpointIPv6;

        sendTasks.Add(
          new Task(async () => _ = await sender.Value.SendAsync(message, message.Length, endpoint))
        );
      }
      catch (Exception ex)
      {
        CommunicationsRuntime.Current.RuntimeLog.Error(
          $"{GetType().Name}::{nameof(SendMessageAsync)}",
          $"Failed to send multicast message over {sender.Key}!",
          ex
        );
      }
    }

    await Task.WhenAll(sendTasks);
  }

  private void Listen(UdpClient udpClient) =>
    _ = Task.Run(async () =>
    {
      try
      {
        var receiveTask = udpClient.ReceiveAsync();
        _ = receiveTask.ContinueWith(
          x => Listen(udpClient),
          TaskContinuationOptions.OnlyOnRanToCompletion
            | TaskContinuationOptions.RunContinuationsAsynchronously
        );
        _ = receiveTask.ContinueWith(
          x => MessageReceived?.Invoke(this, x.Result),
          TaskContinuationOptions.OnlyOnRanToCompletion
            | TaskContinuationOptions.RunContinuationsAsynchronously
        );
        _ = await receiveTask.ConfigureAwait(false);
      }
      catch
      {
        // Don't care.
      }
    });

  private static void BindToInterface(
    UdpClient? ipv4Client,
    UdpClient? ipv6Client,
    IPAddress address
  )
  {
    UdpClient? udpClient = null;
    try
    {
      switch (address.AddressFamily)
      {
        case AddressFamily.InterNetwork:
          if (ipv4Client != null)
          {
            udpClient = CreateInterfaceUdpClient(ipv4Client, address);
          }

          break;
        case AddressFamily.InterNetworkV6:
          if (ipv6Client != null)
          {
            udpClient = CreateInterfaceUdpClient(ipv6Client, address);
          }
          break;

        default:
          throw new NotSupportedException(
            $"Multicast does not support the {address.AddressFamily} address family."
          );
      }
    }
    catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressNotAvailable)
    {
      CommunicationsRuntime.Current.RuntimeLog.Verbose(
        $"Attempt to bind to {address} resulted in an address not available error. Ignoring"
      );
      udpClient?.Dispose();
    }
    catch (Exception ex)
    {
      CommunicationsRuntime.Current.RuntimeLog.Error(
        $"{typeof(Multicast).Name}::{nameof(BindToInterface)}",
        $"Attempt to bind to {address} failed.",
        ex
      );
      udpClient?.Dispose();
    }
  }

  private static UdpClient CreateInterfaceUdpClient(
    UdpClient ipVersionClient,
    IPAddress interfaceAddress
  )
  {
    ArgumentNullException.ThrowIfNull(ipVersionClient);
    ipVersionClient.Client.SetSocketOption(
      SocketOptionLevel.IP,
      SocketOptionName.AddMembership,
      new MulticastOption(_multicastIPv4Address, interfaceAddress)
    );

    var interfaceEndpoint = new IPEndPoint(interfaceAddress, MulticastPort);
    var udpClient = new UdpClient(interfaceEndpoint);
    udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
    udpClient.Client.Bind(interfaceEndpoint);
    udpClient.Client.SetSocketOption(
      SocketOptionLevel.IP,
      SocketOptionName.AddMembership,
      new MulticastOption(_multicastIPv4Address)
    );
    udpClient.Client.SetSocketOption(
      SocketOptionLevel.IP,
      SocketOptionName.MulticastLoopback,
      true
    );
    return udpClient;
  }

  private UdpClient CreateUdpClient(AddressFamily ipv4AddressFamily, IPAddress ipv4AnyAddress)
  {
    var udpClient = new UdpClient(ipv4AddressFamily);
    udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

    if (Platform.IsLinux() || Platform.IsAndroid())
    {
      // Reuse address??
    }

    udpClient.Client.Bind(new IPEndPoint(ipv4AnyAddress, MulticastPort));
    _receivers.Add(udpClient);
    return udpClient;
  }

  private static IEnumerable<IPAddress> GetInterfaceLocalAddresses(
    NetworkInterface networkInterface
  ) =>
    networkInterface
      .GetIPProperties()
      .UnicastAddresses.Select(x => x.Address)
      .Where(x => x.AddressFamily != AddressFamily.InterNetworkV6 || x.IsIPv6LinkLocal);
}
