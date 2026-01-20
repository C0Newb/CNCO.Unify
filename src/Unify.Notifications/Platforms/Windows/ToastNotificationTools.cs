#if WINDOWS_TOAST_NOTIFICATIONS
using CNCO.Unify.Notifications.Push;
using CNCO.Unify.Notifications.Push.Actions;
using CNCO.Unify.Notifications.Push.Imaging;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace CNCO.Unify.Notifications.Platforms.Windows;

[SupportedOSPlatform("windows")]
internal static class ToastNotificationTools
{
  internal static string GetNotificationGroupTagId(IPushNotification pushNotification) =>
    $"{pushNotification.Group}_{GetTag(pushNotification)}";

  internal static string GetActionId(string base64Tag) =>
    GetId(
      Encoding
        .UTF8.GetString(Convert.FromBase64String(base64Tag))
        .Split(INotificationActionExtensions.ActionDelimiter)[1]
    );

  internal static string ToBase64(string? data) =>
    Convert.ToBase64String(Encoding.UTF8.GetBytes(data ?? string.Empty));

  internal static string GetId(string base64Tag) =>
    Encoding.UTF8.GetString(Convert.FromBase64String(base64Tag));

  internal static string GetTag(IPushNotification pushNotification) =>
    ToBase64(pushNotification.Id.ToString());

  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Interoperability",
    "CA1416:Validate platform compatibility",
    Justification = "Supported platform."
  )]
  internal static void SetNotificationPriority(
    IPushNotification pushNotification,
    ToastNotification toast
  )
  {
    switch (pushNotification.Priority)
    {
      case NotificationPriority.High:
        toast.Priority = ToastNotificationPriority.High;
        break;

      case NotificationPriority.Low:
        toast.Priority = ToastNotificationPriority.Default;
        toast.SuppressPopup = true;
        break;

      default:
        toast.Priority = ToastNotificationPriority.Default;
        break;
    }
  }

  internal static XmlDocument GetToastXml(IPushNotification pushNotification)
  {
    ToastContentBuilder builder = new ToastContentBuilder();
    _ = builder.AddArgument("id", GetTag(pushNotification));
    _ = builder.AddArgument("group", pushNotification.Group);

    // SafeWrapToastAddAction is to ensure that if one part fails, the rest can still be added.

    SafeWrapToastAddAction(() => AddTitle(pushNotification, builder));
    SafeWrapToastAddAction(() => AddBodyText(pushNotification, builder));

    // Add message data to the toast
    SafeWrapToastAddAction(() => AddConversationData(pushNotification, builder));

    // Add actions
    SafeWrapToastAddAction(() => AddActions(pushNotification, builder));

    // Progress bar?
    SafeWrapToastAddAction(() => AddProgressBar(pushNotification, builder));

    SafeWrapToastAddAction(() => AddCustomTimeStamp(pushNotification, builder));

    SafeWrapToastAddAction(() => AddNotificationCategory(pushNotification, builder));

    // Add the application (or conversation) icon
    SafeWrapToastAddAction(() => AddApplicationIcon(pushNotification, builder));

    // Add primary image
    SafeWrapToastAddAction(() => AddBodyImage(pushNotification, builder));

    // Attribution text
    SafeWrapToastAddAction(() => AddAttributionText(pushNotification, builder));

    return builder.GetXml();
  }

  public static void SafeWrapToastAddAction(
    Action action,
    [CallerArgumentExpression(nameof(action))] string? expressionText = null
  )
  {
    int start = expressionText?.IndexOf("Add", StringComparison.OrdinalIgnoreCase) ?? -1;
    int end = expressionText?.IndexOf('(', start > -1 ? start : 0) ?? -1;

    if (start != -1 && end != -1 && end > start)
    {
      // Slice out just the portion after "Add" (e.g., "ProgressBar")
      expressionText = expressionText![(start + 3)..end];
    }

    try
    {
      action.Invoke();
    }
    catch (Exception ex)
    {
      NotificationRuntime.Current.RuntimeLog.Error(
        start != -1 && end != -1
          ? $"Failed to add toast notification element: {expressionText}"
          : $"Failed to add toast notification element, unknown which.",
        ex
      );
    }
  }

  private static void AddTitle(IPushNotification pushNotification, ToastContentBuilder builder)
  {
    if (!string.IsNullOrWhiteSpace(pushNotification.Title))
    {
      _ = builder.AddText(pushNotification.Title, hintMaxLines: 1);
    }
  }

  private static void AddBodyText(IPushNotification pushNotification, ToastContentBuilder builder)
  {
    if (
      !string.IsNullOrWhiteSpace(pushNotification.Contents.Text)
      && pushNotification.Contents.ConversationData == null
      && !HasProgressBar(pushNotification)
    )
    {
      _ = builder.AddText(pushNotification.Contents.Text, AdaptiveTextStyle.Caption);
    }
  }

  private static void AddActions(IPushNotification pushNotification, ToastContentBuilder builder)
  {
    if (pushNotification.Contents.Actions == null)
    {
      return;
    }

    foreach (var action in pushNotification.Contents.Actions)
    {
      if (action is NotificationButton actionButton)
      {
        AddButton(builder, actionButton);
      }
      else if (action is NotificationTextBox actionTextBox)
      {
        AddTextBox(builder, actionTextBox);
      }
      else if (
        // Combobox?
        action is NotificationComboBox actionComboBox
        // At least one item
        && actionComboBox.Choices.Length >= 1
      )
      {
        AddComboBox(builder, actionComboBox);
      }
    }
  }

  private static void AddApplicationIcon(
    IPushNotification pushNotification,
    ToastContentBuilder builder
  )
  {
    var icon = pushNotification.Contents.GetIconToDisplay();
    if (icon?.Uri != null)
    {
      _ = builder.AddAppLogoOverride(
        icon.Uri,
        ToastGenericAppLogoCrop.Circle,
        icon.AlternativeText
      );
    }
  }

  private static void AddAttributionText(
    IPushNotification pushNotification,
    ToastContentBuilder builder
  )
  {
    if (!string.IsNullOrEmpty(pushNotification.Contents.AttributionText))
    {
      _ = builder.AddAttributionText(pushNotification.Contents.AttributionText);
    }
  }

  private static void AddBodyImage(IPushNotification pushNotification, ToastContentBuilder builder)
  {
    var image = pushNotification.Contents.GetImageToDisplay();
    if (image?.Uri != null)
    {
      _ = builder.AddInlineImage(image.Uri, image.AlternativeText);
    }
  }

  private static void AddButton(ToastContentBuilder builder, NotificationButton notificationButton)
  {
    var button = new ToastButton(
      notificationButton.Contents,
      $"action={ToBase64(notificationButton.GetWindowsComponentId())}"
    );

    // Icon?
    var icon = notificationButton.Icon?.Uri;
    if (icon != null)
    {
      _ = button.SetImageUri(icon);
    }

    // Linked to a text box?
    if (notificationButton.TextBox != null)
    {
      _ = button.SetTextBoxId(notificationButton.TextBox.GetWindowsComponentId());
    }

    _ = builder.AddButton(button);
  }

  private static void AddComboBox(
    ToastContentBuilder builder,
    NotificationComboBox notificationComboBox
  )
  {
    var comboBox = new ToastSelectionBox(notificationComboBox.GetWindowsComponentId());
    if (!string.IsNullOrEmpty(notificationComboBox.Hint))
    {
      comboBox.Title = notificationComboBox.Hint;
    }

    foreach (
      ToastSelectionBoxItem item in notificationComboBox
        .Choices.Distinct()
        .Select(option => new ToastSelectionBoxItem(option, option))
        .Take(5)
    )
    {
      comboBox.Items.Add(item);
    }

    comboBox.DefaultSelectionBoxItemId = notificationComboBox.SelectedItem;

    _ = builder.AddToastInput(comboBox);
  }

  private static void AddConversationData(
    IPushNotification pushNotification,
    ToastContentBuilder builder
  )
  {
    var conversationData = pushNotification.Contents.ConversationData;
    if (conversationData == null)
    {
      return;
    }

    if (conversationData.Icon?.Uri != null)
    {
      // Add recipient's profile picture (override icon)
      _ = builder.AddAppLogoOverride(
        conversationData.Icon.Uri,
        // Stealing MSTeam's style here: group -> square, 1:1 chat -> circle
        conversationData.IsGroup
          ? ToastGenericAppLogoCrop.Default
          : ToastGenericAppLogoCrop.Circle,
        conversationData.Icon.AlternativeText ?? conversationData.Name
      );
    }

    foreach (
      var conversation in conversationData
        .Messages.Where(m => m != null && !string.IsNullOrEmpty(m.Text))
        .TakeLast(3) // Only 4 pieces of text allowed (including title) - so there's only space for 3 messages
    )
    {
      string message = conversation.Text;
      if (!string.IsNullOrEmpty(conversation.Name))
      {
        // Add person's name?
        message = conversation.Name + ": " + conversation.Text;
      }

      try
      {
        _ = builder.AddText(message);
      }
      catch (Exception)
      {
        // Must've hit the max count?
#if DEBUG
        Debugger.Break();
#endif
        break;
      }
    }
  }

  private static void AddCustomTimeStamp(
    IPushNotification pushNotification,
    ToastContentBuilder builder
  )
  {
    // No timestamp set
    if (pushNotification.Timestamp == null || pushNotification.Timestamp == DateTime.MinValue)
    {
      return;
    }
    _ = builder.AddCustomTimeStamp(pushNotification.Timestamp ?? DateTime.Now);
  }

  private static void AddProgressBar(
    IPushNotification pushNotification,
    ToastContentBuilder builder
  )
  {
    if (!HasProgressBar(pushNotification))
    {
      return;
    }

    BindableProgressBarValue progressBarValue = new BindableProgressBarValue(
      WindowsNotificationManager.ProgressBarValueBindingName
    );

    var progressBar = new AdaptiveProgressBar()
    {
      Value = progressBarValue,
      Status = pushNotification.Contents.ProgressData?.Status ?? string.Empty,
    };

    if (!string.IsNullOrEmpty(pushNotification.Contents.ProgressData?.Title))
    {
      progressBar.Title = pushNotification.Contents.ProgressData?.Title;
    }

    if (!string.IsNullOrEmpty(pushNotification.Contents.ProgressData?.DisplayedValue))
    {
      progressBar.ValueStringOverride = pushNotification.Contents.ProgressData?.DisplayedValue;
    }

    _ = builder.AddVisualChild(progressBar);
  }

  private static void AddTextBox(ToastContentBuilder builder, NotificationTextBox actionTextBox)
  {
    var textBox = new ToastTextBox(actionTextBox.GetWindowsComponentId());
    if (!string.IsNullOrEmpty(actionTextBox.Hint))
    {
      textBox.PlaceholderContent = actionTextBox.Hint;
    }
    if (!string.IsNullOrEmpty(actionTextBox.Contents))
    {
      textBox.DefaultInput = actionTextBox.Contents;
    }
    if (!string.IsNullOrEmpty(actionTextBox.Title))
    {
      textBox.Title = actionTextBox.Title;
    }
    _ = builder.AddToastInput(textBox);
  }

  private static bool HasProgressBar(IPushNotification pushNotification) =>
    pushNotification.Contents.ProgressData != null
    || pushNotification.Contents.ProgressData?.GetPercentage() < 1;

  private static void AddNotificationCategory(
    IPushNotification pushNotification,
    ToastContentBuilder builder
  )
  {
    var scenario = pushNotification.Category switch
    {
      NotificationCategory.Call => ToastScenario.IncomingCall,
      NotificationCategory.Alarm => ToastScenario.Alarm,
      NotificationCategory.Reminder => ToastScenario.Reminder,
      _ => ToastScenario.Default,
    };
    _ = builder.SetToastScenario(scenario);
  }
}
#endif
