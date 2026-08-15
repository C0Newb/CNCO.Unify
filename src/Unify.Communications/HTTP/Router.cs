using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;
using CNCO.Unify.Communications.Http.Routing;
using CNCO.Unify.Communications.Http.Routing.ControllerInvoker;

namespace CNCO.Unify.Communications.Http;

/// <summary>
/// Routes incoming requests.
/// </summary>
public class Router : IRouter
{
  private bool _log = false;

  public Router(bool initializeRoutes = true)
  {
    if (initializeRoutes)
    {
      InitializeRoutes();
    }
  }

  public void SetLogging(bool enabled) => _log = enabled;

  #region Listeners
  private sealed class Listener
  {
    public HttpVerb Verb { get; private set; }
    public string Path { get; private set; }
    public string PathRegex { get; private set; }
    public Action<IWebRequest, IWebResponse>? OnWebRequest { get; private set; }
    public Action<IWebSocket>? OnWebSocketRequest { get; private set; }

    public bool IsWebSocket { get; set; } = false;

    private Listener(HttpVerb verb, string path)
    {
      Verb = verb;
      Path = '/' + path.Trim('/'); // force only one '/' at the start :)
      PathRegex = Regex.Replace(Path, @":.*?:|\{.*?\}", ".*");
    }

    public Listener(HttpVerb verb, string path, Action<IWebRequest, IWebResponse> onWebRequest)
      : this(verb, path) => OnWebRequest = onWebRequest;

    public Listener(string path, Action<IWebSocket> onWebSocketRequest)
      : this(HttpVerb.Any, path)
    {
      IsWebSocket = true;
      OnWebSocketRequest = onWebSocketRequest;
    }

    public override bool Equals(object? obj)
    {
      if (obj is Listener listener)
      {
        return listener.Path == Path && listener.PathRegex == PathRegex && listener.Verb == Verb;
      }
      return false;
    }

    public override int GetHashCode() =>
      HashCode.Combine(Path ?? string.Empty, PathRegex ?? string.Empty, Verb);
  }

  private readonly Dictionary<string, List<Listener>> Listeners = [];

  // Use this to go from paths to paths with parameter names (/my/path/to/blahBlah/doc -> /my/path/to/:docPath:/doc)
  private readonly Dictionary<string, string> PathRegexLookup = [];
  #endregion

  #region Listeners methods
  private void AddListener(Listener listener)
  {
    List<Listener>? currentListeners = null;
    if (Listeners.TryGetValue(listener.Path, out List<Listener>? value))
      currentListeners = value;

    currentListeners ??= new List<Listener>(1);
    currentListeners.Add(listener);
    Listeners[listener.Path] = currentListeners;

    if (
      (listener.Path != listener.PathRegex || listener.Path.EndsWith('*'))
      && !PathRegexLookup.ContainsKey(listener.PathRegex)
    )
    {
      PathRegexLookup.Add(listener.PathRegex, listener.Path);
    }

    CommunicationsRuntime.Current.RuntimeLog.Verbose(
      $"{GetType().Name}::{nameof(AddListener)}",
      $"Added new {listener.Verb} route {listener.Path}"
    );
  }

  public void Any(string path, Action<IWebRequest, IWebResponse> callback) =>
    AddListener(new Listener(HttpVerb.Any, path, callback));

  public void Connect(string path, Action<IWebRequest, IWebResponse> callback) =>
    AddListener(new Listener(HttpVerb.Connect, path, callback));

  public void Delete(string path, Action<IWebRequest, IWebResponse> callback) =>
    AddListener(new Listener(HttpVerb.Delete, path, callback));

  public void Get(string path, Action<IWebRequest, IWebResponse> callback) =>
    AddListener(new Listener(HttpVerb.Get, path, callback));

  public void Head(string path, Action<IWebRequest, IWebResponse> callback) =>
    AddListener(new Listener(HttpVerb.Head, path, callback));

  public void Options(string path, Action<IWebRequest, IWebResponse> callback) =>
    AddListener(new Listener(HttpVerb.Options, path, callback));

  public void Patch(string path, Action<IWebRequest, IWebResponse> callback) =>
    AddListener(new Listener(HttpVerb.Patch, path, callback));

  public void Post(string path, Action<IWebRequest, IWebResponse> callback) =>
    AddListener(new Listener(HttpVerb.Post, path, callback));

  public void Put(string path, Action<IWebRequest, IWebResponse> callback) =>
    AddListener(new Listener(HttpVerb.Put, path, callback));

  public void Trace(string path, Action<IWebRequest, IWebResponse> callback) =>
    AddListener(new Listener(HttpVerb.Trace, path, callback));

  public void WebSocket(string path, Action<IWebSocket> callback) =>
    AddListener(new Listener(path, callback));

  public void Remove(string path, Action<IWebRequest, IWebResponse>? callback, HttpVerb? httpVerb)
  {
    if (callback == null || !Listeners.TryGetValue(path, out List<Listener>? listeners))
    {
      Listeners.Remove(path);
      return;
    }

    foreach (Listener listener in listeners)
    {
      if (listener.OnWebRequest == callback && (httpVerb == null || listener.Verb == httpVerb))
        listeners.Remove(listener);
    }
    Listeners[path] = listeners;
  }
  #endregion

  /// <summary>
  /// Process an incoming request.
  /// </summary>
  /// <param name="request">Incoming request.</param>
  /// <param name="response">Incoming request's response.</param>
  public void Process(IWebRequest request, IWebResponse response)
  {
    if (string.IsNullOrEmpty(request.Path))
      return;

    string requestPath = '/' + request.Path.Trim('/');
    Listeners.TryGetValue(requestPath, out List<Listener>? listenersForPath);

    // Find the listeners via regex?
    if (listenersForPath == null || listenersForPath?.Count == 0)
    {
      // yep, try to find any via regex
      foreach (var regexString in PathRegexLookup.Keys)
      {
        Regex regex = new Regex(regexString);
        var match = regex.Match(request.Path);
        if (
          !match.Success
          || // does not match
          match.Captures[0].ToString().Split('/').Length != regexString.Split('/').Length // parameter count mismatch
        )
        {
          continue;
        }

        // yep
        var originalPath = PathRegexLookup[regexString];
        _ = Listeners.TryGetValue(originalPath, out List<Listener>? listenersForRegexPath);
        if (listenersForRegexPath != null && listenersForRegexPath.Count > 0)
        {
          listenersForPath = listenersForRegexPath;
          request.RouteTemplate = new RouteTemplate(originalPath, request.Path);

          break;
        }
      }
    }

    if (listenersForPath == null || listenersForPath?.Count == 0)
    {
      response.Status(404);
      response.End();
      if (_log)
        CommunicationsRuntime.Current.RuntimeLog.Warning(
          $"{GetType().Name}::{nameof(Process)}",
          $"404: no listener found for path {request.Path}!"
        );
      return;
    }

    bool listenerFired = false;

    // List of tasks
    ConcurrentDictionary<Listener, Task> listenerTasks = new ConcurrentDictionary<Listener, Task>();

    // Activate listeners!
    bool hasActivatedWebSocket = false; // So we can throw a "hey, don't have HTTP listeners on the same route has WebSockets!" warning!
    bool hasActivatedHttp = false;
    bool hasActivatedWebSocketWarningEmitted = false; // ... but only once
    void WarnAboutMixingHttpAndWebSocketRoutes()
    {
      if (hasActivatedWebSocketWarningEmitted)
        return;
      hasActivatedWebSocketWarningEmitted = true;
      CommunicationsRuntime.Current.RuntimeLog.Warning(
        $"{GetType().Name}::{nameof(Process)}",
        "You have a HTTP listener on the same route as a WebSocket handler! You cannot do this as once the HTTP response closes, the WebSocket will close."
          + Environment.NewLine
          + "You have been warned."
      );
    }
    foreach (Listener listener in listenersForPath!)
    {
      try
      {
        if (response.HasEnded)
        {
          CommunicationsRuntime.Current.RuntimeLog.Verbose(
            $"{GetType().Name}::{nameof(Process)}",
            $"Error encountered calling listener for {listener.Verb} \"{listener.Path}\" as the response has ended."
          );
          continue;
        }

        // Is a WebSocket listener .. and the request is a WebSocket handshake?
        if (listener.IsWebSocket)
        {
          // up here so that we return a 400 even if it's not a valid WebSocket request
          hasActivatedWebSocket = true;

          if (!HasValidWebSocketConnectHandshakeHeaders(request))
          {
            continue;
          }

          if (listener.OnWebSocketRequest == null) // What? How?
            throw new InvalidOperationException(
              $"{nameof(listener.OnWebSocketRequest)} is null, no listener action to call!"
            );

          if (hasActivatedHttp && !hasActivatedWebSocketWarningEmitted)
            WarnAboutMixingHttpAndWebSocketRoutes();

          Task wsTask = new Task(async () =>
            listener.OnWebSocketRequest(await request.CreateWebSocketConnectionAsync())
          );
          listenerTasks.TryAdd(listener, wsTask);
          wsTask.Start();
          listenerFired = true;
        }
        else if (listener.Verb == HttpVerb.Any || listener.Verb == request.Verb)
        {
          if (listener.OnWebRequest == null) // What? How?
            throw new InvalidOperationException(
              $"{nameof(listener.OnWebRequest)} is null, no listener action to call!"
            );

          if (hasActivatedWebSocket && !hasActivatedWebSocketWarningEmitted)
            WarnAboutMixingHttpAndWebSocketRoutes();

          // same path, same verb, send er...
          Task httpTask = new Task(() => WrapListenerInvoke(listener, request, response));
          _ = listenerTasks.TryAdd(listener, httpTask);
          httpTask.Start();
          listenerFired = true;
        }
      }
      catch (Exception ex)
      {
        response.Status(500);
        CommunicationsRuntime.Current.RuntimeLog.Error(
          $"{GetType().Name}::{nameof(Process)}",
          $"Error encountered calling listener for {listener.Verb} \"{listener.Path}\"",
          ex
        );
      }
    }

    // Wait for a response ...
    foreach (Listener listener in listenersForPath)
    {
      try
      {
        if (listener.IsWebSocket || listener.Verb == HttpVerb.Any || listener.Verb == request.Verb)
        {
          Task task = listenerTasks[listener];
          task.Wait(
            CommunicationsRuntime.Current.Configuration.Http.Router.ResponseTimeoutMilliseconds
          ); // it should be cancelling, but ...

          if (task.IsFaulted && task.Exception != null)
          {
            throw task.Exception;
          }

          try
          {
            task?.Dispose();
          }
          catch
          {
            CommunicationsRuntime.Current.RuntimeLog.Warning(
              $"{GetType().Name}::{nameof(Process)}",
              $"Listener task hang for {request.Path}!"
            );
          }
        }
      }
      catch (OperationCanceledException)
      {
        // Ignore
      }
      catch (AggregateException e)
      {
        if (e.InnerException?.GetType() == typeof(ListenerOnWebRequestException))
        {
          throw e.InnerException;
        }
        if (
          e.Message.Contains("websocket request without", StringComparison.OrdinalIgnoreCase)
          && e.Message.Contains("header", StringComparison.OrdinalIgnoreCase)
        )
        {
          hasActivatedWebSocket = true;
          listenerFired = false; // forces a 400 later.
          return;
        }

        // no idea, re-throw
        throw;
      }
    }

    listenerTasks.Clear();

    if (!listenerFired)
    {
      if (hasActivatedWebSocket)
        response.Status(400);
      else
        response.Status(404);
      response.End();

      if (_log)
      {
        if (hasActivatedWebSocket)
          CommunicationsRuntime.Current.RuntimeLog.Warning(
            $"{GetType().Name}::{nameof(Process)}",
            $"400: invalid WebSocket connection handshake request to {request.Path}!"
          );
        else
          CommunicationsRuntime.Current.RuntimeLog.Warning(
            $"{GetType().Name}::{nameof(Process)}",
            $"404: no listener found for path {request.Path}!"
          );
      }
      return;
    }
    else if (!response.HasEnded)
    {
      if (request.WebSocket != null && request.WebSocket.IsOpen)
      {
        // You cannot close the response stream, that would terminate the WebSocket connection!
        return;
      }

      if (CommunicationsRuntime.Current.Configuration.Http.Router.EnableDefaultResponses)
      {
        response.Status(
          CommunicationsRuntime.Current.Configuration.Http.Router.DefaultResponseStatusCode ?? 500
        );
        string? body = CommunicationsRuntime.Current.Configuration.Http.Router.DefaultResponseBody;
        if (!string.IsNullOrEmpty(body))
        {
          response.Send(body);
        }
      }
      else
      {
        response.Status(500);
      }

      response.End();

      if (_log)
        CommunicationsRuntime.Current.RuntimeLog.Warning(
          $"{GetType().Name}::{nameof(Process)}",
          $"500: {listenersForPath?.Count ?? 0} listener(s) found for path {request.Path}, but none responded!"
        );
    }
  }

  // Checks whether the request contains the headers required for a WebSocket connection handshake
  private static bool HasValidWebSocketConnectHandshakeHeaders(IWebRequest request) =>
    !(
      string.IsNullOrEmpty(request.Headers["Connection"])
      || string.IsNullOrEmpty(request.Headers["Upgrade"])
      || string.IsNullOrEmpty(request.Headers["Sec-WebSocket-Version"])
      || string.IsNullOrEmpty(request.Headers["Sec-WebSocket-Key"])
      || string.IsNullOrEmpty(request.Headers["Sec-WebSocket-Extensions"])
      || !request.Headers["Connection"]!.Equals("upgrade", StringComparison.OrdinalIgnoreCase)
      || !request.Headers["Upgrade"]!.Equals("websocket", StringComparison.OrdinalIgnoreCase)
    );

  private static void WrapListenerInvoke(
    Listener listener,
    IWebRequest request,
    IWebResponse response
  )
  {
    try
    {
      if (listener.OnWebRequest == null)
      {
        throw new InvalidOperationException(
          "Attempt to invoke web request for web socket listener!"
        );
      }
      listener.OnWebRequest(request, response);
    }
    catch (Exception ex)
    {
      CommunicationsRuntime.Current.RuntimeLog.Error(
        $"{nameof(Router)}::{nameof(WrapListenerInvoke)}",
        $"Error invoking listener for {listener.Verb} \"{listener.Path}\"",
        ex
      );
      throw new ListenerOnWebRequestException(ex);
    }
  }

  /// <summary>
  /// Adds all routes in controllers to this router.
  /// </summary>
  public void InitializeRoutes() => RouteControllerInitializer.Initialize(this);
}
