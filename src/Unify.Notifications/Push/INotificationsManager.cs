namespace CNCO.Unify.Notifications.Push;

/// <summary>
/// Manages push notifications on the underlying operating system.
/// </summary>
/// <remarks>
/// Send, cancel, read push notifications.
/// </remarks>
public interface INotificationsManager
{
  /// <summary>
  /// Pushes the notification out to the user via a <see cref="IPlatformPushNotificationManager"/>
  /// </summary>
  /// <param name="pushNotification">Notification to send.</param>
  /// <remarks>
  /// Whether the notification was sent.
  /// </remarks>
  public Task<bool> SendAsync(IPushNotification pushNotification);

  /// <summary>
  /// Updates a notification that was already pushed to the user.
  /// </summary>
  /// <param name="pushNotification">Notification to update.</param>
  /// <returns>
  /// Whether the notification updated successfully or not.
  /// </returns>
  public PushNotificationUpdateResult Update(IPushNotification pushNotification);

  /// <summary>
  /// Cancels, or dismisses, a push notification.
  /// </summary>
  /// <param name="pushNotification">Push notification to cancel.</param>
  /// <returns>
  /// Whether the notification was removed or not.
  /// </returns>
  public bool Cancel(IPushNotification pushNotification);

  /// <summary>
  /// Cancels, or dismisses, all of this application's push notifications.
  /// </summary>
  public void ClearAll();
}
