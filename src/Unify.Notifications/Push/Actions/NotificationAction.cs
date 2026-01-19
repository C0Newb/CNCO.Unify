using CNCO.Unify.Notifications.Push.Eventing;

namespace CNCO.Unify.Notifications.Push.Actions;

/// <summary>
/// Notification action (button, textbox, etc).
/// </summary>
public abstract class NotificationAction : INotificationAction
{
  public string Id { get; } = Guid.NewGuid().ToString();

  public abstract NotificationActionType Type { get; }

  public event NotificationActionActivatedEventHandler? ActionActivated;

  public void OnActivated(string? value = null) => ActionActivated?.Invoke(this, value);
}
