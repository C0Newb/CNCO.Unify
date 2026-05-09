using CNCO.Unify.Notifications.Push.Imaging;

namespace CNCO.Unify.Notifications.Push;

/// <summary>
/// Represents a message in a <see cref="INotificationConversation"/>
/// </summary>
public record NotificationConversationMessage
{
  /// <summary>
  /// Sender/author of the message.
  /// </summary>
  public required string Name { get; init; }

  /// <summary>
  /// Icon of the sender.
  /// </summary>
  public INotificationImage? Icon { get; set; }

  /// <summary>
  /// Contents of the message.
  /// </summary>
  public required string Text { get; init; }

  /// <summary>
  /// Optional image attached to the message.
  /// </summary>
  /// <remarks>
  /// The last message's image will be displayed on the notification.
  /// It's important you do not use an image sent by your application here and always
  /// use an image being received.
  /// </remarks>
  public INotificationImage? Image { get; init; }

  /// <summary>
  /// Whether the message was sent by us, the program, or by the user.
  /// </summary>
  /// <remarks>
  /// Images and icons are ignored when they're from us. On WASM, the message is ignore entirely.
  /// </remarks>
  public required bool IsAuthoredByUs { get; init; }
}
