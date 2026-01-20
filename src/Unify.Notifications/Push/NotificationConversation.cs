using CNCO.Unify.Notifications.Push.Imaging;

namespace CNCO.Unify.Notifications.Push;

internal class NotificationConversation : INotificationConversation
{
  public required IList<NotificationConversationMessage> Messages { get; set; } = [];
  public string? Name { get; set; }
  public INotificationImage? Icon { get; set; }
  public required bool IsGroup { get; set; }

  public void DeleteImages() => Icon?.DeleteImage();
}
