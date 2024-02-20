using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace CNCO.Unify.Communications.Http {
    /// <summary>
    /// Routes incoming requests.
    /// </summary>
    public class Router : IRouter {
        private class Listener {
            public HttpVerb Verb;
            public string Path;
            public string PathRegex;
            public Action<WebRequest, WebResponse> OnWebRequest;

            public Listener(HttpVerb verb, string path, Action<WebRequest, WebResponse> onWebRequest) {
                Verb = verb;
                Path = path;
                PathRegex = Regex.Replace(path, @":.*:", ".*");
                OnWebRequest = onWebRequest;
            }
        }

        private readonly Dictionary<string, List<Listener>> Listeners = new Dictionary<string, List<Listener>>();

        // Use this to go from paths to paths with parameter names (/my/path/to/blahBlah/doc -> /my/path/to/:docPath:/doc)
        private readonly Dictionary<string, string> PathRegexLookup = new Dictionary<string, string>();


        private bool _log = true;

        public Router() { }

        public void SetLogging(bool enabled) => _log = enabled;

        private void AddListener(Listener listener) {
            List<Listener>? currentListeners = null;
            if (!Listeners.TryGetValue(listener.Path, out List<Listener>? value))
                currentListeners = value;

            currentListeners ??= new List<Listener>(1);
            currentListeners.Add(listener);
            Listeners[listener.Path] = currentListeners;

            if (listener.Path != listener.PathRegex)
                PathRegexLookup.Add(listener.PathRegex, listener.Path);
        }

        public void All(string path, Action<WebRequest, WebResponse> callback) => AddListener(new Listener(HttpVerb.All, path, callback));
        public void Connect(string path, Action<WebRequest, WebResponse> callback) => AddListener(new Listener(HttpVerb.Connect, path, callback));
        public void Delete(string path, Action<WebRequest, WebResponse> callback) => AddListener(new Listener(HttpVerb.Delete, path, callback));
        public void Get(string path, Action<WebRequest, WebResponse> callback) => AddListener(new Listener(HttpVerb.Get, path, callback));
        public void Head(string path, Action<WebRequest, WebResponse> callback) => AddListener(new Listener(HttpVerb.Head, path, callback));
        public void Options(string path, Action<WebRequest, WebResponse> callback) => AddListener(new Listener(HttpVerb.Options, path, callback));
        public void Patch(string path, Action<WebRequest, WebResponse> callback) => AddListener(new Listener(HttpVerb.Patch, path, callback));
        public void Post(string path, Action<WebRequest, WebResponse> callback) => AddListener(new Listener(HttpVerb.Post, path, callback));
        public void Put(string path, Action<WebRequest, WebResponse> callback) => AddListener(new Listener(HttpVerb.Put, path, callback));
        public void Trace(string path, Action<WebRequest, WebResponse> callback) => AddListener(new Listener(HttpVerb.Trace, path, callback));




        public void Remove(string path, Action<WebRequest, WebResponse>? callback, HttpVerb? httpVerb) {
            if (callback == null || !Listeners.TryGetValue(path, out List<Listener>? listeners)) {
                Listeners.Remove(path);
                return;
            }

            foreach (Listener listener in listeners) {
                if (listener.OnWebRequest == callback && (httpVerb == null || listener.Verb == httpVerb))
                    listeners.Remove(listener);
            }
            Listeners[path] = listeners;
        }

        private NameValueCollection ExtractPathParameters(string requestPath, string originalPath) {
            NameValueCollection parameters = new NameValueCollection();

            string[] requestPathParts = requestPath.Split('/');
            string[] originalPathParts = originalPath.Split('/');

            if (requestPathParts.Length != originalPathParts.Length)
                return parameters; // Return empty dictionary if lengths don't match

            for (int i = 0; i < originalPathParts.Length; i++) {
                var part = originalPathParts[i];
                if (!part.StartsWith(":") || !part.EndsWith(":"))
                    continue;

                // path parameter
                parameters.Add(part[1..^1], requestPathParts[i]); // Get the parameter name and value
            }

            return parameters;
        }


        public void Process(WebRequest request, WebResponse response) {
            string tag = $"{GetType().Name}::{nameof(Process)}";

            if (string.IsNullOrEmpty(request.Path))
                return;

            Listeners.TryGetValue(request.Path, out List<Listener>? listenersForPath);

            if (listenersForPath == null || listenersForPath?.Count == 0) {
                // try to find it via regex
                foreach (var regexString in PathRegexLookup.Keys) {
                    Regex regex = new Regex(regexString);
                    if (!regex.IsMatch(request.Path))
                        continue;

                    // yep
                    var originalPath = PathRegexLookup[regexString];
                    Listeners.TryGetValue(originalPath, out List<Listener>? listenersForRegexPath);
                    if (listenersForRegexPath != null && listenersForRegexPath.Count > 0) {
                        listenersForPath = listenersForRegexPath;

                        request.Parameters = ExtractPathParameters(request.Path, originalPath);

                        break;
                    }
                }
            }

            if (listenersForPath == null || listenersForPath?.Count == 0) {
                response.Status(404);
                response.End();
                if (_log)
                    CommunicationsRuntime.Log.Warning($"404: no listener found for path {request.Path}!");
                return;
            }

            bool listenerFired = false;

            foreach (Listener listener in listenersForPath ?? new List<Listener>()) {
                if (listener.Verb == HttpVerb.All || listener.Verb == request.Verb) {
                    listenerFired = true;
                    // same path, same verb, send er...
                    listener.OnWebRequest(request, response);
                }
            }

            if (!listenerFired) {
                response.Status(404);
                response.End();
                if (_log)
                    CommunicationsRuntime.Log.Warning($"404: no listener found for path {request.Path}!");
                return;
            }
        }
    }
}
