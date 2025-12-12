using CNCO.Unify.Notifications.Push.Actions;

namespace CNCO.Unify.Notifications.Push.Eventing;

/// <summary>
/// Notification activation data.
/// </summary>
public class NotificationActivationArguments
{
  /// <summary>
  /// Action that was invoked by the user.
  /// </summary>
  /// <remarks>
  /// If <see langword="null"/>, the notification body itself was clicked/tapped.
  /// </remarks>
  public INotificationAction? Action { get; private set; }

  /// <summary>
  /// User input associated with the activation event.
  /// </summary>
  public IDictionary<INotificationAction, string?> UserInput { get; private set; }

  public NotificationActivationArguments(
      IDictionary<INotificationAction, string?>? userInput = null,
      INotificationAction? action = null
  )
  {
    Action = action;
    UserInput = userInput ?? new Dictionary<INotificationAction, string?>();
  }
}