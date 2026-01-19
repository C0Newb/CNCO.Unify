namespace CNCO.Unify.Communications.Http;

/// <summary>
/// Hostable HTTP(s) server
/// </summary>
public interface IWebServer : IDisposable
{
  /// <summary>
  /// Adds an endpoint to be listening on. Can only be done if the server is stopped.
  /// </summary>
  /// <param name="endpoint">Address to be listening on, such as <c>http://localhost:8008</c>.</param>
  void AddEndpoint(string endpoint);

  /// <summary>
  /// Adds an endpoint to be listening on. Can only be done if the server is stopped.
  /// </summary>
  /// <param name="uri">The URI to listen on.</param>
  void AddEndpoint(Uri uri);

  /// <summary>
  /// Returns all endpoints the server is listening to.
  /// </summary>
  /// <returns>Current listened to endpoints.</returns>
  IEnumerable<string> GetEndpoints();

  /// <summary>
  /// Starts the web server.
  /// </summary>
  /// <exception cref="System.Security.SecurityException">
  /// Access is denied binding to an endpoint.
  /// </exception>
  void Start();

  /// <summary>
  /// Stops the web server.
  /// </summary>
  void Stop();

  /// <summary>
  /// Aborts the web server.
  /// </summary>
  void Abort();

  /// <summary>
  /// Checks if the web server is running.
  /// </summary>
  /// <returns><see langword="true"/> if the web server is running; otherwise, <see langword="false"/>.</returns>
  bool Running();

  /// <summary>
  /// Sets the router for handling incoming requests.
  /// </summary>
  /// <param name="router">The router to be used for handling requests.</param>
  void Use(IRouter router);
}

/// <summary>
/// Protocols used by the webserver.
/// </summary>
public enum Protocol
{
  HTTP,
  HTTPS,
}
