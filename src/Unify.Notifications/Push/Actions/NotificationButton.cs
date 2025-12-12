namespace CNCO.Unify.Notifications.Push.Actions;

/// <summary>
/// A button on a notification.
/// </summary>
public class NotificationButton : NotificationAction, INotificationAction
{
  public override NotificationActionType Type => NotificationActionType.Button;

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
  /// Initializes a new instance of the <see cref="NotificationButton"/> class.
  /// </summary>
  /// <param name="id">Id of the button.</param>
  /// <param name="contents">Text in the button.</param>
  /// <param name="textBox">Textbox the button belongs to. This marks the button as the 'submit'/action button for the textbox.</param>
  public NotificationButton(string id, string? contents = null, NotificationTextBox? textBox = null) : base(id)
  {
    Contents = contents ?? string.Empty;
    TextBox = textBox ?? null;
  }
}
