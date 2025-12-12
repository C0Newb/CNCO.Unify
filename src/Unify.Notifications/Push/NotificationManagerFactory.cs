namespace CNCO.Unify.Notifications.Push;

/// <summary>
/// Used to get the current platform's <see cref="INotificationManager"/>.
/// </summary>
internal class NotificationManagerFactory
{
  public static INotificationManager GetPlatformNotificationManager()
  {
#pragma warning disable CA1416 // Validate platform compatibility
    if (Platform.IsWindows())
      return new WindowsNotificationManager();

    /*if (Platform.IsAndroid())
        return new AndroidNotificationManager();

    if (Platform.IsLinux())
        return new LinuxNotificationManager();

    if (Platform.IsApple())
        return new AppleCredentialManager();
    */
#pragma warning restore CA1416 // Validate platform compatibility

    NotificationRuntime.Current.RuntimeLog.Warning(
        $"{nameof(NotificationManagerFactory)}::{nameof(GetPlatformNotificationManager)}()",
        "Unsupported platform!"
    );

    throw new UnsupportedPlatformException(string.Empty, "No supported INotificationManagers found.");
  }
}
