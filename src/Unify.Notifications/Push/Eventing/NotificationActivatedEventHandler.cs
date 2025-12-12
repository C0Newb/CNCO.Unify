namespace CNCO.Unify.Notifications.Push.Eventing;

/// <summary>
/// Handles the activation of a push notification when a user interacts with it.
/// </summary>
/// <param name="pushNotification">Push notification that was activated.</param>
/// <param name="activationArguments">Arguments associated with the activation event.</param>
public delegate void NotificationActivatedEventHandler(
  IPushNotification pushNotification,
  NotificationActivationArguments activationArguments
);
