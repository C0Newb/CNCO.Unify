namespace CNCO.Unify.Notifications.Push;

/// <summary>
/// Notification types.
/// </summary>
public enum NotificationCategory
{
  /// <summary>
  /// Notification type is unknown.
  /// </summary>
  /// <remarks>
  /// Unknown types are treated the same as <see cref="Standard"/> types.
  /// </remarks>
  Unknown = 0,

  /// <summary>
  ///  Notification contains standard information: title, text, and optional buttons.
  /// </summary>
  Standard = 1,

  /// <summary>
  /// Notification is a timer or alarm.
  /// </summary>
  Alarm = 2,

  /// <summary>
  /// Notification is a reminder.
  /// </summary>
  Reminder = 3,

  /// <summary>
  /// Notification contains a progress bar.
  /// </summary>
  Progress = 4,

  /// <summary>
  /// Notification contains media controls.
  /// </summary>
  Media = 5,

  /// <summary>
  /// Notification is for a full screen call.
  /// </summary>
  Call = 6,
}
