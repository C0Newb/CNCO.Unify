using CNCO.Unify.Notifications.Push.Imaging;

namespace CNCO.Unify.Notifications.Push.Actions;

/// <summary>
/// A button on a notification.
/// </summary>
public class NotificationButton : NotificationAction, INotificationAction
{
  /// <summary>
  /// Text shown on the button.
  /// </summary>
  public string Contents { get; set; }

  /// <summary>
  /// The id of the text box this button belong to.
  /// </summary>
  /// <remarks>
  /// This sets the button as the 'submit' button for whatever textbox.
  /// </remarks>
  public NotificationTextBox? TextBox { get; set; }

  /// <summary>
  /// An optional image icon for the button to display.
  /// </summary>
  public INotificationImage? Icon { get; set; }

  /// <summary>
  /// Initializes a new instance of the <see cref="NotificationButton"/> class.
  /// </summary>
  /// <param name="contents">Text in the button.</param>
  /// <param name="textBox">Textbox the button belongs to. This marks the button as the 'submit'/action button for the textbox.</param>
  public NotificationButton(string? contents = null, NotificationTextBox? textBox = null)
  {
    Contents = contents ?? string.Empty;
    TextBox = textBox ?? null;
  }

  public override void OnActivated(string? value = null) =>
    base.OnActivated(value == null && TextBox?.Contents != null ? TextBox.Contents : value);
}
