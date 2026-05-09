namespace CNCO.Unify.Device;

public record DeviceStatus
{
  public double? Temperature
  {
    get => field;
    set => field = value.HasValue ? Math.Floor(value.Value * 100) / 100 : null;
  }
  public double? TemperatureInFahrenheit =>
    Temperature.HasValue ? Math.Floor(((Temperature.Value * 9 / 5) + 32) * 100) / 100 : null;

  public double? CpuUsage
  {
    get => field;
    set => field = value.HasValue ? Math.Floor(value.Value * 100) / 100 : null;
  }
  public double MemoryUsage { get; init; }
}
