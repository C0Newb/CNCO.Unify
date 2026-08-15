using System.Diagnostics;
using System.Net;
using System.Security;
using CNCO.Unify.Communications.Http.Routing;

namespace CNCO.Unify.Communications.Http;

public class WebServer : IWebServer
{
  private readonly HttpListener _httpListener;
  private readonly Thread _listenerThread;
  private int _listenerThreadRestart = 0;
  private bool _runListenerThread = true;
  private IRouter? _router;
  private IEnumerable<string>? _endpoints;

  private IRouter Router
  {
    get
    {
      _router ??= new Router();
      return _router;
    }
  }

  private bool _logAccess = false;

  public WebServer()
  {
    _httpListener = new HttpListener();

    _listenerThread = new Thread(RunListenerRoutine)
    {
      Name = UnifyRuntime.Current.ApplicationId + "-WebServer#" + base.GetHashCode(),
    };
  }

  public WebServer(WebServerOptions options)
    : this() => SetOptions(options);

  public WebServer(IRouter router)
    : this() => _router = router;

  public WebServer(IRouter router, WebServerOptions options)
    : this(router) => SetOptions(options);

  public void AddEndpoint(string endpoint)
  {
    if (!endpoint.StartsWith("http://") && !endpoint.StartsWith("https://"))
    {
      endpoint = "http://" + endpoint;
    }
    if (!endpoint.EndsWith('/'))
      endpoint += "/";

    _httpListener.Prefixes.Add(endpoint);
  }

  public void AddEndpoint(Uri uri)
  {
    _httpListener.Prefixes.Add(uri.ToString());
  }

  public void Abort()
  {
    _runListenerThread = false;
    _httpListener.Abort();
  }

  protected virtual void Dispose(bool disposing)
  {
    string tag = $"{GetType().Name}::{nameof(Dispose)}";
    try
    {
      _runListenerThread = false;
      try
      {
        _httpListener.Stop();
        _httpListener.Close();
      }
      catch
      {
        // Swallow
      }
    }
    catch (Exception e)
    {
      CommunicationsRuntime.Current.RuntimeLog.Error(tag, $"Error while disposing!", e);
    }
  }

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  public IEnumerable<string> GetEndpoints() => _endpoints ?? _httpListener.Prefixes;

  public void Start()
  {
    try
    {
      _runListenerThread = true;
      _endpoints = _httpListener.Prefixes;
      _httpListener.Start();
      _listenerThread.Start();
    }
    catch (HttpListenerException ex)
    {
      if (ex.Message.Equals("Access is denied.", StringComparison.OrdinalIgnoreCase))
      {
        throw new SecurityException();
      }
      throw;
    }
  }

  public void Stop()
  {
    _runListenerThread = false;
    _httpListener.Stop();
  }

  public bool Running()
  {
    return _runListenerThread && _httpListener.IsListening;
  }

  public void Use(IRouter router) => _router = router;

  #region Router method proxies
  public void Any(string path, Action<IWebRequest, IWebResponse> callback) =>
    Router.Any(path, callback);

  public void Connect(string path, Action<IWebRequest, IWebResponse> callback) =>
    Router.Connect(path, callback);

  public void Delete(string path, Action<IWebRequest, IWebResponse> callback) =>
    Router.Delete(path, callback);

  public void Get(string path, Action<IWebRequest, IWebResponse> callback) =>
    Router.Get(path, callback);

  public void Head(string path, Action<IWebRequest, IWebResponse> callback) =>
    Router.Head(path, callback);

  public void Options(string path, Action<IWebRequest, IWebResponse> callback) =>
    Router.Options(path, callback);

  public void Patch(string path, Action<IWebRequest, IWebResponse> callback) =>
    Router.Patch(path, callback);

  public void Post(string path, Action<IWebRequest, IWebResponse> callback) =>
    Router.Post(path, callback);

  public void Put(string path, Action<IWebRequest, IWebResponse> callback) =>
    Router.Put(path, callback);

  public void Trace(string path, Action<IWebRequest, IWebResponse> callback) =>
    Router.Trace(path, callback);
  #endregion


  private void SetOptions(WebServerOptions options)
  {
    if (options.Endpoints?.Length > 0)
    {
      foreach (var endpoint in options.Endpoints)
      {
        AddEndpoint(endpoint);
      }
    }
    else if (options.Port.HasValue)
    {
      // Bind to all available interfaces
      string protocol = options.UseHttps ? "https" : "http";
      var localAddresses = NetworkInterfaces.GetSystemIPAddresses(true, options.BindToIPv6);

      foreach (var address in localAddresses)
      {
        AddEndpoint($"{protocol}://{address}:{options.Port}");
      }

      AddEndpoint($"{protocol}://localhost:{options.Port}");
    }

    _logAccess = options.LogAccess;
    Router?.SetLogging(_logAccess);
  }

  private void RunListenerRoutine()
  {
    string tag = $"{GetType().Name}::${nameof(_listenerThread)}";

    if (_httpListener.Prefixes.Count <= 0)
    {
      CommunicationsRuntime.Current.RuntimeLog.Warning(
        tag,
        "Failed to start webserver as there are no bound endpoints!"
      );
    }

    while (_httpListener.IsListening && _runListenerThread)
    {
      try
      {
        // listen
        var context = _httpListener.GetContext();
        _ = Task.Run(() => HandleRequest(context));
      }
      catch (Exception ex)
      {
        if (!_runListenerThread)
        {
          CommunicationsRuntime.Current.RuntimeLog.Info(tag, "Listener thread stopped.");
          return;
        }

        CommunicationsRuntime.Current.RuntimeLog.Alert(
          tag,
          $"HTTP webserver listener thread exception: {ex.Message}"
        );

        CommunicationsRuntime.Current.RuntimeLog.Error(tag, ex);

        // restart the thread
        _listenerThreadRestart++;
        if (_listenerThreadRestart > 5)
        {
          CommunicationsRuntime.Current.RuntimeLog.Emergency(
            $"Listener thread has restarted too many times ({_listenerThreadRestart}). HTTP listener is disabled until the application restarts."
          ); // sorta a lie.. but eh
          break;
        }

        Thread.Sleep(1000 * _listenerThreadRestart);
        CommunicationsRuntime.Current.RuntimeLog.Warning(
          $"Attempting listener thread restart #{_listenerThreadRestart}"
        ); // sorta a lie.. but eh
      }
    }
  }

  private void HandleRequest(HttpListenerContext context)
  {
    string? tag = null;

    if (Router == null)
    {
      return;
    }

    try
    {
      AddDefaultResponseHeaders(context);

      WebRequest request = new WebRequest(context);
      WebResponse response = new WebResponse(context.Response);

      if (_logAccess)
      {
        tag ??= $"{GetType().Name}::{nameof(HandleRequest)}";
        CommunicationsRuntime.Current.RuntimeLog.Debug(tag, $"HTTP-{request.Verb} {request.Path}");
      }

      Router.Process(request, response);
    }
    catch (Exception e)
    {
      string path = "UNKNOWN";
      try
      {
        path = context.Request.Url?.AbsolutePath ?? "NULL";
        if (context.Response != null)
        {
          context.Response.StatusCode = 500;
          context.Response.Close();
        }
      }
      catch
      {
        // Ignore path resolve issues
      }

      // Bubble up?
      if (e is ListenerOnWebRequestException listenerOnWebRequestException)
      {
        Debugger.BreakForUserUnhandledException(listenerOnWebRequestException.ListenerException);
        throw listenerOnWebRequestException.ListenerException;
      }

      tag ??= $"{GetType().Name}::{nameof(HandleRequest)}";
      CommunicationsRuntime.Current.RuntimeLog.Error(
        tag,
        $"Failed to process HTTP request {path}",
        e
      );
    }
  }

  private static void AddDefaultResponseHeaders(HttpListenerContext context)
  {
    string tag = $"{typeof(WebServer).Name}::{nameof(AddDefaultResponseHeaders)}";
    try
    {
      var defaultHeaders = CommunicationsRuntime
        .Current
        .Configuration
        .Http
        .DefaultWebServerResponseHeaders;
      if (defaultHeaders != null)
      {
        foreach (var header in defaultHeaders.AllKeys)
        {
          if (defaultHeaders[header] == null)
            continue;

          context.Response.Headers[header] = defaultHeaders[header];
        }
      }
    }
    catch (Exception ex)
    {
      // Failed to apply default headers
      CommunicationsRuntime.Current.RuntimeLog.Warning(
        tag,
        "Failed to apply default headers. Error: " + ex.Message + "Stack: " + ex.StackTrace
      );
    }
  }
}
