namespace CNCO.Unify.Notifications.Platforms.Browser;

internal record BrowserPushNotificationAction
{
  public required string Action { get; set; }
  public required string Title { get; set; }
  public string? Icon { get; set; }
}
