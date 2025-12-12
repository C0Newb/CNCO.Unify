namespace CNCO.Unify.Notifications.Push.Eventing;

public delegate void NotificationDismissedEventHandler(
  IPushNotification pushNotification,
  NotificationDismissalReason dismissalReason
);
