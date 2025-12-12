using CNCO.Unify.Notifications.Push.Actions;
using CNCO.Unify.Notifications.Push.Imaging;

namespace CNCO.Unify.Notifications.Push;

public class NotificationContents
{
  /// <summary>
  /// Main notification text.
  /// </summary>
  public string Text { get; set; } = string.Empty;

  /// <summary>
  /// Notification subtext.
  /// </summary>
  /// <remarks>
  /// Use this for where the content is from, such as a website or SMS.
  /// </remarks>
  public string? AttributionText { get; set; }

  /// <summary>
  /// Notification icon.
  /// </summary>
  public INotificationImage? Icon { get; set; }

  /// <summary>
  /// Image displayed in the notification.
  /// </summary>
  public INotificationImage? Image { get; set; }

  /// <summary>
  /// Actions (buttons, a textbox, etc) that can be interacted with on the notification.
  /// </summary>
  public IEnumerable<INotificationAction> Actions { get; set; } = [];

  /// <summary>
  /// Conversation data if the notification type is <see cref="NotificationCategory.Conversation"/>.
  /// </summary>
  public INotificationConversation? ConversationData { get; set; }

  /// <summary>
  /// Progress bar properties if the notification type is <see cref="NotificationCategory.Progress"/>.
  /// </summary>
  public NotificationProgressData? ProgressData { get; set; }

  public INotificationAction? GetNotificationAction(string id)
      => Actions.First(action => action.Id == id);

  public void CleanUpImages()
  {
    Icon?.DeleteImage();
    Image?.DeleteImage();

    foreach (var message in ConversationData?.Messages ?? [])
    {
      message.Image?.DeleteImage();
      message.Icon?.DeleteImage();
    }
  }
}
