using CNCO.Unify.Notifications.Push.Eventing;

namespace CNCO.Unify.Notifications.Push;

/// <summary>
/// Push notification.
/// </summary>
public class PushNotification : IPushNotification
{
  private static INotificationManager NotificationManager =>
    NotificationRuntime.NotificationManager;

  private readonly Guid _id = Guid.NewGuid();

  public Guid Id => _id;
  public string Title { get; set; }
  public List<string> Attributes { get; set; } = [];
  public NotificationContents Contents { get; set; }
  public DateTime? Timestamp { get; set; } = DateTime.Now;
  public string Group { get; set; } = "default";
  public NotificationPriority Priority { get; set; }
  public bool IsSilent =>
    Priority == NotificationPriority.Low || Priority == NotificationPriority.Minimum;

  public NotificationCategory Category { get; set; } = NotificationCategory.Standard;

  public PushNotification(string title, NotificationContents? contents = null)
  {
    Title = title;
    Contents = contents ?? new NotificationContents();
  }

  public void Send() => NotificationManager.Send(this);

  // clean up images?
  public void Cancel() => NotificationManager.Cancel(this);

  public event NotificationActivatedEventHandler? NotificationActivated;

  public event NotificationFailedEventHandler? NotificationFailed;
  public event NotificationDismissedEventHandler? NotificationDismissed;

  public void OnActivated(NotificationActivationArguments args) =>
    NotificationActivated?.Invoke(this, args);

  public void OnFailed(NotificationFailureReason reason, string? details) =>
    NotificationFailed?.Invoke(this, reason, details);

  public void OnDimsissed(NotificationDismissalReason reason) =>
    NotificationDismissed?.Invoke(this, reason);
}
