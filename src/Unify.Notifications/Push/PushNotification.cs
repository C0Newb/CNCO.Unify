namespace CNCO.Unify.Notifications.Push {
    /// <summary>
    /// Push notification.
    /// </summary>
    public class PushNotification : IPushNotification {
        private INotificationManager NotificationManager => NotificationRuntime.NotificationManager;

        private Guid _id = Guid.NewGuid();

        public Guid Id => _id;
        public string Title { get; set; }
        public List<string> Attributes { get; set; } = [];
        public NotificationContents Contents { get; set; }
        public DateTime? Timestamp { get; set; } = DateTime.Now;
        public string Group { get; set; } = "default";
        public NotificationPriority Priority { get; set; }
        public bool IsSilent => Priority == NotificationPriority.Low || Priority == NotificationPriority.Minimum;

        public NotificationCategory Category { get; set; } = NotificationCategory.Standard;

        public PushNotification(string title, NotificationContents? contents = null) {
            Title = title;
            Contents = contents ?? new NotificationContents();
        }

        public void Send() => NotificationManager.Send(this);

        // clean up images?
        public void Cancel() => NotificationManager.Cancel(this);
    }
}
