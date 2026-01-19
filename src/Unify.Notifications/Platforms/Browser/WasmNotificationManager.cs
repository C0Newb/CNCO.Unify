#if !WASM_PUSH_NOTIFICATIONS
using CNCO.Unify.Notifications.Push;
using System.Runtime.Versioning;

namespace CNCO.Unify.Notifications.Platforms.Browser;

[SupportedOSPlatform("browser")]
internal class WasmNotificationManager : UnsupportedPlatformNotificationsManager { }
#else
using CNCO.Unify.Notifications.Push;
using CNCO.Unify.Notifications.Push.Actions;
using CNCO.Unify.Notifications.Push.Eventing;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Text;

namespace CNCO.Unify.Notifications.Platforms.Browser;

[SupportedOSPlatform("browser")]
internal class WasmNotificationManager : IPlatformPushNotificationManager
{
  public const string JSModuleName = "NotificationsModule";
  public const int MaxMessageLength = 250;

  public static bool IsNotificationsAvailable => JSHost.GlobalThis.HasProperty("Notification");

  public static int MaxActions => NotificationsInterop.MaxActions;
  public static NotificationsPermission Permissions =>
    JSStringToNotificationsPermission(NotificationsInterop.Permission);

  public bool Cancel(IPushNotification pushNotification)
  {
    AssertOnBrowserNotificationSupport();

    return NotificationsInterop.Cancel(pushNotification.Id.ToString(), pushNotification.Group);
  }

  public void ClearAll()
  {
    AssertOnBrowserNotificationSupport();

    NotificationsInterop.ClearAll();
  }

  public async Task RegisterAsync()
  {
    AssertOnBrowserNotificationSupport();

    var assembly = Assembly.GetExecutingAssembly();
    await ImportJavaScriptResourceAsync(assembly, "Browser.UnifyNotifications.js");

    if (Permissions == NotificationsPermission.Granted)
    {
      // Registered, no need to request.
      return;
    }

    var registeredResult = JSStringToNotificationsPermission(await NotificationsInterop.Register());
    if (registeredResult != NotificationsPermission.Granted)
    {
      NotificationRuntime.Current.RuntimeLog.Warning("Failed to register browser notifications.");
    }
  }

  public async Task<bool> SendAsync(IPushNotification pushNotification)
  {
    AssertOnBrowserNotificationSupport();

    // click, close, error!

    return await NotificationsInterop.Send(
      BrowserPushNotification.FromPushNotification(pushNotification, out _),
      () =>
      {
        // Get button name
        var activatedButtonId = NotificationsInterop.GetNotificationActivationReason(
          pushNotification.Id.ToString(),
          pushNotification.Group
        );

        var activationArgs = new NotificationActivationArguments(
          new Dictionary<INotificationAction, string?>(),
          pushNotification.Contents?.GetNotificationAction(activatedButtonId)
        );
        pushNotification?.OnActivated(activationArgs);
      },
      () => pushNotification.OnDismissed(NotificationDismissalReason.UserCanceled),
      () =>
      {
        if (Permissions != NotificationsPermission.Granted)
        {
          pushNotification.OnFailed(
            NotificationFailureReason.DisabledForApplication,
            "Permissions disabled/not allowed by user."
          );
        }
        // Hell if I know
        pushNotification.OnFailed(NotificationFailureReason.Exception, "Unknown");
      }
    );
  }

  public Task UnregisterAsync()
  {
    AssertOnBrowserNotificationSupport();
    return Task.CompletedTask;
  }

  public PushNotificationUpdateResult Update(IPushNotification pushNotification) =>
    PushNotificationUpdateResult.Failed;

  private void AssertOnBrowserNotificationSupport()
  {
    if (!IsNotificationsAvailable)
    {
      UnsupportedPlatformException.ThrowWithMessage(
        "Browser does not provide the Notification API. Notifications not supported."
      );
    }
  }

  private static NotificationsPermission JSStringToNotificationsPermission(string? permission) =>
    permission switch
    {
      "granted" => NotificationsPermission.Granted,
      "denied" => NotificationsPermission.Denied,
      _ => NotificationsPermission.Default,
    };

  private static async Task ImportJavaScriptResourceAsync(Assembly assembly, string resourceName)
  {
    var javaScriptLibraryResourceName = assembly
      .GetManifestResourceNames()
      .FirstOrDefault(r => r.EndsWith(resourceName));
    if (string.IsNullOrEmpty(javaScriptLibraryResourceName))
    {
      UnsupportedPlatformException.ThrowWithMessage(
        "Unable to load NotificationManager JS library."
      );
    }

    using var stream = assembly.GetManifestResourceStream(javaScriptLibraryResourceName);
    if (stream == null)
    {
      UnsupportedPlatformException.ThrowWithMessage(
        "Unable to load NotificationManager JS library."
      );
    }

    using var reader = new StreamReader(stream, Encoding.UTF8);
    string jsCode = await reader.ReadToEndAsync();

    var jsObjectUrlCode =
      $"URL.createObjectURL(new Blob([`{jsCode.Replace("`", "\\`").Replace("$", "\\$")}`], {{type: 'text/javascript'}}))";
    var blobUrl =
      NotificationsInterop.EvalToString(jsObjectUrlCode)
      ?? throw UnsupportedPlatformException.FromMessage(
        "Failed to import NotificationManager JS library."
      );

    // Register the Blob URL under a friendly module name
    _ = await JSHost.ImportAsync(JSModuleName, blobUrl);
  }
}
#endif
