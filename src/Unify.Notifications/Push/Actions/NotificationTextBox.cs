namespace CNCO.Unify.Notifications.Push.Actions;

/// <summary>
/// A textbox on a notification.
/// </summary>
public class NotificationTextBox : NotificationAction, INotificationAction
{
  public override NotificationActionType Type => NotificationActionType.Textbox;

  /// <summary>
  /// Text inside the textbox.
  /// </summary>
  public string Contents { get; set; } = string.Empty;

  /// <summary>
  /// Hint displayed when no text is entered.
  /// </summary>
  public string? Hint { get; set; }

  /// <summary>
  /// Textbox title.
  /// </summary>
  public string? Title { get; set; }

  public NotificationTextBox(
    string id,
    string? contents = null,
    string? title = null,
    string? hint = null
  )
    : base(id)
  {
    Contents = contents ?? string.Empty;
    Title = title;
    Hint = hint;
  }
}
