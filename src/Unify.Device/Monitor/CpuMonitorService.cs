using System.Diagnostics;
using System.Runtime.Versioning;

namespace CNCO.Unify.Device.Monitor;

internal static class CpuMonitorService
{
  internal const int CpuUsageSampleDelayMs = 750;

  private static bool _runThread = true;
  private static readonly Lazy<Thread> _thread = new Lazy<Thread>(() =>
  {
    ThreadStart threadStart = null;

    if (Platform.IsWindows())
    {
      threadStart = MonitorWindowsCpu;
    }
    else if (Platform.IsLinux() || Platform.IsMacOS() || Platform.IsAndroid())
    {
      threadStart = MonitorUnixCpu;
    }
    else
    {
      throw new PlatformNotSupportedException(
        "CPU monitoring is only supported on Windows, Linux, macOS and Android."
      );
    }

    return new Thread(threadStart)
    {
      Priority = ThreadPriority.BelowNormal,
      IsBackground = true,
      Name = "UnifyCpuMonitorThread",
    };
  });
  private static Thread Thread => _thread.Value;

  private static readonly Lazy<PerformanceCounter> _windowsCpuCounter =
    new Lazy<PerformanceCounter>(() =>
      new PerformanceCounter("Processor", "% Processor Time", "_Total")
    );

  private static int Listeners
  {
    get;
    set
    {
      if (value <= 0)
      {
        _runThread = false;
        field = 0;
        return;
      }
      field = value;
    }
  }

  private static double CpuUsagePercentage { get; set; }

  /// <summary>
  /// Gets the current CPU usage.
  /// </summary>
  /// <returns></returns>
  public static double GetCpuUsage()
  {
    _runThread = true;
    if (!Thread.IsAlive)
    {
      if (Listeners <= 0)
      {
        Listeners = 1;
      }

      Thread.Start();

      // Bit checky, wait for some data first...
      Thread.Sleep(CpuUsageSampleDelayMs);
    }

    return CpuUsagePercentage;
  }

  internal static void AddListener()
  {
    Listeners++;
  }

  /// <summary>
  /// Removes CPU monitor listener. If there are no listeners, the monitoring thread stops.
  /// </summary>
  internal static void RemoveListener()
  {
    Listeners--;
  }

  private static void MonitorUnixCpu()
  {
    while (_runThread)
    {
      // Read from /proc/stat (Linux)
      ReadUnixCpuTimes(out double totalTime1, out double idleTime1);
      Thread.Sleep(CpuUsageSampleDelayMs);
      ReadUnixCpuTimes(out double totalTime2, out double idleTime2);

      var totalDelta = totalTime2 - totalTime1;
      var idleDelta = idleTime2 - idleTime1;

      CpuUsagePercentage = (totalDelta - idleDelta) / totalDelta * 100;
    }
  }

  [SupportedOSPlatformGuard("windows")]
  private static void MonitorWindowsCpu()
  {
    _ = ReadWindowsCpuTime();
    while (_runThread)
    {
      Thread.Sleep(CpuUsageSampleDelayMs);
      CpuUsagePercentage = ReadWindowsCpuTime();
    }
  }

  [SupportedOSPlatformGuard("windows")]
  private static double ReadWindowsCpuTime() => (double)_windowsCpuCounter.Value.NextValue();

  private static void ReadUnixCpuTimes(out double totalTime, out double idleTime)
  {
    totalTime = 0;
    idleTime = 0;

    string[] lines = File.ReadAllLines("/proc/stat");
    foreach (string line in lines)
    {
      if (!line.StartsWith("cpu "))
      {
        continue;
      }

      string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
      if (parts.Length >= 7)
      {
        long user = long.Parse(parts[1]);
        long nice = long.Parse(parts[2]);
        long system = long.Parse(parts[3]);
        long idle = long.Parse(parts[4]);
        long ioWait = long.Parse(parts[5]);
        long irq = long.Parse(parts[6]);
        long softIrq = long.Parse(parts[7]);

        idleTime = idle + ioWait;
        long activeTime = user + nice + system + irq + softIrq;
        if (parts.Length >= 8 && long.TryParse(parts[8], out var steal))
        {
          activeTime += steal;
        }
        totalTime = activeTime + idleTime;

        return;
      }
    }
  }
}
