using CNCO.Unify.Notifications.Push.Eventing;

namespace CNCO.Unify.Notifications.Push.Actions;

public interface INotificationAction
{
  /// <summary>
  /// Identifier of the notification action.
  /// </summary>
  /// <remarks>
  /// Used by uderlying notification system to track events.
  /// </remarks>
  public string Id { get; set; }

  /// <summary>
  /// Gets the type of action this represents.
  /// </summary>
  public NotificationActionType Type { get; }

  /// <summary>
  /// Fired when the action is activated by the user.
  /// </summary>
  /// <remarks>
  /// Will not fire if the action accepts user input and no user input was provided,
  /// except for text boxes if the text box button was clicked.
  /// 
  /// If the action represents accepts user input (such as text or selection),
  /// the input value is provided as the event argument.
  /// </remarks>
  public event NotificationActionActivatedEventHandler? ActionActivated;

  /// <summary>
  /// When the action is activated.
  /// </summary>
  /// <param name="value">Value of the action, if applicable.</param>
  public void OnActivated(string? value = null);
}