namespace CNCO.Unify.Notifications.Push;

public enum PushNotificationUpdateResult
{
  /// <summary>
  /// Notification successfully updated.
  /// </summary>
  Succeeded,

  /// <summary>
  /// Failed to update notification.
  /// </summary>
  Failed,

  /// <summary>
  /// Notification not found.
  /// </summary>
  NotFound,
}
