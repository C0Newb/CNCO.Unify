using System.Runtime.Versioning;
using CNCO.Unify.Notifications.Push.Actions;
using CNCO.Unify.Notifications.Push.Imaging;

namespace CNCO.Unify.Notifications.Push;

public class NotificationContents
{
  /// <summary>
  /// Main notification text.
  /// </summary>
  public string? Text { get; set; }

  /// <summary>
  /// Notification subtext.
  /// </summary>
  /// <remarks>
  /// Use this for where the content is from, such as a website or SMS.
  /// <para>
  /// On WASM, this is not supported.
  /// The browsers will set it to the address the program is served from.
  /// </para>
  /// </remarks>
  [UnsupportedOSPlatform("browser")]
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
  /// Conversation data, such as messages from a individual or group chat.
  /// </summary>
  /// <remarks>
  /// <para>
  ///   Takes priority over <see cref="Text"/>, unless running via WASM.
  /// </para>
  /// <para>
  ///   If running via WASM, the last message is displayed.
  ///   If conformality is your goal, it's recommended you use the <see cref="Text"/> property
  ///   to ensure the content is displayed to your liking.
  /// </para>
  /// </remarks>
  public INotificationConversation? ConversationData { get; set; }

  /// <summary>
  /// Progress bar properties if the notification type is <see cref="NotificationCategory.Progress"/>.
  /// </summary>
  /// <remarks>
  /// On WASM, this is represented as text only.
  /// </remarks>
  public NotificationProgressData? ProgressData { get; set; }

  /// <summary>
  /// Gets the icon to display for the notification.
  /// </summary>
  /// <remarks>
  /// Use this to automatically obtain the icon with the highest priority.
  /// </remarks>
  /// <returns>
  /// Returns the conversation data's icon
  /// (<see cref="INotificationConversation.Icon"/>) or, if that is null, <see cref="Icon"/>.
  /// </returns>
  public INotificationImage? GetIconToDisplay() =>
    ConversationData?.Icon?.Uri != null ? ConversationData?.Icon : Icon;

  /// <summary>
  /// Gets the image to display for the notification.
  /// </summary>
  /// <returns>
  /// Returns the image from the last Conversation message
  /// </returns>
  public INotificationImage? GetImageToDisplay()
  {
    var lastConversationMessageImage = ConversationData?.Messages?.LastOrDefault(m =>
      m.Image?.Uri != null
    );
    return lastConversationMessageImage != null ? lastConversationMessageImage.Image : Image;
  }

  /// <summary>
  /// Retrieves the notification action in <see cref="Actions"/> that has the id <paramref name="id"/>.
  /// </summary>
  /// <param name="id">Id of the notification action to get.</param>
  /// <returns>Notification action, if found.</returns>
  public INotificationAction? GetNotificationAction(string id) =>
    Actions.FirstOrDefault(action => action.Id == id);

  /// <summary>
  /// Deletes all the images for this notification content from the storage backing.
  /// </summary>
  internal void DeleteImages()
  {
    Icon?.DeleteImage();
    Image?.DeleteImage();

    ConversationData?.DeleteImages();

    foreach (var button in Actions.OfType<NotificationButton>())
    {
      button.Icon?.DeleteImage();
    }
  }
}
