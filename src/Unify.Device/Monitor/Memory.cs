using System.Diagnostics;

namespace CNCO.Unify.Device.Monitor;

public static class Memory
{
  public static uint GetUsage()
  {
    if (Platform.IsWindows())
    {
      return GetWindowsMemoryUsage();
    }
    else if (Platform.IsLinux() || Platform.IsMacOS() || Platform.IsAndroid())
    {
      return GetUnixMemoryUsage();
    }
    else
    {
      throw new PlatformNotSupportedException("Unsupported operating system.");
    }
  }

  private static uint GetWindowsMemoryUsage() => WindowsMemoryInfo.GetStatus().MemoryLoad;

  private static uint GetUnixMemoryUsage()
  {
    try
    {
      var startInfo = new ProcessStartInfo()
      {
        FileName = "free",
        Arguments = "-m",
        RedirectStandardOutput = true,
      };
      using var proc = new Process() { StartInfo = startInfo };
      proc.Start();
      proc.WaitForExit(500);

      var lines = proc.StandardOutput.ReadToEnd().Split('\n');
      if (lines.Length < 2)
      {
        return 0;
      }
      var memLine = lines[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
      if (memLine.Length < 3)
      {
        return 0;
      }

      var total = double.Parse(memLine[1]);
      var used = double.Parse(memLine[2]);
      return (uint)((used / total) * 100);
    }
    catch
    {
      return 0;
    }
  }
}
