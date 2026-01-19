using CNCO.Unify.Notifications.Push;

namespace CNCO.Unify.Notifications;

public record NotificationRuntimeConfiguration : IRuntimeConfiguration
{
  /// <summary>
  /// Time until a <see cref="IPushNotification"/> is canceled (deleted) after
  /// the progress bar reaches 100%.
  /// </summary>
  /// <remarks>
  /// If below zero or longer than 1 minute then notifications are not canceled.
  /// </remarks>
  public TimeSpan AutoCancelNotificationOnProgressBarCompletionDelay { get; set; } =
    TimeSpan.FromSeconds(5);
}
