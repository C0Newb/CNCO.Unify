namespace CNCO.Unify.Security.Credentials {
    /// <summary>
    /// Gets the current <see cref="ICredentialManager"/> for the current platform
    /// </summary>
    public class CredentialManagerFactory {
        public static ICredentialManagerEndpoint GetPlatformCredentialManager() {
#pragma warning disable CA1416 // Validate platform compatibility
            if (Platform.IsWindows())
                return new WindowsCredentialManager();

            if (Platform.IsAndroid())
                return new FileBasedCredentialManager();

            //if (Platform.IsLinux())
            //    return new LinuxCredentialManager();

            //if (Platform.IsApple())
            //    return new AppleCredentialManager();
#pragma warning restore CA1416 // Validate platform compatibility

            return new FileBasedCredentialManager();
        }
    }
}
