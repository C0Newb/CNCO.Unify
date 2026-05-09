namespace CNCO.Unify.Device.Monitor;

public class Cpu : IDisposable
{
  private static readonly string? ThermalZonePath = FindCpuThermalZone();
  private bool disposedValue;

  /// <summary>
  /// Current CPU usage as a percentage.
  /// </summary>
  /// <remarks>
  /// Rounded to two decimal places.
  /// Updated via a thread in the background every <see cref="UsageUpdateIntervalMs"/> milliseconds.
  /// </remarks>
  public double Usage => CpuMonitorService.GetCpuUsage();

  /// <summary>
  /// How frequently <see cref="Usage"/> is updated.
  /// </summary>
  public static int UsageUpdateIntervalMs => CpuMonitorService.CpuUsageSampleDelayMs;

  /// <summary>
  /// Current CPU temperature.
  /// </summary>
  /// <remarks>
  /// Rounded to two decimal places.
  /// </remarks>
  public double? Temperature => GetCpuTemperature();

  public double? FanSpeed => null; // Not implemented yet

  public Cpu()
  {
    CpuMonitorService.AddListener();
  }

  // Get CPU temperature
  private double? GetCpuTemperature()
  {
    if (Platform.IsWindows())
    {
      return Math.Floor(GetCpuTemperatureWindows() * 100) / 100;
    }
    else if (Platform.IsDesktop() || Platform.IsAndroid())
    {
      return Math.Floor(GetTemperatureUnix() * 100) / 100;
    }

    return null;
  }

  #region Windows Specific Code
  private double GetCpuTemperatureWindows()
  {
    try
    {
      return 0;
    }
    catch (Exception)
    {
      return 0;
    }
  }
  #endregion

  #region Linux/Mac Specific Code
  // Read CPU temperature for Linux/Mac systems
  private static double GetTemperatureUnix()
  {
    try
    {
      if (!string.IsNullOrEmpty(ThermalZonePath))
      {
        // Check if the correct thermal zone exists
        string tempFile = $"/sys/class/thermal/{ThermalZonePath}/temp";
        if (File.Exists(tempFile))
        {
          using var fileStream = new FileStream(tempFile, FileMode.Open, FileAccess.Read);
          using var streamReader = new StreamReader(fileStream);
          string? text = streamReader.ReadLine();
          if (text != null && text.Length > 0 && int.TryParse(text, out var temp))
          {
            return temp / 1000f; // Convert to Celsius
          }
        }
      }
    }
    catch (Exception ex)
    {
      // TODO: Log once and move on.
    }

    return double.NaN;
  }

  // Find the appropriate CPU thermal zone by checking the 'type' file
  private static string? FindCpuThermalZone()
  {
    try
    {
      var thermalZoneDirectories = Directory.GetDirectories("/sys/class/thermal/");
      foreach (var zone in thermalZoneDirectories)
      {
        string typeFile = Path.Combine(zone, "type");
        if (File.Exists(typeFile))
        {
          string type = File.ReadAllText(typeFile).Trim();
          if (type == "cpu-thermal")
          {
            return Path.GetFileName(zone);
          }
        }
      }
    }
    catch (Exception ex)
    {
      // TODO: Log once and move on.
    }

    return null;
  }
  #endregion

  protected virtual void Dispose(bool disposing)
  {
    if (!disposedValue)
    {
      if (disposing)
      {
        CpuMonitorService.RemoveListener();
      }

      disposedValue = true;
    }
  }

  public void Dispose()
  {
    // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }
}
