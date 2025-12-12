#if WINDOWS_TOAST_NOTIFICATIONS
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.UI.Notifications;

namespace CNCO.Unify.Notifications.Platforms.Windows;

[SupportedOSPlatform("windows")]
[ClassInterface(ClassInterfaceType.None)]
[ComSourceInterfaces(typeof(INotificationActivationCallback))]
[Guid("BB2560CC-4D03-4988-89D5-8715F0DA2FF2"), ComVisible(true)] // Set to your application (assembly)'s GUID
public class NotificationActivator : INotificationActivationCallback
{
  private static readonly Action<string, NotificationEventArgs>? ActivatedFunction;

  /// <summary>
  /// This is called when a notification is activated
  /// </summary>
  public void Activate(
    string appUserModelId,
    string invokedArgs,
    NOTIFICATION_USER_INPUT_DATA[] data,
    uint dataCount
  )
  {
    ActivatedFunction?.Invoke(
      appUserModelId,
      new NotificationEventArgs(invokedArgs)
      {
        Action = NotificationEventArgs.NotificationAction.Activated,
        COMActivated = true,
      }
    );
  }

  /// <summary>
  /// Calls the initialized activator method
  /// </summary>
  public static void Activate(string appUserModelId, NotificationEventArgs args)
  {
    ActivatedFunction?.Invoke(appUserModelId, args);
  }

  /// <summary>
  /// Run this once on application first start
  /// </summary>
  /*public static void Initialize(Action<string, NotificationEventArgs> action) {
      ActivatedFunction = action;

      regService = new RegistrationServices();

      cookie = regService.RegisterTypeForComClients(
          typeof(NotificationActivator),
          RegistrationClassContext.LocalServer,
          RegistrationConnectionType.MultipleUse);
  }
  public static void Uninitialize() {
      if (cookie != -1 && regService != null)
          regService.UnregisterTypeForComClients(cookie);
  }*/

  //private static int cookie = -1;
  //private static RegistrationServices regService = null;
}
#endif
