namespace CNCO.Unify.Notifications.Push;

/// <summary>
/// Manages push notifications on the underlying operating system.
/// </summary>
/// <remarks>
/// Send, cancel, read push notifications.
/// </remarks>
public interface INotificationManager : IPlatformNotificationManager
{
  /// <summary>
  /// Pushes the notification out to the user via a <see cref="IPlatformNotificationManager"/>
  /// </summary>
  /// <param name="pushNotification">Notification to send.</param>
  public void Send(IPushNotification pushNotification);

  /// <summary>
  /// Updates a notification that was already pushed to the user.
  /// </summary>
  /// <param name="pushNotification">Notification to update.</param>
  public void Update(IPushNotification pushNotification);

  /// <summary>
  /// Cancels, or dismisses, a push notification.
  /// </summary>
  /// <param name="pushNotification">Push notification to cancel.</param>
  public void Cancel(IPushNotification pushNotification);

  /// <summary>
  /// Cancels, or dismisses, all of this application's push notifications.
  /// </summary>
  public void ClearAll();
}
