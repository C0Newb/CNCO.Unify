using CNCO.Unify.Logging;

namespace CNCO.Unify.Device;

[LinkRuntime(typeof(UnifyRuntime))]
public sealed class DeviceRuntime : Runtime
{
  private static Lazy<DeviceRuntime> _instance = new Lazy<DeviceRuntime>(() => new DeviceRuntime());
  protected static readonly Lock _log = new();

  public static DeviceRuntime Current => _instance.Value;

  internal new ILogger RuntimeLog => base.RuntimeLog;

  private DeviceRuntime() { }
}
