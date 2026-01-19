using CNCO.Unify.Logging;
using CNCO.Unify.Notifications.Push;
using CNCO.Unify.Storage;

namespace CNCO.Unify.Notifications;

[LinkRuntime(typeof(UnifyRuntime))]
public class NotificationRuntime : Runtime
{
  private static NotificationRuntime? _instance;
  private static IPlatformPushNotificationManager? _notificationManager;
  private static ILocalFileStorage? _imageFileStore;

  #region Locks
  // Lock used when initializing this class.
  private static new readonly object _initializationLock = new object();
  #endregion

  internal new ILogger RuntimeLog
  {
    get => base.RuntimeLog;
  }

  /// <summary>
  /// Current, shared <see cref="IPlatformPushNotificationManager"/>.
  /// </summary>
  public static INotificationsManager NotificationManager
  {
    get
    {
      if (_notificationManager == null)
      {
        lock (_initializationLock)
        {
          _notificationManager ??= new Push.NotificationManager();
        }
      }
      return _notificationManager;
    }
  }

  public static ILocalFileStorage ImageFileStore
  {
    get
    {
      if (_imageFileStore == null)
      {
        lock (_initializationLock)
        {
          _imageFileStore ??= new LocalFileStorage(
            Path.Combine(UnifyRuntime.FileStorage.Directory, "notification_images")
          );
        }
      }
      return _imageFileStore;
    }
  }

  public static NotificationRuntime Current
  {
    get
    {
      if (_instance == null)
      { // Null?
        lock (_initializationLock)
        { // Should only hit one time.
          _instance ??= new NotificationRuntime();
        }
      }
      return _instance;
    }
  }

  public NotificationRuntimeConfiguration Configuration { get; init; }

  public NotificationRuntime(NotificationRuntimeConfiguration? configuration = null)
  {
    Configuration = configuration ?? new();
    if (_instance != null)
    {
      return;
    }

    lock (_initializationLock)
    {
      if (_instance != null)
        return;
      _instance = this;
    }

    // This will be ran once UnifyRuntime.Initialize() is invoked.
    var hookAction = new Action<UnifyRuntime>(async runtimeTarget =>
    {
      try
      {
        if (NotificationManager is IPlatformPushNotificationManager platformPushNotificationManager)
        {
          await platformPushNotificationManager.RegisterAsync();
        }
      }
      catch (Exception ex)
      {
        var tag = $"{nameof(NotificationRuntime)}::{nameof(NotificationManager)}";
        var notificationManagerTag =
          $"{_notificationManager?.GetType().Name ?? "NO_TYPE"}::{nameof(IPlatformPushNotificationManager.RegisterAsync)}";
        Current.RuntimeLog.Error(
          $"{tag}=>{notificationManagerTag}()",
          "Unable to register platform notification manager!",
          ex
        );
      }
    });
    RuntimeHook notificationRegisterHook = new RuntimeHook()
    {
      Name = $"{GetType().Name}${nameof(NotificationManager)}-Register",
      Action = hookAction,
    };
    AddHook(notificationRegisterHook);
  }

  public static NotificationRuntime Create() => Current;
}
