using CNCO.Unify.Notifications.Push.Imaging;

namespace CNCO.Unify.Notifications.Push;

/// <summary>
/// Conversation data (messages) for a notification.
/// </summary>
public interface INotificationConversation
{
  /// <summary>
  /// Messages for the conversation.
  /// </summary>
  public IList<NotificationConversationMessage> Messages { get; set; }

  /// <summary>
  /// Recipient's or group's title/name.
  /// </summary>
  /// <remarks>
  /// Who the conversation is with.
  /// </remarks>
  public string? Name { get; set; }

  /// <summary>
  /// Icon for the conversation, typically the avatar of the other participant/group.
  /// </summary>
  public INotificationImage? Icon { get; set; }

  /// <summary>
  /// Whether the conversation is a group chat (not one-on-one).
  /// </summary>
  /// <remarks>
  /// If the platform supports, <see langword="true"/> sets <see cref="Icon"/> to a square, otherwise a circle.
  ///
  /// Again, depends on platform support.
  /// </remarks>
  public bool IsGroup { get; set; }

  /// <summary>
  /// Deletes the icon and all images within messages.
  /// </summary>
  public void DeleteImages();
}
