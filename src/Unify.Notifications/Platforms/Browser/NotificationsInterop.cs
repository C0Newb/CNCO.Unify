#if WASM_PUSH_NOTIFICATIONS
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace CNCO.Unify.Notifications.Platforms.Browser;

[SupportedOSPlatform("browser")]
internal static partial class NotificationsInterop
{
  public static string? Permission =>
    JSHost.GlobalThis.GetPropertyAsString("Notification.permission");

  #region Shim code
  // lol, lmao even
  [JSImport("globalThis.eval")]
  [return: JSMarshalAs<JSType.Object>]
  public static partial JSObject Eval([JSMarshalAs<JSType.String>] string jsCode);

  [JSImport("globalThis.eval")]
  [return: JSMarshalAs<JSType.String>]
  public static partial string EvalToString([JSMarshalAs<JSType.String>] string jsCode);

  [JSImport("globalThis.URL.createObjectURL")]
  [return: JSMarshalAs<JSType.String>]
  public static partial string CreateObjectUrl([JSMarshalAs<JSType.Object>] JSObject blob);

  [JSImport("default", WasmNotificationManager.ImportShimModuleName)]
  [return: JSMarshalAs<JSType.String>]
  public static partial string GetImportUrl([JSMarshalAs<JSType.String>] string code);
  #endregion

  [JSImport("globalThis.Notification.requestPermission")]
  [return: JSMarshalAs<JSType.Promise<JSType.String>>]
  public static partial Task<string> RequestPermission();

  [JSImport("NotMan.register", WasmNotificationManager.JSModuleName)]
  [return: JSMarshalAs<JSType.Promise<JSType.String>>]
  public static partial Task<string> RegisterAsync();

  [JSImport("NotMan.unregister", WasmNotificationManager.JSModuleName)]
  [return: JSMarshalAs<JSType.Promise<JSType.Boolean>>]
  public static partial Task<bool> UnregisterAsync();

  [JSImport("NotMan.loadServiceWorker", WasmNotificationManager.JSModuleName)]
  [return: JSMarshalAs<JSType.Promise<JSType.Boolean>>]
  public static partial Task<bool> RegisterServiceWorker(
    [JSMarshalAs<JSType.String>] string serviceWorkerUrl
  );

  [JSImport("NotMan.cancel", WasmNotificationManager.JSModuleName)]
  [return: JSMarshalAs<JSType.Boolean>]
  public static partial bool Cancel(
    [JSMarshalAs<JSType.String>] string id,
    [JSMarshalAs<JSType.String>] string group
  );

  [JSImport("NotMan.clearAll", WasmNotificationManager.JSModuleName)]
  public static partial void ClearAll();

  [JSImport("NotMan.getNotificationActivationReason", WasmNotificationManager.JSModuleName)]
  [return: JSMarshalAs<JSType.String>]
  public static partial string GetNotificationActivationReason(
    [JSMarshalAs<JSType.String>] string id,
    [JSMarshalAs<JSType.String>] string group
  );

  [JSImport("NotMan.getMaxNotificationActions", WasmNotificationManager.JSModuleName)]
  [return: JSMarshalAs<JSType.Number>]
  public static partial int GetMaxNotificationActions();

  [JSImport("NotMan.send", WasmNotificationManager.JSModuleName)]
  [return: JSMarshalAs<JSType.Promise<JSType.Boolean>>]
  public static partial Task<bool> Send(
    [JSMarshalAs<JSType.String>] string pushNotificationJson,
    [JSMarshalAs<JSType.Function>] Action onClick,
    [JSMarshalAs<JSType.Function>] Action onClose,
    [JSMarshalAs<JSType.Function>] Action onError
  );
}
#endif
