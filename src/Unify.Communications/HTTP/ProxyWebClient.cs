namespace CNCO.Unify.Communications.Http {
    /// <summary>
    /// Allows you to set the base Uri for a <see cref="WebClient"/>.
    /// This is NOT to configure a proxy in the sense you might be thinking of.
    /// </summary>
    public class ProxyWebClient : WebClient, IWebClient {
        public Uri BaseUri;

        public ProxyWebClient(Uri baseUri) : base() {
            BaseUri = baseUri;
        }

        private Uri CombineUri(Uri uri) {
            return new Uri(BaseUri, uri);
        }

        private Uri CombineUri(string uri) {
            return new Uri(BaseUri, uri);
        }

        public new Task<HttpResponseMessage> ConnectAsync(string uri) => ConnectAsync(CombineUri(uri));
        public new Task<HttpResponseMessage> ConnectAsync(Uri uri) => base.ConnectAsync(CombineUri(uri));
        public new Task<HttpResponseMessage> DeleteAsync(string uri) => DeleteAsync(CombineUri(uri));
        public new Task<HttpResponseMessage> DeleteAsync(Uri uri) => base.DeleteAsync(CombineUri(uri));
        public new Task<HttpResponseMessage> GetAsync(string uri) => GetAsync(CombineUri(uri));
        public new Task<HttpResponseMessage> GetAsync(Uri uri) => base.GetAsync(CombineUri(uri));
        public new Task<string> GetStringAsync(string uri) => GetStringAsync(CombineUri(uri));
        public new Task<string> GetStringAsync(Uri uri) => base.GetStringAsync(CombineUri(uri));
        public new Task<HttpResponseMessage> HeadAsync(string uri) => HeadAsync(CombineUri(uri));
        public new Task<HttpResponseMessage> HeadAsync(Uri uri) => base.HeadAsync(CombineUri(uri));
        public new Task<HttpResponseMessage> OptionsAsync(string uri) => OptionsAsync(CombineUri(uri));
        public new Task<HttpResponseMessage> OptionsAsync(Uri uri) => base.OptionsAsync(CombineUri(uri));
        public new Task<HttpResponseMessage> PatchAsync(string uri, HttpContent content) => PatchAsync(CombineUri(uri), content);
        public new Task<HttpResponseMessage> PatchAsync(Uri uri, HttpContent content) => base.PatchAsync(CombineUri(uri), content);
        public new Task<HttpResponseMessage> PostAsync(string uri, HttpContent content) => PostAsync(CombineUri(uri), content);
        public new Task<HttpResponseMessage> PostAsync(Uri uri, HttpContent content) => base.PostAsync(CombineUri(uri), content);
        public new Task<HttpResponseMessage> PutAsync(string uri, HttpContent content) => PutAsync(CombineUri(uri), content);
        public new Task<HttpResponseMessage> PutAsync(Uri uri, HttpContent content) => base.PutAsync(CombineUri(uri), content);
        public new Task<HttpResponseMessage> TraceAsync(string uri) => TraceAsync(CombineUri(uri));
        public new Task<HttpResponseMessage> TraceAsync(Uri uri) => base.TraceAsync(CombineUri(uri));
    }
}
