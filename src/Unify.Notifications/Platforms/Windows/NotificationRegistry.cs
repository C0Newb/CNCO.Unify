#if WINDOWS_TOAST_NOTIFICATIONS
using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace CNCO.Unify.Notifications.Platforms.Windows;

/// <summary>
/// Registers the application with the COM server and creates a shortcut in the StartMenu.
/// </summary>
[SupportedOSPlatform("windows")]
internal class NotificationRegistry
{
  private static string ShortcutPath
  {
    get => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).TrimEnd('\\') + @"\Microsoft\Windows\Start Menu\Programs\" + UnifyRuntime.Current.ApplicationId + ".lnk";
  }

  public static void RegisterAppForNotificationSupport(bool force)
  {
    bool shortcutExists = File.Exists(ShortcutPath);
    if (force || !shortcutExists)
    {
      string currentApplicationPath = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule!.FileName;
      InstallShortcut(currentApplicationPath);
    }
  }

  /// <summary>
  /// Registers the COM server for the running program
  /// </summary>
  /// <param name="exePath">The current path of the running program</param>
  private static void RegisterComServer(string exePath)
  {
    // We register the app process itself to start up when the notification is activated, but
    // other options like launching a background process instead that then decides to launch
    // the UI as needed.
    string guid = "{" + typeof(NotificationActivator).GUID + "}";

    using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64))
    {
      using (RegistryKey? subkey = key.OpenSubKey(@"SOFTWARE\Classes\CLSID", true))
      {
        using (RegistryKey? subkeyClsGuid = subkey?.CreateSubKey(guid, true))
        {
          // Below breaks something
          //subkeyClsGuid.SetValue(null, typeof(NotificationActivator).GUID);
          //subkeyClsGuid.SetValue("AppID", APP_ID);
          //subkeyClsGuid.SetValue("DisplayName", "Neptune Notification Handler");

          using (RegistryKey? subkeyLocalServer32 = subkeyClsGuid?.CreateSubKey("LocalServer32"))
          {
            ArgumentNullException.ThrowIfNull(subkeyLocalServer32);
            subkeyLocalServer32.SetValue(null, exePath);
          }
        }
      }
    }
  }

  /// <summary>
  /// Install the applications shortcut into the start menu for notification support
  /// </summary>
  /// <param name="exePath">The current path of the running program</param>
  private static void InstallShortcut(string exePath)
  {
    IShellLinkW newShortcut = (IShellLinkW)new CShellLink();

    // Create a shortcut to the exe
    newShortcut.SetPath(exePath);

    // Open the shortcut property store, set the AppUserModelId property
    IPropertyStore newShortcutProperties = (IPropertyStore)newShortcut;

    PropVariantHelper varAppId = new PropVariantHelper();
    varAppId.SetValue(UnifyRuntime.Current.ApplicationId);
    newShortcutProperties.SetValue(PROPERTYKEY.AppUserModel_ID, varAppId.Propvariant);

    PropVariantHelper varToastId = new PropVariantHelper
    {
      VarType = VarEnum.VT_CLSID
    };
    varToastId.SetValue(typeof(NotificationActivator).GUID);

    newShortcutProperties.SetValue(PROPERTYKEY.AppUserModel_ToastActivatorCLSID, varToastId.Propvariant);

    // Commit the shortcut to disk
    IPersistFile newShortcutSave = (IPersistFile)newShortcut;

    newShortcutSave.Save(ShortcutPath, true);
  }


  public static void UninstallShortcut()
  {
    if (File.Exists(ShortcutPath))
      File.Delete(ShortcutPath);
  }
}
#endif