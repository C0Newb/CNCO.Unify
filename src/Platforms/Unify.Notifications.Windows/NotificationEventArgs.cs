using System.Runtime.Versioning;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;

namespace CNCO.Unify.Notifications.Windows {
    /// <summary>
    /// Notification activation arguments
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class NotificationEventArgs {
        // This is inside the activated args
        public ValueSet? UserInput;

        public NotificationAction Action = NotificationAction.Unknown;
        public ToastDismissalReason DismissalReason;
        public string? ErrorDetails;

        public bool COMActivated = false;

        public string? Tag;
        public string? Group;
        public string? LaunchArguments;


        public NotificationEventArgs() { }

        public NotificationEventArgs(ToastNotification sender, string arguments) {
            GetId(arguments.ToString());
            Tag = sender.Tag;
            Group = sender.Group;

            Action = NotificationAction.Activated;
            LaunchArguments = arguments;
        }
        public NotificationEventArgs(ToastNotification sender, ToastActivatedEventArgs arguments) {
            GetId(arguments.Arguments?.ToString());
            Tag = sender.Tag;
            Group = sender.Group;

            Action = NotificationAction.Activated;
            UserInput = arguments.UserInput;
        }

        public NotificationEventArgs(ToastNotification sender, ToastDismissedEventArgs args) {
            if (sender.Content.ChildNodes.Count <= 1) {
                Tag = sender.Tag;
                Action = NotificationAction.Dismissed;
                DismissalReason = args.Reason;
                return;
            }
            string arguments = sender.Content.ChildNodes[1].Attributes.GetNamedItem("launch").InnerText;
            GetId(arguments);
            Tag = sender.Tag;
            Group = sender.Group;

            Action = NotificationAction.Dismissed;
            DismissalReason = args.Reason;
        }

        public NotificationEventArgs(ToastNotification sender, string reason = "", string moreDetails = "") {
            string arguments = sender?.Content?.GetElementsByTagName("toast")[0]?.Attributes?.GetNamedItem("launch")?.ToString() ?? "";
            GetId(arguments);
            Tag = sender?.Tag ?? string.Empty;
            Group = sender?.Group ?? string.Empty;

            if (reason != "") {
                ErrorDetails = reason;
                Console.Error.WriteLine("Notifications are disabled (" + reason + ").");
                if (moreDetails != "") {
                    Console.Error.WriteLine(moreDetails);
                    ErrorDetails += Environment.NewLine + moreDetails;
                }
            }
            Action = NotificationAction.Error;
        }

        public NotificationEventArgs(string arguments) {
            GetId(arguments);
        }


        private void GetId(string? arguments) {
            try {
                string[] split = arguments?.Split(':') ?? [];
                if (split.Length >= 2) {
                    LaunchArguments = split[1];
                    Tag = split[0];
                } else {
                    LaunchArguments = arguments;
                    Tag = "";
                }
            } catch (FormatException) {
                Tag = "";
            } catch (Exception) { }
        }


        /// <summary>
        /// How the notification was activated
        /// </summary>
        public enum NotificationAction {
            Unknown = 0,
            Activated = 1,
            Dismissed = 2,
            Error = 3,
        }
    }
}
