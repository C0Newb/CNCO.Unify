using CNCO.Unify.Notifications.Push.Eventing;

namespace CNCO.Unify.Notifications.Push {
    /// <summary>
    /// Push notification.
    /// </summary>
    public interface IPushNotification {
        /// <summary>
        /// Unique identifier for the notification
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Which notification group this belongs to.
        /// </summary>
        /// <remarks>
        /// This is similar to <see cref="Id"/>, but is used to group similar notifications together, such as a category.
        /// </remarks>
        public string Group { get; set; }

        /// <summary>
        /// Notification title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Attributes and miscellaneous data to transmit with the notification.
        /// This is not viewable by the user.
        /// </summary>
        public List<string> Attributes { get; set; }

        /// <summary>
        /// Contents of the push notification.
        /// </summary>
        public NotificationContents Contents { get; set; }

        /// <summary>
        /// Displayed timestamp.
        /// </summary>
        public DateTime? Timestamp { get; set; }

        /// <summary>
        /// Priority of the notification.
        /// </summary>
        public NotificationPriority Priority { get; set; }

        /// <summary>
        /// Whether the notification is displayed silently or not.
        /// </summary>
        /// <remarks>
        /// <see langword="true"/> if <see cref="Priority"/> is <see cref="NotificationPriority.Minimum"/> or <see cref="NotificationPriority.Low"/>.
        /// </remarks>
        public bool IsSilent { get; }

        /// <summary>
        /// Category/type of notification.
        /// </summary>
        public NotificationCategory Category { get; set; }

        /// <summary>
        /// Pushes the notification to the user via the operating system.
        /// </summary>
        public void Send();

        /// <summary>
        /// Delete the notification from the operating system.
        /// </summary>
        public void Cancel();

        #region Eventing
        /// <summary>
        /// Event fired when the notification is activated by the user.
        /// </summary>
        /// <remarks>
        /// This is fired after all <see cref="NotificationActionActivatedEventHandler"/> events
        /// are fired, making this the last event in the "this notification was activated" chain.
        /// </remarks>
        public event NotificationActivatedEventHandler? NotificationActivated;

        /// <summary>
        /// When the notification fails to be sent or displayed.
        /// </summary>
        public event NotificationFailedEventHandler? NotificationFailed;

        /// <summary>
        /// When the notification is dismissed and no longer visible to the user.
        /// </summary>
        /// <remarks>
        /// Includes dismassals by this application. Be sure to check the <see cref="NotificationDismissalReason"/>.
        /// </remarks>
        public event NotificationDismissedEventHandler? NotificationDismissed;

        void OnActivated(NotificationActivationArguments args);
        void OnFailed(NotificationFailureReason reason, string? details);
        void OnDimsissed(NotificationDismissalReason reason);
        #endregion
    }
}
