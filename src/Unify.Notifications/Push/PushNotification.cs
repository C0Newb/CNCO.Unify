using CNCO.Unify.Notifications.Push.Eventing;

namespace CNCO.Unify.Notifications.Push;

/// <summary>
/// Push notification.
/// </summary>
public class PushNotification(string title, NotificationContents? contents = null)
  : IPushNotification
{
  private static INotificationsManager NotificationManager =>
    NotificationRuntime.NotificationManager;

  private readonly Guid _id = Guid.NewGuid();

  private NotificationContents _contents = contents ?? new NotificationContents();

  public Guid Id => _id;
  public string Title { get; set; } = title;
  public IEnumerable<string>? Attributes { get; set; }
  public NotificationContents Contents
  {
    get => _contents;
    init => _contents = value;
  }
  public DateTime? Timestamp { get; set; } = DateTime.Now;
  public string Group { get; set; } = "default";
  public NotificationPriority Priority { get; set; }
  public bool IsSilent => Priority == NotificationPriority.Low;

  public NotificationCategory Category { get; set; } = NotificationCategory.Standard;

  public void Cancel()
  {
    try
    {
      NotificationManager.Cancel(this);
    }
    finally
    {
      // Clear images
      Contents.DeleteImages();
    }
  }

  public void ClearContents() => _contents = new();

  public void Send() => NotificationManager.SendAsync(this);

  public void SetContents(NotificationContents contents) => _contents = contents;

  public event NotificationActivatedEventHandler? NotificationActivated;

  public event NotificationFailedEventHandler? NotificationFailed;
  public event NotificationDismissedEventHandler? NotificationDismissed;

  public void OnActivated(NotificationActivationArguments args) =>
    NotificationActivated?.Invoke(this, args);

  public void OnFailed(NotificationFailureReason reason, string? details) =>
    NotificationFailed?.Invoke(this, reason, details);

  public void OnDismissed(NotificationDismissalReason reason) =>
    NotificationDismissed?.Invoke(this, reason);
}
