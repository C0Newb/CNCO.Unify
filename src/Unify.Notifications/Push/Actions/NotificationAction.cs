using CNCO.Unify.Notifications.Push.Eventing;

namespace CNCO.Unify.Notifications.Push.Actions;

/// <summary>
/// Notification action (button, textbox, etc).
/// </summary>
public abstract class NotificationAction : INotificationAction
{
  /// <summary>
  /// Type of notification action.
  /// </summary>
  public abstract NotificationActionType Type { get; }

  /// <summary>
  /// Name of the action.
  /// </summary>
  public string Id { get; set; }

  public event NotificationActionActivatedEventHandler? ActionActivated;

  public NotificationAction(string id)
  {
    ArgumentNullException.ThrowIfNullOrEmpty(id);
    Id = id;
  }

  public void OnActivated(string? value = null) => ActionActivated?.Invoke(this, value);
}
