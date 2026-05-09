#if ANDROID_PUSH_NOTIFICATIONS
using CNCO.Unify.Notifications.Platforms.Android;
#endif
#if IOS_PUSH_NOTIFICATIONS || MAC_PUSH_NOTIFICATIONS
using CNCO.Unify.Notifications.Platforms.Apple;
#endif
#if WASM_PUSH_NOTIFICATIONS
using CNCO.Unify.Notifications.Platforms.Browser;
#endif
#if LINUX_PUSH_NOTIFICATIONS
using CNCO.Unify.Notifications.Platforms.Linux;
#endif
#if WINDOWS_TOAST_NOTIFICATIONS
using CNCO.Unify.Notifications.Platforms.Windows;
#endif

namespace CNCO.Unify.Notifications.Push;

/// <summary>
/// Notification manager used to manage push notifications.
/// </summary>
/// <remarks>
/// Uses the platform specific implementation of <see cref="INotificationsManager"/>.
/// </remarks>
public class NotificationManager : IPlatformPushNotificationManager
{
  private readonly IPlatformPushNotificationManager _manager;

  /// <summary>
  /// Initializes a new instance of the <see cref="NotificationManager"/> class,
  /// providing access to platform-agnostic notification functionality.
  /// </summary>
  /// <remarks>
  /// Utilizes the platform-specific implementation of <see cref="INotificationsManager"/>.
  /// </remarks>
  internal NotificationManager() => _manager = GetPlatformNotificationManager();

  public void ClearAll() => _manager.ClearAll();

  public Task<bool> SendAsync(IPushNotification pushNotification) =>
    _manager.SendAsync(pushNotification);

  public PushNotificationUpdateResult Update(IPushNotification pushNotification) =>
    _manager.Update(pushNotification);

  public bool Cancel(IPushNotification pushNotification) => _manager.Cancel(pushNotification);

  public Task RegisterAsync() => _manager.RegisterAsync();

  public Task UnregisterAsync() => _manager.UnregisterAsync();

  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Interoperability",
    "CA1416:Validate platform compatibility",
    Justification = "It's being performed."
  )]
  private static IPlatformPushNotificationManager GetPlatformNotificationManager()
  {
#if WINDOWS_TOAST_NOTIFICATIONS
    if (Platform.IsWindows())
    {
      // It is very important to understand that push (toast) notifications on Windows
      // requires a StartMenu shortcut to be created for the application and to register
      // the shortcut with the COM server.
      return new WindowsNotificationManager();
    }
#endif

#if ANDROID
    if (Platform.IsAndroid())
    {
      return new AndroidNotificationManager();
    }
#endif

#if IOS_PUSH_NOTIFICATIONS || MAC_PUSH_NOTIFICATIONS
    if (Platform.IsApple())
    {
      return new AppleNotificationManager();
    }
#endif

#if WASM_PUSH_NOTIFICATIONS
    if (Platform.IsBrowser())
    {
      // Utilizes the JavaScript Notifications API.
      return new WasmNotificationManager();
    }
#endif

#if LINUX_PUSH_NOTIFICATIONS
    if (Platform.IsLinux())
    {
      return new LinuxNotificationManager();
    }
#endif

    NotificationRuntime.Current.RuntimeLog.Warning(
      $"{nameof(NotificationManager)}::{nameof(GetPlatformNotificationManager)}()",
      "Unsupported platform!"
    );

    throw UnsupportedPlatformException.FromMethod();
  }
}
