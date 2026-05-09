#if WASM_PUSH_NOTIFICATIONS
using CNCO.Unify.Notifications.Push;
using CNCO.Unify.Notifications.Push.Actions;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace CNCO.Unify.Notifications.Platforms.Browser;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(BrowserPushNotification))]
internal partial class BrowserPushNotificationContext : JsonSerializerContext { }

/// <summary>
/// Used to serialize a <see cref="IPushNotification"/> when sending it
/// over the JS Interop.
/// </summary>
[SupportedOSPlatform("browser")]
internal class BrowserPushNotification
{
  private static JsonSerializerOptions JsonSerializerOptions =>
    new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

  public required string Id { get; set; }
  public required string Group { get; set; }
  public required string Title { get; set; }

  public IEnumerable<BrowserPushNotificationAction> Actions { get; set; } = [];
  public string? Text { get; set; }
  public string? IconUrl { get; set; }
  public string? ImageUrl { get; set; }

  public required bool IsPersistent { get; set; }
  public required bool IsSilent { get; set; }

  /// <summary>
  /// Epoch-based timestamp.
  /// </summary>
  public long? TimeStamp { get; set; }

  public static string FromPushNotification(
    IPushNotification pushNotification,
    out BrowserPushNotification browserPushNotification
  )
  {
    browserPushNotification = new BrowserPushNotification
    {
      Id = pushNotification.Id.ToString(),
      Group = pushNotification.Group,
      Title = pushNotification.Title,
      Text = GetNotificationBody(pushNotification),
      Actions = GetNotificationActions(pushNotification),

      IconUrl = pushNotification.Contents?.GetIconToDisplay()?.Uri?.ToString(),
      ImageUrl = pushNotification.Contents?.GetIconToDisplay()?.Uri?.ToString(),

      IsPersistent =
        IsPersistentCategory(pushNotification) || IsPersistentPriority(pushNotification),
      IsSilent = pushNotification.IsSilent || IsSilentPriority(pushNotification),

      TimeStamp = pushNotification.Timestamp.HasValue
        ? new DateTimeOffset(pushNotification.Timestamp.Value).ToUnixTimeMilliseconds()
        : null,
    };

    return browserPushNotification.ToString();
  }

  public override string ToString()
  {
    var json = new JsonObject
    {
      ["id"] = Id.ToString(),
      ["group"] = Group,
      ["title"] = Title,
      ["text"] = Text,
      ["actions"] = new JsonArray(
        Actions
          .Select(a => new JsonObject()
          {
            ["action"] = a.Action,
            ["title"] = a.Title,
            ["icon"] = a.Icon,
          })
          .ToArray()
      ),
      ["isPersistent"] = IsPersistent,
      ["isSilent"] = IsSilent,
    };

    if (TimeStamp.HasValue)
    {
      json["timeStamp"] = TimeStamp;
    }

    return json.ToJsonString(JsonSerializerOptions);
  }

  private static string? GetNotificationBody(IPushNotification pushNotification)
  {
    // Text defined?
    if (!string.IsNullOrEmpty(pushNotification.Contents?.Text))
    {
      return pushNotification.Contents.Text;
    }

    // Must be a message?
    var lastMessage = pushNotification.Contents?.ConversationData?.Messages?.LastOrDefault(m =>
      !m.IsAuthoredByUs
    );
    if (lastMessage != null && !string.IsNullOrEmpty(lastMessage.Text))
    {
      return $"{lastMessage.Name}: {lastMessage.Text[..Math.Min(lastMessage.Text.Length, WasmNotificationManager.MaxMessageLength)]}";
    }
    return null;
  }

  private static IEnumerable<BrowserPushNotificationAction> GetNotificationActions(
    IPushNotification pushNotification
  ) =>
    pushNotification
      .Contents?.Actions?.OfType<NotificationButton>()
      .Take(WasmNotificationManager.MaxActions)
      .Select(a => new BrowserPushNotificationAction()
      {
        Title = a.Contents,
        Action = a.Id,
        Icon = a.Icon?.Uri?.ToString(),
      }) ?? [];

  private static bool IsPersistentCategory(IPushNotification pushNotification) =>
    pushNotification.Category switch
    {
      NotificationCategory.Alarm => true,
      NotificationCategory.Call => true,
      NotificationCategory.Reminder => true,
      _ => false,
    };

  private static bool IsPersistentPriority(IPushNotification pushNotification) =>
    pushNotification.Priority switch
    {
      Push.NotificationPriority.High => true,
      _ => false,
    };

  private static bool IsSilentPriority(IPushNotification pushNotification) =>
    pushNotification.Priority switch
    {
      Push.NotificationPriority.Low => true,
      _ => false,
    };
}
#endif
