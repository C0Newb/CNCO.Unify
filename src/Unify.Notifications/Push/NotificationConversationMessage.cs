using CNCO.Unify.Notifications.Push.Imaging;

namespace CNCO.Unify.Notifications.Push {
    /// <summary>
    /// Represents a message in a <see cref="INotificationConversation"/>
    /// </summary>
    public class NotificationConversationMessage {
        /// <summary>
        /// Sender/author of the message.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Icon of the sender.
        /// </summary>
        public INotificationImage? Icon { get; set; }

        /// <summary>
        /// Contents of the message.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Optional image attached to the message.
        /// </summary>
        public INotificationImage? Image { get; set; }


        public NotificationConversationMessage(string name, string text, INotificationImage? icon = null, INotificationImage? image = null) {
            Name = name;
            Icon = icon;
            Text = text;
            Image = image;
        }
    }
}
