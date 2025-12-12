using System.Runtime.Versioning;

namespace CNCO.Unify.Notifications.Push;

/// <summary>
/// Windows push (toast) notifications.
/// </summary>
/// <remarks>
/// It is very important to understand that push (toast) notifications on Windows
/// requires a StartMenu shortcut to be created for the application and to register
/// the shortcut with the COM server.
/// </remarks>
[SupportedOSPlatform("windows")]
internal class WindowsNotificationManager : INotificationManager
{
#if WINDOWS
  private Platforms.Windows.WindowsNotificationManager _manager;
#endif

  public PlatformID PlatformId => PlatformID.Win32NT;

  public WindowsNotificationManager()
  {
#if WINDOWS
    _manager = new Platforms.Windows.WindowsNotificationManager();
#endif
  }

  public void Register()
  {
#if WINDOWS
    _manager.Register();
#else
    throw new UnsupportedPlatformException($"{GetType().Name}::{nameof(Register)}");
#endif
  }

  public void Unregister()
  {
#if WINDOWS
    _manager.Unregister();
#else
    throw new UnsupportedPlatformException($"{GetType().Name}::{nameof(Unregister)}");
#endif
  }

  public void ClearAll()
  {
#if WINDOWS
    _manager.ClearAll();
#else
    throw new UnsupportedPlatformException($"{GetType().Name}::{nameof(Unregister)}");
#endif
  }

  public void Send(IPushNotification pushNotification)
  {
#if WINDOWS
    _manager.Send(pushNotification);
#else
    throw new UnsupportedPlatformException($"{GetType().Name}::{nameof(Send)}");
#endif
  }

  public void Update(IPushNotification pushNotification)
  {
#if WINDOWS
    _manager.Update(pushNotification);
#else
    throw new UnsupportedPlatformException($"{GetType().Name}::{nameof(Send)}");
#endif
  }

  public void Cancel(IPushNotification pushNotification)
  {
#if WINDOWS
    _manager.Cancel(pushNotification);
#else
    throw new UnsupportedPlatformException($"{GetType().Name}::{nameof(Send)}");
#endif
  }
}
