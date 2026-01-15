namespace CNCO.Unify.Communications.Http;

/// <summary>
/// <see cref="WebServer"/> settings.
/// </summary>
public record WebServerOptions
{
  /// <summary>
  /// The base URL where the server will listen.
  /// </summary>
  public string[]? Endpoints { get; init; }

  /// <summary>
  /// Port to listen on. Will bind on all available network interfaces.
  /// </summary>
  public int? Port { get; init; } = 8080;

  public bool BindToIPv6 { get; init; } = false;

  /// <summary>
  /// Log accesses, not only errors (500).
  /// </summary>
  /// <remarks>
  /// I highly recommend this being off until the file logger can improve.
  /// Since every log entry is instantly flushed to disk, this murders performance.
  /// ~900rq/s on Windows with logging on, ~90k with it off.
  /// </remarks>
  public bool LogAccess { get; init; } = false;

  // HTTPS settings
  public bool UseHttps { get; init; } = false;
  public string? CertificatePath { get; init; }
  public string? CertificatePassword { get; init; }
}
