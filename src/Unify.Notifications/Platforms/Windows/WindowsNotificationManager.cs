#if !WINDOWS_TOAST_NOTIFICATIONS
using CNCO.Unify.Notifications.Push;
using System.Runtime.Versioning;

namespace CNCO.Unify.Notifications.Platforms.Windows;

/// <summary>
/// Windows push (toast) notifications.
/// </summary>
/// <remarks>
/// It is very important to understand that push (toast) notifications on Windows
/// requires a StartMenu shortcut to be created for the application and to register
/// the shortcut with the COM server.
/// </remarks>
[SupportedOSPlatform("windows10.0.19041.0")]
internal class WindowsNotificationManager : UnsupportedPlatformNotificationsManager { }
#else
using CNCO.Unify.Notifications.Push;
using CNCO.Unify.Notifications.Push.Actions;
using CNCO.Unify.Notifications.Push.Eventing;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Collections.Concurrent;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
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
[SupportedOSPlatform("windows10.0.19041.0")]
public partial class WindowsNotificationManager : IPlatformPushNotificationManager
{
  internal const string ProgressBarValueBindingName = "progressValue";
  private const string DefaultNotificationGroup = "default";

  [GeneratedRegex("(?:action=)(?<id>[^&]+)")]
  private static partial Regex ActivatedNotificationArgumentsButtonIdRegex();

  private ToastNotifierCompat? _toastNotifier;
  private static readonly ConcurrentDictionary<string, IPushNotification> _activeNotifications =
    new();

  private ToastNotifierCompat ToastNotifier
  {
    get
    {
      if (_toastNotifier == null)
      {
        _ = RegisterAsync();
      }
      return _toastNotifier!;
    }
  }

  public Task<bool> SendAsync(IPushNotification pushNotification)
  {
    _toastNotifier ??= ToastNotificationManagerCompat.CreateToastNotifier();
    ToastNotification toastNotification = GetToastNotification(pushNotification);
    _toastNotifier.Show(toastNotification);
    _ = _activeNotifications.TryAdd(
      ToastNotificationTools.GetNotificationGroupTagId(pushNotification),
      pushNotification
    );

    return Task.FromResult(NotificationExists(pushNotification));
  }

  public PushNotificationUpdateResult Update(IPushNotification pushNotification)
  {
    try
    {
      _toastNotifier ??= ToastNotificationManagerCompat.CreateToastNotifier();

      string tag = ToastNotificationTools.GetTag(pushNotification);
      string group = pushNotification.Group;

      if (pushNotification.Contents.ProgressData != null)
      {
        var autoCancelDelay = NotificationRuntime
          .Current
          .Configuration
          .AutoCancelNotificationOnProgressBarCompletionDelay;
        if (
          pushNotification.Contents.ProgressData.GetPercentage() >= 1
          && autoCancelDelay > TimeSpan.Zero
          && autoCancelDelay <= TimeSpan.FromMinutes(1)
        )
        {
          // Progress bar finished! Delete in ...
          _ = Task.Run(async () =>
          {
            await Task.Delay(autoCancelDelay);
            ToastNotificationManagerCompat.History.Remove(tag, group);
          });

          return PushNotificationUpdateResult.Succeeded;
        }
      }

      switch (_toastNotifier.Update(GetToastNotification(pushNotification).Data, tag, group))
      {
        case NotificationUpdateResult.Succeeded:
          return PushNotificationUpdateResult.Succeeded;

        case NotificationUpdateResult.NotificationNotFound:
          return PushNotificationUpdateResult.NotFound;

        default:
          NotificationRuntime.Current.RuntimeLog.Warning(
            $"{GetType().Name}::{nameof(Update)}",
            $"Failed to update notification {pushNotification.Id}"
          );
          return PushNotificationUpdateResult.Failed;
      }
    }
    catch (Exception e)
    {
      NotificationRuntime.Current.RuntimeLog.Error(
        $"Failed to update notification {pushNotification.Id}",
        e
      );
      return PushNotificationUpdateResult.Failed;
    }
  }

  public bool Cancel(IPushNotification pushNotification)
  {
    ToastNotificationManagerCompat.History.Remove(
      ToastNotificationTools.GetTag(pushNotification),
      pushNotification.Group ?? DefaultNotificationGroup
    );
    pushNotification.Contents.DeleteImages();

    pushNotification.OnDismissed(NotificationDismissalReason.ApplicationHidden);

    return !NotificationExists(pushNotification);
  }

  public void ClearAll()
  {
    ToastNotificationManagerCompat.History.Clear();
    try
    {
      foreach (var notification in _activeNotifications.Values)
      {
        notification.Contents?.DeleteImages();
      }
      _ = UnifyRuntime.FileStorage.Delete(NotificationRuntime.ImageFileStore.Directory);
    }
    catch (Exception ex)
    {
      NotificationRuntime.Current.RuntimeLog.Error(
        $"{GetType().Name}::{nameof(ClearAll)}",
        "Failed to clear notification image storage.",
        ex
      );
    }
  }

  public Task RegisterAsync() => Register(null);

  private Task Register(Exception? registrationException)
  {
    try
    {
      // First attempt?
      if (registrationException == null)
      {
        // Setup notification support
        NotificationRegistry.RegisterAppForNotificationSupport(true);
        // Fired if a notification is activated and the app is closed.
        ToastNotificationManagerCompat.OnActivated += ToastNotificationManagerCompat_OnActivated;
        _toastNotifier = ToastNotificationManagerCompat.CreateToastNotifier();
        return Task.CompletedTask;
      }

      NotificationRuntime.Current.RuntimeLog.Error(
        "Failed to register ToastNotifier into Windows (1)",
        registrationException
      );
      ToastNotificationManagerCompat.Uninstall();
      NotificationRegistry.UninstallShortcut();

      NotificationRegistry.RegisterAppForNotificationSupport(true);
      _toastNotifier = ToastNotificationManagerCompat.CreateToastNotifier();
    }
    catch (Exception exception)
    {
      // First attempt?
      if (registrationException == null)
      {
        NotificationRuntime.Current.RuntimeLog.Error(
          "Failed to register ToastNotifier into Windows (1)",
          exception
        );

        // Try again
        return Register(exception);
      }

      NotificationRuntime.Current.RuntimeLog.Error(
        $"{GetType().Name}::{nameof(Register)}",
        "Unable to register the ToastNotifier into Windows (2, forced)",
        exception
      );
    }

    return Task.CompletedTask;
  }

  public Task UnregisterAsync()
  {
    try
    {
      ClearAll();
      ToastNotificationManagerCompat.Uninstall();
      NotificationRegistry.UninstallShortcut();
    }
    catch (Exception e)
    {
      NotificationRuntime.Current.RuntimeLog.Error(
        $"{GetType().Name}::{nameof(UnregisterAsync)}",
        "Unable to unregister the ToastNotifier on Windows. This is mostly fine, but there may be a lingering Start Menu shortcut.",
        e
      );
    }
    return Task.CompletedTask;
  }

  private static bool NotificationExists(IPushNotification pushNotification) =>
    ToastNotificationManagerCompat
      .History.GetHistory()
      .Any(t =>
        t.Group == pushNotification.Group
        && t.Tag == ToastNotificationTools.GetTag(pushNotification)
      );

  private ToastNotification GetToastNotification(IPushNotification pushNotification)
  {
    var toastXml = ToastNotificationTools.GetToastXml(pushNotification);
    ToastNotification toast = new ToastNotification(toastXml);

    toast.Activated += Notification_Activated;
    toast.Dismissed += Notification_Dismissed;
    toast.Failed += Notification_Failed;

    toast.Tag = ToastNotificationTools.GetTag(pushNotification);
    toast.Group = string.IsNullOrEmpty(pushNotification.Group)
      ? DefaultNotificationGroup
      : pushNotification.Group;
    toast.SuppressPopup = pushNotification.IsSilent;
    ToastNotificationTools.SetNotificationPriority(pushNotification, toast);

    if (pushNotification.Contents.ProgressData != null)
    {
      toast.Data = new NotificationData([
        new KeyValuePair<string, string>(
          ProgressBarValueBindingName,
          pushNotification.Contents.ProgressData.GetPercentage().ToString()
        ),
      ]);
    }

    // TODO: Store notification to persist an app restart/system reboot?
    toast.ExpiresOnReboot = true;
    return toast;
  }

  #region Events
  private static void Notification_Dismissed(ToastNotification sender, ToastDismissedEventArgs args)
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

    pushNotification?.OnDismissed(reason);
  }

  private static void Notification_Activated(ToastNotification sender, object args)
  {
    ToastActivatedEventArgs toastArgs = (ToastActivatedEventArgs)args;
    string tag = ToastNotificationTools.GetId(sender.Tag);

    NotificationRuntime.Current.RuntimeLog.Debug(
      $"{nameof(WindowsNotificationManager)}#{nameof(Notification_Activated)}",
      $"Notification {tag} (group: {sender.Group}) activated!"
    );

    // Remove from active list
    IPushNotification? pushNotification = RemoveAndCancelNotification(sender);

    // Add data for actions
    Dictionary<INotificationAction, string?> activatedActions = [];
    NotificationButton? activatedButton = GetActivatedButtonFromActivatedNotification(
      toastArgs,
      pushNotification
    );

    foreach (var inputItem in toastArgs.UserInput)
    {
      var splitKey = inputItem.Key.Split(INotificationActionExtensions.ActionDelimiter);
      if (splitKey.Length != 2)
      {
        continue;
      }

      var inputId = ToastNotificationTools.GetId(splitKey[1]);
      INotificationAction? inputAction = pushNotification?.Contents.GetNotificationAction(inputId);
      if (inputAction == null || inputItem.Value is not string value)
      {
        continue;
      }

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

  private static NotificationButton? GetActivatedButtonFromActivatedNotification(
    ToastActivatedEventArgs toastArgs,
    IPushNotification? pushNotification
  )
  {
    NotificationButton? activatedButton = null;
    if (!string.IsNullOrEmpty(toastArgs.Arguments))
    {
      var match = ActivatedNotificationArgumentsButtonIdRegex().Match(toastArgs.Arguments);
      if (match.Success && !string.IsNullOrEmpty(match.Groups["id"].Value))
      {
        // Found our button
        var buttonId = ToastNotificationTools.GetActionId(match.Groups["id"].Value);
        activatedButton =
          pushNotification?.Contents.GetNotificationAction(buttonId) as NotificationButton;
      }
    }

    return activatedButton;
  }

  private static void ToastNotificationManagerCompat_OnActivated(
    ToastNotificationActivatedEventArgsCompat e
  )
  {
    // TODO: Investigate this and what to do here..
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
    {
      data.Add("textboxText", e.UserInput["textInput"] as string);
    }
    if (e.UserInput.ContainsKey("combobox"))
    {
      data.Add("comboBoxSelectedItem", e.UserInput["combobox"] as string);
    }

    NotificationRuntime.Current.RuntimeLog.Info("Toast activated. Args: " + e.Argument);
  }

  private void Notification_Failed(ToastNotification sender, ToastFailedEventArgs args)
  {
    IPushNotification? pushNotification = RemoveAndCancelNotification(sender);
    string tag = ToastNotificationTools.GetId(sender.Tag);

    NotificationFailureReason reason = NotificationFailureReason.Unknown;
    string details = "";
    switch (ToastNotifier.Setting)
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
  #endregion

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
}
#endif
