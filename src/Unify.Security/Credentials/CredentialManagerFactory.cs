namespace CNCO.Unify.Security.Credentials;

/// <summary>
/// Gets the current <see cref="ICredentialManager"/> for the current platform
/// </summary>
public static class CredentialManagerFactory
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Interoperability",
    "CA1416:Validate platform compatibility",
    Justification = "It is performed."
  )]
  public static ICredentialManagerEndpoint GetPlatformCredentialManager()
  {
    if (Platform.IsWindows())
    {
      return new WindowsCredentialManager();
    }

    if (Platform.IsAndroid())
    {
      return new AndroidCredentialManager();
    }

    if (Platform.IsLinux())
    {
      return new FileBasedCredentialManager();
    }

    if (Platform.IsApple())
    {
      return new AppleCredentialManager();
    }

    if (Platform.IsBrowser())
    {
      return new InMemoryCredentialManager();
    }

    SecurityRuntime.Current.RuntimeLog.Warning(
      $"{nameof(CredentialManagerFactory)}::{nameof(GetPlatformCredentialManager)}()",
      "Unknown platform, returning an unsecure FileBasedCredentialManager instance!"
    );
    return new FileBasedCredentialManager();
  }
}
