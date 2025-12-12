namespace CNCO.Unify.Notifications.Push;

/// <summary>
/// Specifically used by platform specific notification managers.
/// </summary>
public interface IPlatformNotificationManager
{
  public PlatformID PlatformId { get; }

  /// <summary>
  /// Sets up push notification support for this operating system.
  /// </summary>
  public void Register();

  /// <summary>
  /// Unregisters push notification support for this application on the current operating system.
  /// </summary>
  public void Unregister();
}
