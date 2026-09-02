#if !ANDROID_PUSH_NOTIFICATIONS
using CNCO.Unify.Notifications.Push;
using System.Runtime.Versioning;

namespace CNCO.Unify.Notifications.Platforms.Android;

[SupportedOSPlatform("android")]
internal class AndroidNotificationManager : UnsupportedPlatformNotificationsManager { }
#else
using Android.Content;
using AndroidX.Core.App;
using CNCO.Unify.Notifications.Push;
using System.Runtime.Versioning;
using AndroidApplication = Android.App.Application;
using AndroidNotificationChannel = Android.App.NotificationChannel;
using AndroidNotificationImportance = Android.App.NotificationImportance;
using AndroidNotificationManagerService = Android.App.NotificationManager;

namespace CNCO.Unify.Notifications.Platforms.Android;

/// <summary>
/// Android push (notification) notifications.
/// </summary>
[SupportedOSPlatform("android26.0")]
internal class AndroidNotificationManager : IPlatformPushNotificationManager
{
  /// <summary>
  /// Id of the notification channel every notification sent by this manager is posted to.
  /// </summary>
  /// <remarks>
  /// This is a placeholder, single, "catch all" channel.
  /// Eventually this should be derived per <see cref="NotificationCategory"/>/<see cref="IPushNotification.Group"/>
  /// so users can individually configure notification behavior (sound, importance, etc) per category from
  /// the system settings, same as every other Android app.
  /// </remarks>
  internal const string DefaultChannelId = "default";

  private static Context AppContext => AndroidApplication.Context;

  public async Task RegisterAsync()
  {
    CreateNotificationChannel();

    if (!await AndroidNotificationPermissionRequester.EnsureGrantedAsync())
    {
      NotificationRuntime.Current.RuntimeLog.Warning(
        $"{nameof(AndroidNotificationManager)}::{nameof(RegisterAsync)}",
        "POST_NOTIFICATIONS was not granted."
      );
    }
  }

  public Task UnregisterAsync()
  {
    var manager = (AndroidNotificationManagerService)
      AppContext.GetSystemService(Context.NotificationService)!;
    manager.DeleteNotificationChannel(DefaultChannelId);
    return Task.CompletedTask;
  }

  public async Task<bool> SendAsync(IPushNotification pushNotification)
  {
    if (!await AndroidNotificationPermissionRequester.EnsureGrantedAsync())
    {
      pushNotification.OnFailed(
        NotificationFailureReason.DisabledForUser,
        "Notifications are not permitted (POST_NOTIFICATIONS was denied)."
      );
      return false;
    }

    // TODO: Map the rest of NotificationContents (icon, image, actions, progress, conversation)
    // the same way WindowsNotificationManager.GetToastNotification does. For now this only
    // sends a minimal "This is a test" style notification.
    var builder = new NotificationCompat.Builder(AppContext, DefaultChannelId)!
      .SetContentTitle(pushNotification.Title)!
      .SetContentText(pushNotification.Contents.Text ?? "This is a test")!
      .SetSmallIcon(AppContext.ApplicationInfo!.Icon)!
      .SetPriority(ToAndroidPriority(pushNotification.Priority))!
      .SetAutoCancel(true)!;

    NotificationManagerCompat
      .From(AppContext)!
      .Notify(pushNotification.Id.GetHashCode(), builder.Build());
    return true;
  }

  public PushNotificationUpdateResult Update(IPushNotification pushNotification) =>
    throw new NotImplementedException();

  public bool Cancel(IPushNotification pushNotification)
  {
    NotificationManagerCompat.From(AppContext)!.Cancel(pushNotification.Id.GetHashCode());
    return true;
  }

  public void ClearAll() => NotificationManagerCompat.From(AppContext)!.CancelAll();

  private static void CreateNotificationChannel()
  {
    var channel = new AndroidNotificationChannel(
      DefaultChannelId,
      "Notifications",
      AndroidNotificationImportance.Default
    );

    var manager = (AndroidNotificationManagerService)
      AppContext.GetSystemService(Context.NotificationService)!;
    manager.CreateNotificationChannel(channel);
  }

  private static int ToAndroidPriority(Push.NotificationPriority priority) =>
    priority switch
    {
      Push.NotificationPriority.Low => NotificationCompat.PriorityLow,
      Push.NotificationPriority.High => NotificationCompat.PriorityHigh,
      _ => NotificationCompat.PriorityDefault,
    };
}
#endif
