namespace CNCO.Unify.Notifications.Push;

/// <summary>
/// Specifically used by platform specific notification managers.
/// </summary>
internal interface IPlatformPushNotificationManager : INotificationsManager
{
  /// <summary>
  /// Sets up push notification support for this operating system.
  /// </summary>
  public Task RegisterAsync();

  /// <summary>
  /// Unregisters push notification support for this application on the current operating system.
  /// </summary>
  public Task UnregisterAsync();
}
