namespace CNCO.Unify.Notifications.Push;

/// <summary>
/// Conversation data (messages) for a notification.
/// </summary>
public interface INotificationConversation
{
  /// <summary>
  /// Messages for the conversation.
  /// </summary>
  public NotificationConversationMessage[] Messages { get; set; }
}
