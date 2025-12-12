#if WINDOWS_TOAST_NOTIFICATIONS
using CNCO.Unify.Notifications.Push;
using CNCO.Unify.Notifications.Push.Actions;
using CNCO.Unify.Notifications.Push.Eventing;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Collections.Concurrent;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace CNCO.Unify.Notifications.Platforms.Windows;

/// <summary>
/// Windows push (toast) notifications.
/// </summary>
/// <remarks>
/// It is very important to understand that push (toast) notifications on Windows
/// requires a StartMenu shortcut to be created for the application and to register
/// the shortcut with the COM server.
/// </remarks>
[SupportedOSPlatform("windows")]
public partial class WindowsNotificationManager : INotificationManager
{
#pragma warning disable CA1416 // Validate platform compatibility
  public PlatformID PlatformId => PlatformID.Win32NT;

  private static ToastNotifierCompat? _toastNotifier;
  private static readonly ConcurrentDictionary<string, IPushNotification> _activeNotifications =
    new();

  [GeneratedRegex("(?:action=)(?<id>[^&]+)")]
  private static partial Regex ActivatedNotificationArgumentsButtonIdRegex();

  [GeneratedRegex(@"\b(reply|send|respond)\b", RegexOptions.IgnoreCase, "en-US")]
  private static partial Regex ReplyWordRegex();

  private static void ToastNotificationManagerCompat_OnActivated(
    ToastNotificationActivatedEventArgsCompat e
  )
  {
    // Why is this being called when the app is open!!??
    var args = ToastArguments.Parse(e.Argument);
    var data = new Dictionary<string, string?>(args.Count);
    foreach (KeyValuePair<string, string> valuePair in args)
    {
      data[valuePair.Key] = valuePair.Value;
    }

    if (args.Contains("action") && args["action"].Split('=').Length == 2)
    {
      string buttonId = args["action"].Split('=')[1];
      data.Add("buttonId", buttonId);
    }

    if (e.UserInput.ContainsKey("textInput"))
      data.Add("textboxText", e.UserInput["textInput"] as string);
    if (e.UserInput.ContainsKey("combobox"))
      data.Add("comboBoxSelectedItem", e.UserInput["combobox"] as string);

    NotificationRuntime.Current.RuntimeLog.Info("Toast activated. Args: " + e.Argument);
  }

  public void Register()
  {
    try
    {
      NotificationRegistry.RegisterAppForNotificationSupport(true); // Setup notification support
      //Notifications.NotificationActivator.Initialize(ToastActivated); // Initialize

      ToastNotificationManagerCompat.OnActivated += ToastNotificationManagerCompat_OnActivated;
      //Notifications.NotificationActivator.Initialize(ToastActivated);
      _toastNotifier = ToastNotificationManagerCompat.CreateToastNotifier();
    }
    catch (Exception e)
    {
      try
      {
        NotificationRuntime.Current.RuntimeLog.Error(
          "Failed to register ToastNotifier into Windows (1)",
          e
        );
        ToastNotificationManagerCompat.Uninstall();
        NotificationRegistry.UninstallShortcut();

        NotificationRegistry.RegisterAppForNotificationSupport(true);
        _toastNotifier = ToastNotificationManagerCompat.CreateToastNotifier();
      }
      catch (Exception e2)
      {
        NotificationRuntime.Current.RuntimeLog.Error(
          $"{GetType().Name}::{nameof(Register)}",
          "Unable to register the ToastNotifier into Windows (2, forced)",
          e2
        );
      }
    }
  }

  public void Unregister()
  {
    try
    {
      ToastNotificationManagerCompat.Uninstall();
      NotificationRegistry.UninstallShortcut();
    }
    catch (Exception e)
    {
      NotificationRuntime.Current.RuntimeLog.Error(
        $"{GetType().Name}::{nameof(Unregister)}",
        "Unable to unregister the ToastNotifier on Windows. This is mostly fine, but there may be a lingering Start Menu shortcut.",
        e
      );
    }
  }

  public void Send(IPushNotification pushNotification)
  {
    _toastNotifier ??= ToastNotificationManagerCompat.CreateToastNotifier();
    ToastNotification toastNotification = GetToastNotification(pushNotification);
    _toastNotifier.Show(toastNotification);
    _activeNotifications.TryAdd(
      $"{pushNotification.Group}_{GetTag(pushNotification)}",
      pushNotification
    );
  }

  public void Update(IPushNotification pushNotification)
  {
    try
    {
      _toastNotifier ??= ToastNotificationManagerCompat.CreateToastNotifier();

      string tag = GetTag(pushNotification);
      string group = pushNotification.Group;

      if (pushNotification.Contents.ProgressData != null)
      {
        if (pushNotification.Contents.ProgressData.GetPercentage() == 1)
        {
          // Done!
          ToastNotificationManagerCompat.History.Remove(tag, group);
        }
        else
        {
          var data = new NotificationData { SequenceNumber = 1 };
          //_updateIncrementor++;

          data.Values["progressValue"] = pushNotification
            .Contents.ProgressData?.GetPercentage()
            .ToString();
          _toastNotifier.Update(data, tag, group);
        }
      }
      else
      {
        _toastNotifier.Update(GetToastNotification(pushNotification).Data, tag, group); // for whatever reason Id must stay Id!
      }
    }
    catch (Exception e)
    {
      NotificationRuntime.Current.RuntimeLog.Error("Failed to update notification!", e);
      //return NotificationUpdateResult.Failed;
    }
  }

  public void Cancel(IPushNotification pushNotification)
  {
    ToastNotificationManagerCompat.History.Remove(
      GetTag(pushNotification.Id),
      pushNotification.Group ?? string.Empty
    );
    pushNotification.Contents.CleanUpImages();
  }

  public void ClearAll()
  {
    ToastNotificationManagerCompat.History.Clear();
    UnifyRuntime.FileStorage.Delete(NotificationRuntime.ImageFileStore.Directory);
  }

  #region Helpers
  private static ToastNotification GetToastNotification(IPushNotification pushNotification)
  {
    var toastXml = GetToastXml(pushNotification);
    ToastNotification toast = new ToastNotification(toastXml);

    toast.Activated += Notification_Activated;
    toast.Dismissed += Notification_Dismissed;
    toast.Failed += Notification_Failed;

    toast.Tag = GetTag(pushNotification);
    toast.Group = string.IsNullOrEmpty(pushNotification.Group) ? "default" : pushNotification.Group;
    toast.SuppressPopup = pushNotification.IsSilent;

    switch (pushNotification.Priority)
    {
      case NotificationPriority.Max:
      case NotificationPriority.High:
        toast.Priority = ToastNotificationPriority.High;
        break;

      case NotificationPriority.Low:
      case NotificationPriority.Minimum:
        toast.Priority = ToastNotificationPriority.Default;
        toast.SuppressPopup = true;
        break;

      default:
        toast.Priority = ToastNotificationPriority.Default;
        break;
    }

    if (pushNotification.Contents.ProgressData != null)
    {
      toast.Data = new NotificationData(
        new KeyValuePair<string, string>[]
        {
          new KeyValuePair<string, string>(
            "progressValue",
            pushNotification.Contents.ProgressData.GetPercentage().ToString()
          ),
        }
      );
    }

    toast.ExpiresOnReboot = true;

    return toast;
  }

  internal static string GetTag(Guid id) =>
    Convert.ToBase64String(Encoding.UTF8.GetBytes(id.ToString()));

  internal static string GetTag(IPushNotification pushNotification) => GetTag(pushNotification.Id);

  internal static string GetId(string base64Tag) =>
    Encoding.UTF8.GetString(Convert.FromBase64String(base64Tag));

  internal static string GetActionId(string base64Tag) =>
    GetId(
      Encoding
        .UTF8.GetString(Convert.FromBase64String(base64Tag))
        .Split(INotificationActionExtensions.ActionDelimiter)[1]
    );

  internal static XmlDocument GetToastXml(IPushNotification pushNotification)
  {
    ToastContentBuilder builder = new ToastContentBuilder();
    builder.AddArgument("id", GetTag(pushNotification));
    builder.AddArgument("group", pushNotification.Group);

    /*if (!string.IsNullOrEmpty(ApplicationPackageName) && !string.IsNullOrEmpty(ApplicationName) && ApplicationName != ApplicationPackageName)
        builder.AddHeader(ApplicationPackageName, ApplicationName, ApplicationPackageName);*/
    //builder.AddHeader(pushNotification.Title, pushNotification.Title, "");

    // Set the notification icon
    bool iconSet = false;
    bool addedImage = false; // Add only ONE image!

    if (!string.IsNullOrWhiteSpace(pushNotification.Title))
    {
      builder.AddText(pushNotification.Title, hintMaxLines: 1);
    }

    if (
      !string.IsNullOrWhiteSpace(pushNotification.Contents.Text)
      && pushNotification.Contents.ConversationData == null
      && (
        pushNotification.Contents.ProgressData == null
        || pushNotification.Contents.ProgressData?.GetPercentage() == 1
      )
    )
    {
      builder.AddText(pushNotification.Contents.Text, AdaptiveTextStyle.Caption);
    }

    // Add message data to the toast
    if (pushNotification.Contents.ConversationData != null)
    {
      List<string> messages = new List<string>();
      bool setUserIcon = false; // Set the icon on the FIRST instance of an icon.

      foreach (var conversation in pushNotification.Contents.ConversationData.Messages)
      {
        if (conversation == null || string.IsNullOrEmpty(conversation.Text))
          continue; // No message to add ...

        string message = conversation.Text;

        if (conversation.Name != null)
        { // Add person's name?
          if (!setUserIcon && conversation.Icon != null && conversation.Icon.WriteImage())
          { // Profile picture set?
            // Add profile picture (override icon)
            builder.AddAppLogoOverride(
              conversation.Icon.Uri,
              ToastGenericAppLogoCrop.Circle,
              conversation.Icon.AlternativeText ?? (conversation.Name + "'s profile picture.")
            );
            setUserIcon = iconSet = true;
          }

          message = conversation.Name + ": " + message;
          messages.Add(message);
        }

        if (!addedImage && conversation.Image != null && conversation.Image.WriteImage())
        {
          builder.AddInlineImage(conversation.Image.Uri, conversation.Image.AlternativeText);
          addedImage = true;
        }
      }

      int startIndex = 0;
      if (messages.Count > 3)
        startIndex = messages.Count - 2;
      try
      {
        for (int i = startIndex; i < messages.Count; i++)
        {
          builder.AddText(messages[i]);
        }
      }
      catch (Exception) { }
    }

    // Add actions
    var buttons = new List<ToastButton>();
    ToastTextBox? textBox = null;
    ToastSelectionBox? comboBox = null;
    var comboBoxItems = new List<ToastSelectionBoxItem>();

    static string ToBase64(string? data) =>
      Convert.ToBase64String(Encoding.UTF8.GetBytes(data ?? string.Empty));

    if (pushNotification.Contents.Actions != null)
    {
      foreach (var action in pushNotification.Contents.Actions)
      {
        if (action is NotificationButton actionButton)
        {
          ToastButton button = new ToastButton(
            actionButton.Contents,
            $"action={ToBase64(actionButton.GetWindowsComponentId())}"
          );
          if (actionButton.TextBox != null)
          {
            button.TextBoxId = actionButton.TextBox.GetWindowsComponentId();
          }
          buttons.Add(button);
        }
        else if (action is NotificationTextBox actionTextBox)
        {
          textBox = new ToastTextBox(actionTextBox.GetWindowsComponentId());

          if (!string.IsNullOrEmpty(actionTextBox.Hint))
            textBox.PlaceholderContent = actionTextBox.Hint;
          if (!string.IsNullOrEmpty(actionTextBox.Contents))
            textBox.DefaultInput = actionTextBox.Contents;
          if (!string.IsNullOrEmpty(actionTextBox.Title))
            textBox.Title = actionTextBox.Title;
        }
        else if (action is NotificationComboBox actionComboBox)
        {
          if (actionComboBox.Choices.Length == 0)
            continue;

          comboBox = new ToastSelectionBox(actionComboBox.GetWindowsComponentId());
          if (!string.IsNullOrEmpty(actionComboBox.Hint))
          {
            comboBox.Title = actionComboBox.Hint;
          }

          comboBoxItems = new List<ToastSelectionBoxItem>(actionComboBox.Choices.Length);
          foreach (string option in actionComboBox.Choices)
          {
            if (comboBoxItems.FindIndex(x => x.Id == option) == -1)
            {
              ToastSelectionBoxItem item = new ToastSelectionBoxItem(option, option);
              comboBox.Items.Add(item);
              comboBoxItems.Add(item);
            }
          }

          comboBox.DefaultSelectionBoxItemId = actionComboBox.SelectedItem;
        }
      }
    }

    if (buttons != null)
    {
      bool setReplyButton = false;
      for (int i = 0; i < buttons.Count && i <= 5; i++)
      {
        if (buttons[i] != null)
        {
          bool containsReplyWord = ReplyWordRegex().IsMatch(buttons[i].Content);
          if (!setReplyButton && containsReplyWord && textBox != null)
          {
            // this will set the button to be the 'submit' button for the textbox.
            buttons[i].TextBoxId = textBox.Id;
            setReplyButton = true;
          }
          builder.AddButton(buttons[i]);
        }
      }
    }
    if (textBox != null)
    {
      builder.AddInputTextBox(textBox.Id, textBox.PlaceholderContent, textBox.Title);
      // set current text
    }
    if (comboBox != null && comboBoxItems != null && comboBoxItems.Count > 0)
    {
      string[] choices = comboBoxItems.Select(x => x.Id).ToArray();
      ValueTuple<string, string>[] choicesTuple = new ValueTuple<string, string>[choices.Length];

      for (int i = 0; i < choices.Length; i++)
      {
        // TODO: Is this right? Does the id need to be shoved into B64 like the other components?
        choicesTuple[i] = new ValueTuple<string, string>(choices[i], choices[i]);
      }

      if (
        !string.IsNullOrEmpty(comboBox.DefaultSelectionBoxItemId)
        || !string.IsNullOrEmpty(comboBox.Title)
      )
      {
        builder.AddComboBox(
          comboBox.Id,
          comboBox.Title,
          comboBox.DefaultSelectionBoxItemId,
          choicesTuple
        );
      }
      else if (!string.IsNullOrEmpty(comboBox.DefaultSelectionBoxItemId))
      {
        builder.AddComboBox(comboBox.Id, comboBox.DefaultSelectionBoxItemId, choicesTuple);
      }
      else
      {
        builder.AddComboBox(comboBox.Id, choicesTuple);
      }
    }

    // Progress bar?
    if (
      pushNotification.Contents.ProgressData != null
      && pushNotification.Contents.ProgressData.GetPercentage() != 1
    )
    {
      //AdaptiveProgressBar progressBar = pushNotification.Contents.ProgressData.BuildProgressBar();
      //builder.AddVisualChild(progressBar);
      BindableProgressBarValue progressBarValue = new BindableProgressBarValue("progressValue");

      var progressBar = new AdaptiveProgressBar()
      {
        Value = progressBarValue,
        Status = pushNotification.Contents.ProgressData.Status ?? string.Empty,
        Title = pushNotification.Contents.ProgressData.Title,
        ValueStringOverride = pushNotification.Contents.ProgressData.DisplayedValue,
      };

      builder.AddVisualChild(progressBar);
    }

    if (pushNotification.Timestamp != null && pushNotification.Timestamp != DateTime.MinValue)
      builder.AddCustomTimeStamp(pushNotification.Timestamp ?? DateTime.Now);

    switch (pushNotification.Category)
    {
      case NotificationCategory.Call:
        builder.SetToastScenario(ToastScenario.IncomingCall);
        break;

      case NotificationCategory.Alarm:
        builder.SetToastScenario(ToastScenario.Alarm);
        break;
    }

    // Add the application icon
    if (
      !iconSet
      && pushNotification.Contents.Icon != null
      && pushNotification.Contents.Icon.WriteImage()
    )
    {
      builder.AddAppLogoOverride(
        pushNotification.Contents.Icon.Uri,
        ToastGenericAppLogoCrop.Circle,
        pushNotification.Contents.Icon.AlternativeText
      );
    }

    // Add notification image
    if (
      !addedImage
      && pushNotification.Contents.Image != null
      && pushNotification.Contents.Image.WriteImage()
    )
    {
      builder.AddInlineImage(
        pushNotification.Contents.Image.Uri,
        pushNotification.Contents.Image.AlternativeText
      );
    }

    if (!string.IsNullOrEmpty(pushNotification.Contents.AttributionText))
    {
      builder.AddAttributionText(pushNotification.Contents.AttributionText);
    }

    return builder.GetXml();
  }

  #endregion


  #region Events
  public static void Notification_Dismissed(ToastNotification sender, ToastDismissedEventArgs args)
  {
    if (args.Reason == ToastDismissalReason.TimedOut)
    {
      return;
    }

    IPushNotification? pushNotification = RemoveAndCancelNotification(sender);
    var reason = args.Reason switch
    {
      ToastDismissalReason.UserCanceled => NotificationDismissalReason.UserCanceled,
      ToastDismissalReason.ApplicationHidden => NotificationDismissalReason.ApplicationHidden,
      ToastDismissalReason.TimedOut => NotificationDismissalReason.TimedOut,
      _ => NotificationDismissalReason.Unknown,
    };

    pushNotification?.OnDimsissed(reason);
  }

  private static void Notification_Failed(ToastNotification sender, ToastFailedEventArgs args)
  {
    IPushNotification? pushNotification = RemoveAndCancelNotification(sender);
    string tag = GetId(sender.Tag);

    NotificationFailureReason reason = NotificationFailureReason.Unknown;
    string details = "";
    switch (_toastNotifier?.Setting)
    {
      case NotificationSetting.Enabled:
        reason = NotificationFailureReason.Exception;
        NotificationRuntime.Current.RuntimeLog.Error(
          $"Notification ${tag} failed to send.",
          args.ErrorCode
        );
        break;

      case NotificationSetting.DisabledForApplication:
        reason = NotificationFailureReason.DisabledForApplication;
        details =
          "Enable notifications for "
          + UnifyRuntime.Current.ApplicationId
          + " inside the Settings app -> System -> Notifications & actions -> Get notifications from these senders.";
        NotificationRuntime.Current.RuntimeLog.Warning(
          "Failed to send push notification, notifications are disabled for this application."
        );
        break;

      case NotificationSetting.DisabledForUser:
        reason = NotificationFailureReason.DisabledForUser;
        NotificationRuntime.Current.RuntimeLog.Warning(
          "Failed to send push notification, notifications are disabled for this user."
        );
        details =
          "Notifications are disabled for your user account. Enable them inside the Settings app -> System -> Notifications & actions -> Enable \"Get notifications from apps and other senders.\"";
        break;

      case NotificationSetting.DisabledByGroupPolicy:
        reason = NotificationFailureReason.DisabledForDevice;
        NotificationRuntime.Current.RuntimeLog.Warning(
          "Failed to send push notification, notifications are disabled by group policy."
        );
        details =
          "Notifications are disabled by your organization (via group policy). View more information inside the Settings app -> System -> Notifications & actions";
        // Can check registry here...
        break;

      case NotificationSetting.DisabledByManifest:
        reason = NotificationFailureReason.DisabledForApplication;
        details = "Notifications are disabled for this application by the developer.";
        NotificationRuntime.Current.RuntimeLog.Warning(
          "Failed to send push notification, notifications are disabled via the application manifest yet you tried anyways?"
        );
        break;
    }

    pushNotification?.OnFailed(reason, details);
  }

  private static void Notification_Activated(ToastNotification sender, object args)
  {
    ToastActivatedEventArgs toastArgs = (ToastActivatedEventArgs)args;
    string tag = GetId(sender.Tag);

    NotificationRuntime.Current.RuntimeLog.Debug(
      $"{nameof(WindowsNotificationManager)}#{nameof(Notification_Activated)}",
      $"Notification {tag} (group: {sender.Group}) activated!"
    );

    // Remove from active list
    IPushNotification? pushNotification = RemoveAndCancelNotification(sender);

    // Add data for actions
    Dictionary<INotificationAction, string?> activatedActions = [];
    NotificationButton? activatedButton = null;
    if (!string.IsNullOrEmpty(toastArgs.Arguments))
    {
      var match = ActivatedNotificationArgumentsButtonIdRegex().Match(toastArgs.Arguments);
      if (match.Success && !string.IsNullOrEmpty(match.Groups["id"].Value))
      {
        // Found our button
        var buttonId = GetActionId(match.Groups["id"].Value);
        activatedButton =
          pushNotification?.Contents.GetNotificationAction(buttonId) as NotificationButton;
      }
    }

    foreach (var inputItem in toastArgs.UserInput)
    {
      var splitKey = inputItem.Key.Split(INotificationActionExtensions.ActionDelimiter);
      if (splitKey.Length != 2)
      {
        continue;
      }

      var inputId = GetId(splitKey[1]);
      INotificationAction? inputAction = pushNotification?.Contents.GetNotificationAction(inputId);
      if (inputAction == null)
      {
        continue;
      }

      var value = inputItem.Value as string;
      activatedActions.Add(inputAction, value);

      if (!string.IsNullOrEmpty(value))
      {
        if (inputAction is NotificationTextBox notificationTextBox)
        {
          notificationTextBox.Contents = value;
        }
        else if (inputAction is NotificationComboBox notificationComboBox)
        {
          notificationComboBox.SelectedIndex = notificationComboBox.Choices.IndexOf(value);
        }

        // Activate action if there's a value, or...
        inputAction.OnActivated(value);
      }
      else if (activatedButton?.TextBox == inputAction)
      {
        // ...textbox button clicked
        inputAction.OnActivated(value);
      }
    }

    activatedButton?.OnActivated();

    pushNotification?.OnActivated(
      new NotificationActivationArguments(activatedActions, activatedButton)
    );
  }

  private static IPushNotification? RemoveAndCancelNotification(ToastNotification sender)
  {
    try
    {
      if (_activeNotifications.Remove(sender.Group + "_" + sender.Tag, out var pushNotification))
      {
        pushNotification.Cancel();

        return pushNotification;
      }
    }
    catch (Exception exception)
    {
      NotificationRuntime.Current.RuntimeLog.Error(
        $"{nameof(WindowsNotificationManager)}#{nameof(RemoveAndCancelNotification)}",
        $"Failed to remove and cancel {sender.Tag} (group: {sender.Group})!",
        exception
      );
    }
    return null;
  }
  #endregion
#pragma warning restore CA1416 // Validate platform compatibility
}
#endif
