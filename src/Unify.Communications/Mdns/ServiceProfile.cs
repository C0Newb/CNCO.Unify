using System.Net;

namespace CNCO.Unify.Communications.Mdns;

/// <summary>
/// A service that can be discovered.
/// </summary>
public class ServiceProfile
{
  private IEnumerable<IPAddress> _addresses;
  private string _instanceName;
  private ushort _port;


  /// <summary>
  /// Creates a new instance of the <see cref="ServiceProfile"/> class.
  /// </summary>
  public ServiceProfile(string instanceName, string serviceType, ushort port, string protocol = "tcp", IEnumerable<IPAddress>? addresses = null)
  {
    _port = port;
    _addresses = addresses ?? InterfaceHelpers.GetLocalIPAddresses();
    _instanceName = instanceName;

    ServiceType = serviceType;
    Protocol = protocol;
  }

  /// <summary>
  /// Top level domain (TLD) name of the service. Always <c>local</c>.
  /// </summary>
  public string Domain { get; } = "local";

  /// <summary>
  /// The <see cref="InstanceName"/>, <see cref="ServiceType"/>, <see cref="Protocol"/> and <see cref="Domain"/> joined and separated by a period.
  /// </summary>
  public string FullyQualifiedName => $"{InstanceName}._{ServiceType}._{Protocol}.{Domain}";

  /// <summary>
  /// The fully qualified name of the instance's host. This is the <see cref="InstanceName"/> and <see cref="Domain"/>.
  /// </summary>
  public string HostName => $"{InstanceName}.{Domain}";

  /// <summary>
  /// Unique identifier for the service instance.
  /// </summary>
  public string InstanceName
  {
    get => _instanceName;
    set
    {
      if (_instanceName == value)
        return;
      _instanceName = value;

    }
  }

  /// <summary>
  /// Protocol used as part of the SRV record. This is after <see cref="ServiceType"/> but before the domain.
  /// </summary>
  /// <remarks>
  /// The underscore character (<c>_</c>) is handled automatically, do not include it.
  /// </remarks>
  public string Protocol { get; set; } = "tcp";

  /// <summary>
  /// The <see cref="ServiceType"/>, <see cref="Protocol"/> and <see cref="Domain"/> joined and separated by a period, such as <c>_service._tcp.local</c>
  /// </summary>
  public string QualifiedServiceName => $"_{ServiceType}._{Protocol}.{Domain}";

#if false
      /// <summary>
      /// DNS resource records that are used to locate the service instance.
      /// </summary>
      //public List<ResourceRecord> Resources { get; set; } = new List<ResourceRecord>();
#endif

  /// <summary>
  /// Name of your service, such as <c>http</c> or <c>printer</c>.
  /// </summary>
  /// <remarks>
  /// The underscore character (<c>_</c>) is handled automatically, do not include it.
  /// </remarks>
  public string ServiceType { get; set; }

  /// <summary>
  /// A list of service features implemented by the service instance.
  /// </summary>
  /// <value>
  /// The default is an empty list.
  /// </value>
  /// <seealso href="https://tools.ietf.org/html/rfc6763#section-7.1"/>
  public List<string> SubTypes { get; set; } = new List<string>();
}
