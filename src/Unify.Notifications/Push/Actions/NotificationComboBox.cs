namespace CNCO.Unify.Notifications.Push.Actions;

/// <summary>
/// A combo box in a notification.
/// </summary>
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


  public NotificationComboBox(string id, string[]? choices = null, string? hint = null) : base(id)
  {
    Choices = choices ?? [];
    Hint = hint ?? string.Empty;
  }
}
