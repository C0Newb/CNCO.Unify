using System.Net;
using Makaretu.Dns;

namespace CNCO.Unify.Communications.Mdns;

/// <summary>
/// Provides data for events that are raised when a multicast message is received.
/// </summary>
public record MulticastMessageEventArgs
{
  public required Message Message { get; init; }

  public required IPEndPoint RemoteEndPoint { get; init; }

  public bool IsLegacyUnicast => RemoteEndPoint.Port != Multicast.MulticastPort;
}
