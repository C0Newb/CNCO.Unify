using System.Diagnostics;
using System.Reflection;

namespace CNCO.Unify {
    public class Platform {
        public static PlatformID Id => Environment.OSVersion.Platform;
        public static Version Version => Environment.OSVersion.Version;
        public static string VersionString => Environment.OSVersion.VersionString;


        public static bool IsBrowser() => OperatingSystem.IsBrowser();
        public static bool IsWasi() => OperatingSystem.IsWasi();

        public static bool IsAndroid() => OperatingSystem.IsAndroid();
        public static bool IsLinux() => OperatingSystem.IsLinux();
        public static bool IsFreeBSD() => OperatingSystem.IsFreeBSD();


        public static bool IsIOS() => OperatingSystem.IsIOS();
        public static bool IsMacCatalyst() => OperatingSystem.IsMacCatalyst();
        public static bool IsMacOS() => OperatingSystem.IsMacOS();
        public static bool IsTvOS() => OperatingSystem.IsTvOS();
        public static bool IsWatchOS() => OperatingSystem.IsWatchOS();
        public static bool IsApple() => IsIOS() || IsMacCatalyst() || IsMacOS() || IsTvOS() || IsWatchOS();


        public static bool IsWindows() => OperatingSystem.IsWindows();
        public static bool IsWindows9X() => Environment.OSVersion.Platform == PlatformID.Win32Windows;
        public static bool IsWindowsCE() => Environment.OSVersion.Platform == PlatformID.WinCE;
        public static bool IsXbox360() => Environment.OSVersion.Platform == PlatformID.Xbox;

        public static bool IsMobile() => IsIOS() || IsAndroid() || IsMacCatalyst();
        public static bool IsDesktop() => IsLinux() || IsFreeBSD() || IsWindows();


        public static string GetApplicationRootDirectory() {
            if (IsApple())
                return "./Library/";
            else if (IsAndroid())
                return Path.GetFullPath("..", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)); // -> /data/users/*/<package>/files/
            else
                return Directory.GetCurrentDirectory();
        }

        /// <summary>
        /// Path to the currently executing assembly.
        /// </summary>
        /// <returns></returns>
        public static string GetApplicationPath() => Assembly.GetExecutingAssembly().Location;

        /// <summary>
        /// Version of either the current executing assembly or the provided assembly.
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static Version GetApplicationVersion(Assembly? assembly = null) {
            assembly ??= Assembly.GetExecutingAssembly();
            Version? version = assembly.GetName().Version;
            if (version != null)
                return version;

            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return new Version(fileVersionInfo.FileVersion ?? fileVersionInfo.ProductVersion ?? "0.0.0");
        }
    }
}
