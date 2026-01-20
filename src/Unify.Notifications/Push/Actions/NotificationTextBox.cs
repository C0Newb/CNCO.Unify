using System.Runtime.Versioning;

namespace CNCO.Unify.Notifications.Push.Actions;

/// <summary>
/// A textbox on a notification.
/// </summary>
[UnsupportedOSPlatform("browser")]
public class NotificationTextBox : NotificationAction, INotificationAction
{
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

  /// <summary>
  /// Initializes a new instance of the <see cref="NotificationTextBox"/> class.
  /// </summary>
  /// <param name="contents">Text inside the textbox.</param>
  /// <param name="title">Name of the textbox.</param>
  /// <param name="hint">Hint displayed when no text is entered.</param>
  public NotificationTextBox(string? contents = null, string? title = null, string? hint = null)
  {
    Contents = contents ?? string.Empty;
    Title = title;
    Hint = hint;
  }
}
