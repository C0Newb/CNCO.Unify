namespace CNCO.Unify.Notifications.Push;

/// <summary>
/// Notification grouping data.
/// </summary>
public class NotificationGroup
{
  /// <summary>
  /// Unique identifier for the notification group.
  /// </summary>
  public string Id { get; set; }

  /// <summary>
  /// Text displayed for the group.
  /// </summary>
  public string? Name { get; set; }


  public NotificationGroup(string id, string? name = null)
  {
    Id = id;
    Name = name ?? string.Empty;
  }
}
