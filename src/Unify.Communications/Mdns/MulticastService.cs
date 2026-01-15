using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Makaretu.Dns;

namespace CNCO.Unify.Communications.Mdns;

public class MulticastService : IResolver, IDisposable
{
  private const int PacketOverhead = 48;
  private const int MaxDatagramSize = Message.MaxLength;
  private readonly int _maxPacketSize = MaxDatagramSize - PacketOverhead;

  private readonly TimeSpan _maxLegacyUnicastTTL = TimeSpan.FromSeconds(10);

  private readonly ProcessedMessageIds _sentMessageIds = new();
  private readonly ProcessedMessageIds _receivedMessageIds = new();

  private readonly UdpClient _unicastIPv4Client = new UdpClient(AddressFamily.InterNetwork);
  private readonly UdpClient _unicastIPv6Client = new UdpClient(AddressFamily.InterNetworkV6);

  private Multicast _client;
  private IEnumerable<NetworkInterface> _networkInterfaces = [];

  public MulticastService() => _client = new Multicast([], UseIPv4, UseIPv6);

  ~MulticastService()
  {
    Dispose(true);
  }

  /// <summary>
  /// Raised when a multicast query is received.
  /// </summary>
  public event EventHandler<MulticastMessageEventArgs>? QueryReceived;

  /// <summary>
  /// Raised when any link-local mDNS service responds to a query.
  /// </summary>
  public event EventHandler<MulticastMessageEventArgs>? AnswerReceived;

  /// <summary>
  /// Raised when a malformed message is received.
  /// </summary>
  public event EventHandler<byte[]>? MalformedMessageReceived;

  /// <summary>
  /// Raised when a new network interface is discovered on the system.
  /// </summary>
  public event EventHandler<NetworkInterfaceEventArgs>? NetworkInterfaceDiscovered;

  public bool UseIPv4 { get; init; } = Socket.OSSupportsIPv4;
  public bool UseIPv6 { get; init; } = Socket.OSSupportsIPv6;

  /// <summary>
  /// Wether to ignore duplicate messages received within a short time frame, dertermined by
  /// <see cref="ProcessedMessageIds.MessageRetentionDuration"/>
  /// </summary>
  public bool IgnoreDuplicateMessages { get; init; } = true;

  protected virtual void Dispose(bool disposing)
  {
    if (disposing)
    {
      Stop();

      _client?.Dispose();
      _unicastIPv4Client.Dispose();
      _unicastIPv6Client.Dispose();
    }
  }

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  public void Start()
  {
    _networkInterfaces = [];
    FindNetworkInterfaces();
  }

  public void Stop()
  {
    QueryReceived = null;
    AnswerReceived = null;
    NetworkInterfaceDiscovered = null;
  }

  public async Task<Message> ResolveAsync(Message request, CancellationToken cancel = default)
  {
    var taskCompletionSource = new TaskCompletionSource<Message>();

    CancellationTokenRegistration? cancellationTokenRegistration = null;

    void answerReceived(object? sender, MulticastMessageEventArgs messageEventArgs)
    {
      var response = messageEventArgs.Message;
      // Does it answer our questions?
      if (request.Questions.All(q => response.Answers.Any(a => a.Name == q.Name)))
      {
        AnswerReceived -= answerReceived;
        _ = cancellationTokenRegistration?.Unregister();
        taskCompletionSource.SetResult(response);
      }
    }

    cancellationTokenRegistration = cancel.Register(() =>
    {
      AnswerReceived -= answerReceived;
      _ = taskCompletionSource.TrySetCanceled();
    });

    AnswerReceived += answerReceived;
    await Send(request);

    return await taskCompletionSource.Task;
  }

  public async Task SendAnswer(Message answer, bool checkDuplicates = true)
  {
    answer.Id = 0; // Always
    answer.Questions.Clear();
    answer.Truncate(_maxPacketSize);
    await Send(answer, checkDuplicates);
  }

  public async Task SendAnswer(
    Message answer,
    MulticastMessageEventArgs query,
    bool checkDuplicates = true
  )
  {
    answer.AA = true;
    if (!query.IsLegacyUnicast)
    {
      await SendAnswer(answer, checkDuplicates);
      return;
    }

    answer.Id = query.Message.Id;
    answer.Questions.AddRange(query.Message.Questions);
    answer.Truncate(_maxPacketSize);

    foreach (var resourceRecord in answer.Answers)
    {
      resourceRecord.TTL =
        (resourceRecord.TTL > _maxLegacyUnicastTTL) ? _maxLegacyUnicastTTL : resourceRecord.TTL;
    }
    foreach (var resourceRecord in answer.AdditionalRecords)
    {
      resourceRecord.TTL =
        (resourceRecord.TTL > _maxLegacyUnicastTTL) ? _maxLegacyUnicastTTL : resourceRecord.TTL;
    }

    await Send(answer, checkDuplicates, query.RemoteEndPoint);
  }

  public Task SendQuery(
    DomainName domainName,
    DnsClass dnsClass = DnsClass.IN,
    DnsType dnsType = DnsType.ANY
  )
  {
    var message = new Message() { Opcode = MessageOperation.Query, QR = false };
    message.Questions.Add(
      new Question()
      {
        Name = domainName,
        Class = dnsClass,
        Type = dnsType,
      }
    );

    return Send(message);
  }

  public Task SendUnicastQuery(
    DomainName domainName,
    DnsClass dnsClass = DnsClass.IN,
    DnsType dnsType = DnsType.ANY
  )
  {
    var msg = new Message { Opcode = MessageOperation.Query, QR = false };
    msg.Questions.Add(
      new Question
      {
        Name = domainName,
        Class = (DnsClass)((ushort)dnsClass | 0x8000),
        Type = dnsType,
      }
    );

    return Send(msg);
  }

  public async Task Send(
    Message message,
    bool checkDuplicates = false,
    IPEndPoint? remoteEndPoint = null
  )
  {
    var packet = message.ToByteArray();
    if (packet.Length > _maxPacketSize)
    {
      throw new ArgumentOutOfRangeException(
        nameof(message),
        $"Message exceeds max packet size of {_maxPacketSize}."
      );
    }

    if (checkDuplicates && !_sentMessageIds.TryAdd(packet))
    {
      // Duplicate
      return;
    }

    if (remoteEndPoint != null)
    {
      var unicastClient =
        (remoteEndPoint.Address.AddressFamily == AddressFamily.InterNetwork)
          ? _unicastIPv4Client
          : _unicastIPv6Client;
      _ = await unicastClient.SendAsync(packet, packet.Length, remoteEndPoint);
      return;
    }

    await _client.SendMessageAsync(packet);
  }

  private void OnNetworkAddressChanged(object sender, EventArgs e) => FindNetworkInterfaces();

  private void FindNetworkInterfaces()
  {
    var tag = $"{GetType().Name}::{nameof(FindNetworkInterfaces)}";
    CommunicationsRuntime.Current.RuntimeLog.Debug(tag, "Finding available network interfaces.");

    try
    {
      var newInterfaces = NetworkInterfaces.GetNetworkInterfaces();
      var newInterfaceIds = new HashSet<string>(newInterfaces.Select(i => i.Id));
      var currentInterfaceIds = new HashSet<string>(_networkInterfaces.Select(i => i.Id));

      // Newly discovered interfaces to bind to?
      var interfacesToSetup = newInterfaces.Where(i => !currentInterfaceIds.Contains(i.Id));

      // Any previously bound interface that's not there now?
      var resetClient = newInterfaceIds.Any(id => !currentInterfaceIds.Contains(id));

      _networkInterfaces = newInterfaces;

      // Any discovered?
      if (interfacesToSetup.Any())
      {
        resetClient = true;
        NetworkInterfaceDiscovered?.Invoke(
          this,
          new NetworkInterfaceEventArgs() { NetworkInterfaces = interfacesToSetup }
        );
      }

      if (resetClient)
      {
        // Reset the client
        _client?.Dispose();
        _client = new Multicast(_networkInterfaces, UseIPv4, UseIPv6);
        _client.MessageReceived += ProcessClientMessageReceived;
      }
    }
    catch (Exception ex)
    {
      CommunicationsRuntime.Current.RuntimeLog.Error(
        tag,
        "Failed to find available network interfaces!",
        ex
      );
    }
  }

  private void ProcessClientMessageReceived(object? sender, UdpReceiveResult result)
  {
    var tag = $"{GetType().Name}::{nameof(ProcessClientMessageReceived)}";

    // Recently received?
    if (IgnoreDuplicateMessages && !_receivedMessageIds.TryAdd(result.Buffer))
    {
      // Yep!
      return;
    }

    var message = new Message();
    try
    {
      message.Read(result.Buffer, 0, result.Buffer.Length);
    }
    catch (Exception)
    {
      CommunicationsRuntime.Current.RuntimeLog.Warning(
        tag,
        $"Malformed DNS message was received by {result.RemoteEndPoint.Address}!"
      );
      MalformedMessageReceived?.Invoke(this, result.Buffer);
      return;
    }

    if (message.Opcode != MessageOperation.Query)
    {
      return;
    }
    if (message.Status != MessageStatus.NoError)
    {
      CommunicationsRuntime.Current.RuntimeLog.Info(
        tag,
        $"Received DNS error message by {result.RemoteEndPoint.Address}."
      );
      return;
    }

    // Handle it
    try
    {
      var multicastMessageEventArgs = new MulticastMessageEventArgs()
      {
        Message = message,
        RemoteEndPoint = result.RemoteEndPoint,
      };
      if (message.IsQuery && message.Questions.Count > 0)
      {
        QueryReceived?.Invoke(this, multicastMessageEventArgs);
      }
      else if (message.IsResponse && message.Answers.Count > 0)
      {
        AnswerReceived?.Invoke(this, multicastMessageEventArgs);
      }
    }
    catch (Exception ex)
    {
      CommunicationsRuntime.Current.RuntimeLog.Error(
        tag,
        $"Failed to handle DNS message received by {result.RemoteEndPoint.Address}!",
        ex
      );
    }
  }
}
