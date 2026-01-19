using CNCO.Unify.Notifications.Platforms.Windows;

namespace CNCO.Unify.Notifications.Push;

/// <summary>
/// This is ONLY used in place of platform specific notification managers.
/// This is what, say, the <see cref="WindowsNotificationManager"/>
/// becomes when compiling for Linux.
/// All it does is throw <see cref="UnsupportedPlatformException"/> for each method.
///
/// DO NOT USE THIS CLASS.
/// </summary>
internal class UnsupportedPlatformNotificationsManager : IPlatformPushNotificationManager
{
  public Task RegisterAsync() => throw UnsupportedPlatformException.FromMethod();

  public Task UnregisterAsync() => throw UnsupportedPlatformException.FromMethod();

  public void ClearAll() => UnsupportedPlatformException.ThrowFromMethod();

  public Task<bool> SendAsync(IPushNotification pushNotification) =>
    throw UnsupportedPlatformException.FromMethod();

  public PushNotificationUpdateResult Update(IPushNotification pushNotification) =>
    throw UnsupportedPlatformException.FromMethod();

  public bool Cancel(IPushNotification pushNotification) =>
    throw UnsupportedPlatformException.FromMethod();
}
