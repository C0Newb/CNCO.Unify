using CNCO.Unify.Notifications.Push.Actions;

namespace CNCO.Unify.Notifications.Push.Eventing {
    /// <summary>
    /// Invoked when a notification action is activated.
    /// </summary>
    /// <param name="action">Action that was activated.</param>
    /// <param name="value">Value associated with the action,
    /// if applicable (such as with <see cref="NotificationTextBox"/>
    /// or <see cref="NotificationComboBox"/>).</param>
    public delegate void NotificationActionActivatedEventHandler(
        INotificationAction action,
        string? value
    );
}
