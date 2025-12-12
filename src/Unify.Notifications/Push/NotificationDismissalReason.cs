namespace CNCO.Unify.Notifications.Push;

public enum NotificationDismissalReason
{
  /// <summary>
  /// Unknown reason.
  /// </summary>
  Unknown,

  /// <summary>
  /// User dismissed the notification.
  /// </summary>
  UserCanceled,

  /// <summary>
  /// Notification timed out and was removed automatically.
  /// </summary>
  /// <remarks>
  /// This is a bit misleading, as for Windows this is never actually raised.
  /// Timed out for Windows occurs whenever the notification hides itself in the action center.
  /// </remarks>
  TimedOut,

  /// <summary>
  /// Application dismissed the notification.
  /// </summary>
  ApplicationHidden,
}
