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
  public const string ImportShimModuleName = "ImportShimModule";
  public const int MaxMessageLength = 250;

  public static bool IsNotificationsAvailable => JSHost.GlobalThis.HasProperty("Notification");

  public static int MaxActions => NotificationsInterop.GetMaxNotificationActions();
  public static NotificationsPermission Permissions =>
    JSStringToNotificationsPermission(NotificationsInterop.Permission);

  private static bool _importShimInstalled = false;

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
    await ImportJavaScriptResourceAsync(assembly, "UnifyNotifications.js");

    if (Permissions == NotificationsPermission.Granted)
    {
      // Registered, no need to request.
      return;
    }

    var registeredResult = JSStringToNotificationsPermission(
      await NotificationsInterop.RegisterAsync()
    );
    if (registeredResult != NotificationsPermission.Granted)
    {
      NotificationRuntime.Current.RuntimeLog.Error(
        "Failed to register browser notifications as we were not permitted."
      );
    }

    // Register/ready-up service worker
    try
    {
      //var serviceWorkerModuleUrl = (await GetModuleUrl(assembly, "ServiceWorker.js"));
      var serviceWorkerModuleUrl = "ServiceWorker.js";
      if (await NotificationsInterop.RegisterServiceWorker(serviceWorkerModuleUrl))
      {
        NotificationRuntime.Current.RuntimeLog.Info(
          "Successfully registered all support libraries for browser notifications."
        );
      }
      else
      {
        NotificationRuntime.Current.RuntimeLog.Error(
          "Failed to register browser notifications service worker. Some notification features will not work!"
        );
      }
    }
    catch (Exception ex)
    {
      NotificationRuntime.Current.RuntimeLog.Error(
        "Failed to register service worker, some notification features will not work!",
        ex
      );
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

        var action = pushNotification.Contents?.GetNotificationAction(activatedButtonId);
        var activationArgs = new NotificationActivationArguments(
          new Dictionary<INotificationAction, string?>(),
          action
        );
        action?.OnActivated();
        pushNotification?.OnActivated(activationArgs);
      },
      () =>
      {
        pushNotification.OnDismissed(NotificationDismissalReason.UserCanceled);
      },
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
    return NotificationsInterop.UnregisterAsync();
  }

  public PushNotificationUpdateResult Update(IPushNotification pushNotification) =>
    PushNotificationUpdateResult.Failed;

  private static void AssertOnBrowserNotificationSupport()
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
    string blobUrl = await GetModuleUrl(assembly, resourceName);

    // Register the Blob URL under a friendly module name
    _ = await JSHost.ImportAsync(JSModuleName, blobUrl);
  }

  private static async Task<string> GetModuleUrl(Assembly assembly, string resourceName)
  {
    await InstallBrowserImportShimAsync(assembly);
    string jsCode = await GetJSCode(assembly, resourceName);

    return NotificationsInterop.GetImportUrl(jsCode)
      ?? throw new EntryPointNotFoundException(
        $"Failed to generate import URL for JavaScript, unable to import: {resourceName}"
      );
  }

  private static async Task InstallBrowserImportShimAsync(Assembly assembly)
  {
    if (JSHost.GlobalThis.HasProperty(ImportShimModuleName) && _importShimInstalled)
    {
      // Install it
      return;
    }

    try
    {
      string jsCode = await GetJSCode(assembly, "ImportScriptShim.js");
      var jsObjectUrlCode =
        $"URL.createObjectURL(new Blob([`{jsCode.Replace("`", "\\`").Replace("$", "\\$")}`], {{type: 'text/javascript'}}))";
      var blobUrl =
        NotificationsInterop.EvalToString(jsObjectUrlCode)
        ?? throw UnsupportedPlatformException.FromMessage("Failed to load JS importer shim code.");
      _ = await JSHost.ImportAsync(ImportShimModuleName, blobUrl);
    }
    catch (Exception ex) when (ex.GetType() != typeof(UnsupportedPlatformException))
    {
      NotificationRuntime.Current.RuntimeLog.Warning(
        "Failed to load import script shim, unable to load any notification modules!"
      );
    }
  }

  private static async Task<string> GetJSCode(Assembly assembly, string resourceName)
  {
    var javaScriptLibraryResourceName = assembly
      .GetManifestResourceNames()
      .FirstOrDefault(r => r.EndsWith(resourceName));
    if (string.IsNullOrEmpty(javaScriptLibraryResourceName))
    {
      throw new FileNotFoundException($"Unable to find JS resource: '{resourceName}'.");
    }

    using var stream =
      assembly.GetManifestResourceStream(javaScriptLibraryResourceName)
      ?? throw new EndOfStreamException($"Unable to load JS resource stream: '{resourceName}'.");
    using var reader = new StreamReader(stream, Encoding.UTF8);
    return await reader.ReadToEndAsync();
  }
}
#endif
