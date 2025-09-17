namespace CNCO.Unify.Notifications.Push.Actions {
    public interface INotificationAction {
        string Id { get; set; }
        NotificationActionType Type { get; }
    }
}