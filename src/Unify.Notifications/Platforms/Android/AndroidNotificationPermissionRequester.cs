#if ANDROID_PUSH_NOTIFICATIONS
using Android.Content.PM;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using System.Runtime.Versioning;
using AndroidApplication = Android.App.Application;
using AndroidPermission = Android.Manifest.Permission;

namespace CNCO.Unify.Notifications.Platforms.Android;

/// <summary>
/// Tracks the current foreground <see cref="Activity"/> and drives the POST_NOTIFICATIONS
/// (API 33+) runtime permission prompt.
/// </summary>
/// <remarks>
/// There is no reliable way to await <c>ActivityCompat.RequestPermissions</c>' result without
/// the hosting app's <see cref="Activity"/> forwarding <c>OnRequestPermissionsResult</c>, which
/// would require every consuming app to add boilerplate. Instead, this relies on the fact that
/// the OS always resumes the requesting activity immediately after the permission dialog is
/// dismissed (granted or denied) - <see cref="Application.IActivityLifecycleCallbacks"/> picks
/// that resume up and re-checks the permission then, with no cooperation needed from the app.
/// </remarks>
[SupportedOSPlatform("android26.0")]
internal static class AndroidNotificationPermissionRequester
{
  private const int PostNotificationsRequestCode = 42871;

  private static readonly Lock _lock = new();
  private static bool _callbacksRegistered;
  private static Activity? _currentActivity;
  private static TaskCompletionSource<bool>? _pendingRequest;

  /// <summary>
  /// Ensures the POST_NOTIFICATIONS permission is granted, prompting the user via the current
  /// foreground activity if it isn't (API 33+ only; earlier versions granted at install time).
  /// </summary>
  /// <remarks>
  ///
  /// </remarks>
  /// <returns>Whether the permission is (now) granted.</returns>
  internal static async Task<bool> EnsureGrantedAsync()
  {
    if (!OperatingSystem.IsAndroidVersionAtLeast(33))
    {
      NotificationRuntime.Current.RuntimeLog.Debug(
        $"{nameof(AndroidNotificationPermissionRequester)}::{nameof(EnsureGrantedAsync)}",
        "Application has notification permissions (Android <33)"
      );
      return HasPermission();
    }

    if (HasPermissionAndroid33())
    {
      NotificationRuntime.Current.RuntimeLog.Debug(
        $"{nameof(AndroidNotificationPermissionRequester)}::{nameof(EnsureGrantedAsync)}",
        "Application has notification permissions (Android 33+)"
      );
      return true;
    }

    EnsureCallbacksRegistered();

    // Pulled now - racing...
    Activity? activity = _currentActivity;
    if (activity == null)
    {
      // No foreground activity to prompt from (e.g. registering before any UI is shown).
      return false;
    }

    TaskCompletionSource<bool> request;
    lock (_lock)
    {
      if (_pendingRequest != null)
      {
        request = _pendingRequest;
      }
      else
      {
        request = _pendingRequest = new TaskCompletionSource<bool>(
          TaskCreationOptions.RunContinuationsAsynchronously
        );
        ActivityCompat.RequestPermissions(
          activity,
          [AndroidPermission.PostNotifications],
          PostNotificationsRequestCode
        );
      }
    }

    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
    using var registration = timeout.Token.Register(CompletePendingRequest);
    return await request.Task;
  }

  private static bool HasPermission()
  {
    var areEnabled =
      NotificationManagerCompat.From(AndroidApplication.Context)?.AreNotificationsEnabled()
      ?? false;
    if (!areEnabled)
    {
      NotificationRuntime.Current.RuntimeLog.Info(
        $"{nameof(AndroidNotificationPermissionRequester)}::{nameof(HasPermission)}",
        "Notifications are disabled for this app in system settings."
      );
    }

    return areEnabled;
  }

  private static bool HasPermissionAndroid33() =>
    OperatingSystem.IsAndroidVersionAtLeast(33)
    && ContextCompat.CheckSelfPermission(
      AndroidApplication.Context,
      AndroidPermission.PostNotifications
    ) == Permission.Granted;

  private static void EnsureCallbacksRegistered()
  {
    if (_callbacksRegistered)
    {
      return;
    }

    lock (_lock)
    {
      if (_callbacksRegistered)
      {
        return;
      }

      if (AndroidApplication.Context is Application application)
      {
        var updateActivity = new Action<Activity?>(activity =>
        {
          lock (_lock)
          {
            _currentActivity = activity;
          }
        });

        application.RegisterActivityLifecycleCallbacks(new LifecycleCallbacks(updateActivity));
      }
      _callbacksRegistered = true;
    }
  }

  private static void CompletePendingRequest()
  {
    TaskCompletionSource<bool>? request;
    lock (_lock)
    {
      request = _pendingRequest;
      _pendingRequest = null;
    }
    _ = (request?.TrySetResult(HasPermissionAndroid33()));
  }

  private sealed class LifecycleCallbacks(Action<Activity?> updateActivity)
    : Java.Lang.Object,
      Application.IActivityLifecycleCallbacks
  {
    public void OnActivityCreated(Activity activity, Bundle? savedInstanceState) { }

    public void OnActivityDestroyed(Activity activity)
    {
      if (ReferenceEquals(_currentActivity, activity))
      {
        updateActivity.Invoke(null);
      }
    }

    public void OnActivityPaused(Activity activity) { }

    public void OnActivityResumed(Activity activity)
    {
      updateActivity(activity);
      CompletePendingRequest();
    }

    public void OnActivitySaveInstanceState(Activity activity, Bundle outState) { }

    public void OnActivityStarted(Activity activity) { }

    public void OnActivityStopped(Activity activity) { }
  }
}
#endif
