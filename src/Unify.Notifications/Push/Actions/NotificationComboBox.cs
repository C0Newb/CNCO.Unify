using System.Runtime.Versioning;

namespace CNCO.Unify.Notifications.Push.Actions;

/// <summary>
/// A combo box in a notification.
/// </summary>
[UnsupportedOSPlatform("browser")]
public class NotificationComboBox : NotificationAction, INotificationAction
{
  public override NotificationActionType Type => NotificationActionType.ComboBox;

  /// <summary>
  /// Options shown in the combo box.
  /// </summary>
  public string[] Choices { get; set; } = [];

  /// <summary>
  /// Hint displayed when no option is selected.
  /// </summary>
  public string Hint { get; set; } = string.Empty;

  /// <summary>
  /// Which item is selected.
  /// </summary>
  public int SelectedIndex { get; set; } = -1;

  /// <summary>
  /// Which item is selected, if any.
  /// </summary>
  public string? SelectedItem
  {
    get
    {
      if (SelectedIndex >= 0 && SelectedIndex < Choices.Length)
      {
        return Choices[SelectedIndex];
      }
      return null;
    }
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="NotificationComboBox"/> class.
  /// </summary>
  /// <param name="choices">Options to show in the combo box.</param>
  /// <param name="hint">Hint displayed when no option is selected.</param>
  public NotificationComboBox(string[]? choices = null, string? hint = null)
  {
    Choices = choices ?? [];
    Hint = hint ?? string.Empty;
  }
}
