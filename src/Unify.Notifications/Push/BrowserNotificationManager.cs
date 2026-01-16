using System.Runtime.InteropServices.JavaScript;

namespace CNCO.Unify.Notifications.Push;

internal class BrowserNotificationManager : INotificationManager
{
  public PlatformID PlatformId => PlatformID.Other;

  public void Cancel(IPushNotification pushNotification) =>
    BrowserNotificationManagerInterop.Cancel(pushNotification.Id.ToString());

  public void ClearAll() => BrowserNotificationManagerInterop.ClearAll();

  public void Register() => BrowserNotificationManagerInterop.Register();

  public void Send(IPushNotification pushNotification) =>
    BrowserNotificationManagerInterop.Send(pushNotification.Contents.Text);

  public void Unregister() => BrowserNotificationManagerInterop.Unregister();

  public void Update(IPushNotification pushNotification) =>
    BrowserNotificationManagerInterop.Update(pushNotification.Id.ToString());
}

internal static partial class BrowserNotificationManagerInterop
{
  [JSImport("globalThis.Unify.Notifications.Push.Update")]
  internal static partial void Cancel(string v);

  [JSImport("globalThis.Unify.Notifications.Push.ClearAll")]
  internal static partial void ClearAll();

  [JSImport("globalThis.Unify.Notifications.Push.Register")]
  internal static partial void Register();

  [JSImport("globalThis.Unify.Notifications.Push.Send")]
  internal static partial void Send(string message);

  [JSImport("globalThis.Unify.Notifications.Push.Unregister")]
  internal static partial void Unregister();

  [JSImport("globalThis.Unify.Notifications.Push.Update")]
  internal static partial void Update(string id);
}
