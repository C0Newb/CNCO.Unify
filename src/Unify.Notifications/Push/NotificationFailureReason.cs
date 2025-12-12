namespace CNCO.Unify.Notifications.Push;

public enum NotificationFailureReason
{
  /// <summary>
  /// Unknown reason, see details.
  /// </summary>
  Unknown,

  /// <summary>
  /// An exception was thrown.
  /// </summary>
  Exception,

  /// <summary>
  /// Disabled for this application.
  /// </summary>
  DisabledForApplication,

  /// <summary>
  /// Disabled for the current user or by the user.
  /// </summary>
  DisabledForUser,

  /// <summary>
  /// Disabled device-wide or unsupported.
  /// </summary>
  DisabledForDevice,
}
