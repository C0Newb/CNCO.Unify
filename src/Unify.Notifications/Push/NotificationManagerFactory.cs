namespace CNCO.Unify.Notifications.Push;

/// <summary>
/// Used to get the current platform's <see cref="INotificationManager"/>.
/// </summary>
internal class NotificationManagerFactory
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Interoperability",
    "CA1416:Validate platform compatibility",
    Justification = "It is checked."
  )]
  public static INotificationManager GetPlatformNotificationManager()
  {
    if (Platform.IsWindows())
    {
      return new WindowsNotificationManager();
    }

    if (Platform.IsAndroid())
    {
      return new AndroidNotificationManager();
    }

    if (Platform.IsLinux())
    {
      return new LinuxNotificationManager();
    }

    if (Platform.IsApple())
    {
      return new AppleCredentialManager();
    }

    if (Platform.IsBrowser())
    {
      return new BrowserNotificationManager();
    }

    NotificationRuntime.Current.RuntimeLog.Warning(
      $"{nameof(NotificationManagerFactory)}::{nameof(GetPlatformNotificationManager)}()",
      "Unsupported platform!"
    );

    throw new UnsupportedPlatformException(
      string.Empty,
      "No supported INotificationManagers found."
    );
  }
}
