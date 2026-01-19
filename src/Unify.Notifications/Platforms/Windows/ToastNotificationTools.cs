#if WINDOWS_TOAST_NOTIFICATIONS
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;
using CNCO.Unify.Notifications.Push;
using CNCO.Unify.Notifications.Push.Actions;
using CNCO.Unify.Notifications.Push.Imaging;
using Microsoft.Toolkit.Uwp.Notifications;
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

    if (!string.IsNullOrWhiteSpace(pushNotification.Title))
    {
      _ = builder.AddText(pushNotification.Title, hintMaxLines: 1);
    }

    if (
      !string.IsNullOrWhiteSpace(pushNotification.Contents.Text)
      && pushNotification.Contents.ConversationData == null
      && !HasProgressBar(pushNotification)
    )
    {
      _ = builder.AddText(pushNotification.Contents.Text, AdaptiveTextStyle.Caption);
    }

    // Add message data to the toast
    AddConversationData(pushNotification, builder);

    // Add actions
    AddActions(pushNotification, builder);

    // Progress bar?
    AddProgressBar(pushNotification, builder);

    AddCustomTimeStamp(pushNotification, builder);

    SetNotificationCategory(pushNotification, builder);

    // Add the application (or conversation) icon
    AddApplicationIcon(pushNotification, builder);

    // Add primary image
    AddBodyImage(pushNotification, builder);

    // Attribution text
    AddAttributionText(pushNotification, builder);

    return builder.GetXml();
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

  private static void SetNotificationCategory(
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
