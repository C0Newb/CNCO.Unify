using System.Collections.Concurrent;

namespace CNCO.Unify.Communications.Mdns;

/// <summary>
/// Class to track recently processed message IDs to avoid processing duplicates.
/// </summary>
public class ProcessedMessageIds
{
  private ConcurrentDictionary<string, DateTime> ReceivedMessages { get; } =
    new ConcurrentDictionary<string, DateTime>();

  public TimeSpan MessageRetentionDuration { get; set; } = TimeSpan.FromSeconds(5);

  public bool TryAdd(byte[] message)
  {
    _ = Prune();
    return ReceivedMessages.TryAdd(GetMessageId(message), DateTime.UtcNow);
  }

  public int Prune()
  {
    var deadTime = DateTime.UtcNow - MessageRetentionDuration;
    var removedCount = 0;

    foreach (var staleMessage in ReceivedMessages.Where(x => x.Value < deadTime))
    {
      if (ReceivedMessages.TryRemove(staleMessage.Key, out _))
      {
        removedCount++;
      }
    }
    return removedCount;
  }

  internal static string GetMessageId(byte[] message)
  {
    var hash = System.Security.Cryptography.SHA3_256.HashData(message);
    return Convert.ToBase64String(hash);
  }
}
