namespace CNCO.Unify {
    public class Platform {
        public static bool IsBrowser() => OperatingSystem.IsBrowser();

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
    }
}
