using CNCO.Unify.Device.Monitor;

namespace CNCO.Unify.Device;

public class Device
{
  private readonly Cpu _cpu;

  public Device() => _cpu = new Cpu();

  public DeviceStatus GetStatus() =>
    new()
    {
      CpuUsage = _cpu.Usage,
      Temperature = _cpu.Temperature,
      MemoryUsage = Memory.GetUsage(),
    };
}
