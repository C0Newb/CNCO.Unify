namespace CNCO.Unify.Notifications.Push.Eventing {
    public delegate void NotificationFailedEventHandler(
        IPushNotification pushNotification,
        NotificationFailureReason failureReason,
        string? details
    );
}
