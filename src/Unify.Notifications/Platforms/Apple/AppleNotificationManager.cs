#if !IOS_PUSH_NOTIFICATIONS && !MAC_PUSH_NOTIFICATIONS
using CNCO.Unify.Notifications.Push;
using System.Runtime.Versioning;

namespace CNCO.Unify.Notifications.Platforms.Apple;

[SupportedOSPlatform("macos")]
[SupportedOSPlatform("ios")]
internal class AppleNotificationManager : UnsupportedPlatformNotificationsManager { }
#else
using CNCO.Unify.Notifications.Push;
using System.Runtime.Versioning;

namespace CNCO.Unify.Notifications.Platforms.Apple;

[SupportedOSPlatform("macos")]
[SupportedOSPlatform("ios")]
internal class AppleNotificationManager : IPlatformPushNotificationManager
{
  public bool Cancel(IPushNotification pushNotification) => throw new NotImplementedException();

  public void ClearAll() => throw new NotImplementedException();

  public Task RegisterAsync() => throw new NotImplementedException();

  public Task<bool> SendAsync(IPushNotification pushNotification) => throw new NotImplementedException();

  public Task UnregisterAsync() => throw new NotImplementedException();

  public PushNotificationUpdateResult Update(IPushNotification pushNotification) =>
    throw new NotImplementedException();
}
#endif
