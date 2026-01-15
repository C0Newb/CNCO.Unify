using System.Diagnostics;
using System.Threading.Tasks;
using Makaretu.Dns;
using Makaretu.Dns.Resolving;

namespace CNCO.Unify.Communications.Mdns;

public class ServiceDiscovery : IDisposable
{
  private static readonly DomainName LocalDomain = new DomainName("local");
  private static readonly DomainName SubName = new DomainName("_sub");
  private static readonly DomainName DiscoveryServiceName = new DomainName(
    "_services._dns-sd._udp.local"
  );

  private readonly bool _disposeMdnsService = false;

  private readonly List<ServiceProfile> _serviceProfiles = [];
  private readonly MulticastService _multicastService;

  public ServiceDiscovery()
    : this(new MulticastService())
  {
    _disposeMdnsService = true;
    _multicastService.Start();
  }

  public ServiceDiscovery(MulticastService multicastService)
  {
    _multicastService = multicastService;
    _multicastService.QueryReceived += MulticastService_QueryReceived;
    _multicastService.AnswerReceived += MulticastService_AnswerReceived;
  }

  protected virtual void Dispose(bool disposing)
  {
    if (!disposing || _multicastService == null)
    {
      return;
    }

    _multicastService.QueryReceived -= MulticastService_QueryReceived;
    _multicastService.AnswerReceived -= MulticastService_AnswerReceived;

    if (_disposeMdnsService)
    {
      _multicastService.Dispose();
    }
  }

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  public bool AddAdditionalRecordsToAnswers { get; init; } = false;

  public NameServer NameServer { get; } =
    new NameServer() { Catalog = new(), AnswerAllQuestions = true };

  /// <summary>
  /// Raised when a new DNS-SD response is received.
  /// </summary>
  public event EventHandler<DomainName>? ServiceDiscovered;

  public event EventHandler<ServiceInstanceDiscoveryEventArgs>? ServiceInstanceDiscovered;

  public event EventHandler<ServiceInstanceShutdownEventArgs>? ServiceInstanceShutdown;

  public void Advertise(ServiceProfile service)
  {
    if (_serviceProfiles.Contains(service))
    {
      return;
    }

    _serviceProfiles.Add(service);

    var catalog = NameServer.Catalog;
    _ = catalog.Add(
      new PTRRecord() { Name = DiscoveryServiceName, DomainName = service.QualifiedServiceName },
      authoritative: true
    );
    _ = catalog.Add(
      new PTRRecord()
      {
        Name = service.QualifiedServiceName,
        DomainName = service.QualifiedServiceName,
      },
      true
    );

    foreach (var subtype in service.SubTypes)
    {
      if (string.IsNullOrEmpty(subtype))
      {
        continue;
      }

      var subTypePTRRecord = new PTRRecord()
      {
        Name = DomainName.Join(new DomainName(subtype), SubName, service.QualifiedServiceName),
        DomainName = service.FullyQualifiedName,
      };
      _ = catalog.Add(subTypePTRRecord, true);
    }

    foreach (var resource in service.Resources)
    {
      catalog.Add(resource, true);
    }

    catalog.IncludeReverseLookupRecords();
  }

  public async Task Announce(ServiceProfile service)
  {
    var message = new Message() { QR = true };

    var ptrRecord = new PTRRecord()
    {
      Name = service.QualifiedServiceName,
      DomainName = service.FullyQualifiedName,
    };
    message.Answers.Add(ptrRecord);
    service.Resources.ForEach(message.Answers.Add);

    await _multicastService.SendAnswer(message, checkDuplicates: false);
  }

  public async Task Unadvertise()
  {
    foreach (var profile in _serviceProfiles)
    {
      await Unadvertise(profile);
    }
  }

  public async Task Unadvertise(ServiceProfile service)
  {
    var message = new Message() { QR = true };

    var ptrRecord = new PTRRecord()
    {
      Name = service.QualifiedServiceName,
      DomainName = service.FullyQualifiedName,
      TTL = TimeSpan.Zero,
    };
    message.Answers.Add(ptrRecord);
    service.Resources.ForEach(resource =>
    {
      resource.TTL = TimeSpan.Zero;
      message.AdditionalRecords.Add(resource);
    });

    await _multicastService.SendAnswer(message);

    NameServer.Catalog.TryRemove(service.QualifiedServiceName, out var _);
  }

  public void QueryServices() =>
    _multicastService.SendQuery(DiscoveryServiceName, dnsType: DnsType.PTR);

  public void QueryServicesUnicast() =>
    _multicastService.SendUnicastQuery(DiscoveryServiceName, dnsType: DnsType.PTR);

  public void QueryService(DomainName service, string? subtype = null) =>
    _multicastService.SendQuery(
      string.IsNullOrEmpty(subtype)
        ? DomainName.Join(service, LocalDomain)
        : DomainName.Join(new DomainName(subtype), SubName, service, LocalDomain),
      dnsType: DnsType.PTR
    );

  public void QueryServiceUnicast(DomainName service, string? subtype = null) =>
    _multicastService.SendUnicastQuery(
      string.IsNullOrEmpty(subtype)
        ? DomainName.Join(service, LocalDomain)
        : DomainName.Join(new DomainName(subtype), SubName, service, LocalDomain),
      dnsType: DnsType.PTR
    );

  private void MulticastService_AnswerReceived(object? sender, MulticastMessageEventArgs e)
  {
    //var tag = $"{GetType().Name}::{nameof(MulticastService_AnswerReceived)}";
    var message = e.Message;

    CommunicationsRuntime.Current.RuntimeLog.Verbose($"Service answer from {e.RemoteEndPoint}.");

    var sd = message.Answers.OfType<PTRRecord>().Where(ptr => ptr.Name.IsSubdomainOf(LocalDomain));
    foreach (var ptr in sd)
    {
      if (ptr.Name == DiscoveryServiceName)
      {
        ServiceDiscovered?.Invoke(this, ptr.DomainName);
      }
      else if (ptr.TTL == TimeSpan.Zero)
      {
        var args = new ServiceInstanceShutdownEventArgs()
        {
          ServiceInstanceName = ptr.DomainName,
          Message = message,
          RemoteEndPoint = e.RemoteEndPoint,
        };
        ServiceInstanceShutdown?.Invoke(this, args);
      }
      else
      {
        var args = new ServiceInstanceDiscoveryEventArgs()
        {
          ServiceInstanceName = ptr.DomainName,
          Message = message,
          RemoteEndPoint = e.RemoteEndPoint,
        };
        ServiceInstanceDiscovered?.Invoke(this, args);
      }
    }
  }

  private void MulticastService_QueryReceived(object? sender, MulticastMessageEventArgs e) =>
    _ = Task.Run(async () => await ProcessQueryReceived(e));

  private async Task ProcessQueryReceived(MulticastMessageEventArgs messageEventArgs)
  {
    var request = messageEventArgs.Message;
    CommunicationsRuntime.Current.RuntimeLog.Verbose(
      $"Query from {messageEventArgs.RemoteEndPoint}"
    );

    var unicastResponseRequired = false;
    foreach (var question in request.Questions)
    {
      if (((ushort)question.Class & 0x8000) != 0)
      {
        unicastResponseRequired = true;
        question.Class = (DnsClass)((ushort)question.Class & 0x7fff);
      }
    }

    var response = await NameServer.ResolveAsync(request);
    if (response.Status != MessageStatus.NoError)
    {
      CommunicationsRuntime.Current.RuntimeLog.Verbose(
        $"Failed to generate response, got non-error response."
      );
      //return;
    }

    if (response.Answers.Any(a => a.Name == DiscoveryServiceName))
    {
      response.AdditionalRecords.Clear();
    }

    if (AddAdditionalRecordsToAnswers)
    {
      response.Answers.AddRange(response.AdditionalRecords);
      response.AdditionalRecords.Clear();
    }

    CommunicationsRuntime.Current.RuntimeLog.Verbose(
      $"Sending mDNS query answer to {messageEventArgs.RemoteEndPoint}"
    );
    await _multicastService.SendAnswer(response, messageEventArgs);
    CommunicationsRuntime.Current.RuntimeLog.Verbose($"MDNS query answered!");

    foreach (var service in _serviceProfiles)
    {
      await Announce(service);
    }
  }
}
