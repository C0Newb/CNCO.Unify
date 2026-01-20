using CNCO.Unify.Notifications.Push.Eventing;

namespace CNCO.Unify.Notifications.Push.Actions;

/// <summary>
/// Notification action (button, textbox, etc).
/// </summary>
public abstract class NotificationAction : INotificationAction
{
  public string Id { get; } = Guid.NewGuid().ToString();

  public event NotificationActionActivatedEventHandler? ActionActivated;

  public virtual void OnActivated(string? value = null) => ActionActivated?.Invoke(this, value);
}
