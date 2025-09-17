using System.Runtime.Versioning;

using Microsoft.Toolkit.Uwp.Notifications;

namespace CNCO.Unify.Notifications.Windows {
    /// <summary>
    /// Windows push (toast) notifications.
    /// </summary>
    /// <remarks>
    /// It is very important to understand that push (toast) notifications on Windows
    /// requires a StartMenu shortcut to be created for the application and to register
    /// the shortcut with the COM server.
    /// </remarks>
    [SupportedOSPlatform("windows")]
    public class WindowsNotificationManager {
        public PlatformID PlatformId => PlatformID.Win32NT;

        public ToastNotifierCompat? ToastNotifier { get; private set; }

        private static void ToastNotificationManagerCompat_OnActivated(ToastNotificationActivatedEventArgsCompat e) {
            // Why is this being called when the app is open!!??
            var args = ToastArguments.Parse(e.Argument);
            var data = new Dictionary<string, string?>(args.Count);
            foreach (KeyValuePair<string, string> valuePair in args) {
                data[valuePair.Key] = valuePair.Value;
            }

            if (args.Contains("action") && args["action"].Split('=').Length == 2) {
                string buttonId = args["action"].Split('=')[1];
                data.Add("buttonId", buttonId);
            }
#pragma warning disable CA1416 // Validate platform compatibility
            if (e.UserInput.ContainsKey("textInput"))
                data.Add("textboxText", e.UserInput["textInput"] as string);
            if (e.UserInput.ContainsKey("combobox"))
                data.Add("comboBoxSelectedItem", e.UserInput["combobox"] as string);
#pragma warning restore CA1416 // Validate platform compatibility

            WindowsNotificationsRuntime.Current.RuntimeLog.Info("Toast activated. Args: " + e.Argument);
        }

        public void Register() {
            try {
                NotificationRegistry.RegisterAppForNotificationSupport(true); // Setup notification support
                                                                              //Notifications.NotificationActivator.Initialize(ToastActivated); // Initialize

                ToastNotificationManagerCompat.OnActivated += ToastNotificationManagerCompat_OnActivated;
                //Notifications.NotificationActivator.Initialize(ToastActivated);
                ToastNotifier = ToastNotificationManagerCompat.CreateToastNotifier();
            } catch (Exception e) {
                try {
                    WindowsNotificationsRuntime.Current.RuntimeLog.Error("Failed to register ToastNotifier into Windows (1)", e);
                    ToastNotificationManagerCompat.Uninstall();
                    NotificationRegistry.UninstallShortcut();

                    NotificationRegistry.RegisterAppForNotificationSupport(true);
                    ToastNotifier = ToastNotificationManagerCompat.CreateToastNotifier();
                } catch (Exception e2) {
                    WindowsNotificationsRuntime.Current.RuntimeLog.Error($"{GetType().Name}::{nameof(Register)}", "Unable to register the ToastNotifier into Windows (2, forced)", e2);
                }
            }
        }

        public void Unregister() {
            try {
                ToastNotificationManagerCompat.Uninstall();
                NotificationRegistry.UninstallShortcut();
            } catch (Exception e) {
                WindowsNotificationsRuntime.Current.RuntimeLog.Error(
                     $"{GetType().Name}::{nameof(Unregister)}",
                    "Unable to unregister the ToastNotifier on Windows. This is mostly fine, but there may be a lingering Start Menu shortcut.",
                    e
                );
            }
        }
    }
}
