namespace CNCO.Unify.Notifications.Push;

/// <summary>
/// Push notification priority.
/// </summary>
public enum NotificationPriority
{
  /// <summary>
  /// Unknown. Treated the same as <see cref="Default"/>.
  /// </summary>
  Unknown = 0,

  /// <summary>
  /// Low priority.
  /// </summary>
  Low = 1,

  /// <summary>
  /// No change, default priority.
  /// </summary>
  Default = 2,

  /// <summary>
  /// High priority, urgent notifications.
  /// </summary>
  High = 3,
}
