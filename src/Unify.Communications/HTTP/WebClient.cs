namespace CNCO.Unify.Communications.Http {
    /// <summary>
    /// Used to send HTTP requests to a remote server.
    /// </summary>
    public class WebClient : IWebClient {

        private HttpClient HttpClient { get; set; }


        /// <summary>
        /// How long we'll wait for a response before timing out.
        /// </summary>
        public TimeSpan Timeout {
            get => HttpClient.Timeout;
            set => HttpClient.Timeout = value;
        }



        public WebClient() {
            HttpClient = new HttpClient();
            HttpClient.Timeout = new TimeSpan(0, 0, 30);
        }


        /// <summary>
        /// Sets a header in our requests.
        /// </summary>
        /// <param name="name">Name of the header.</param>
        /// <param name="value">Value to set the header to.</param>
        public void SetHeader(string name, string value) {
            if (HttpClient.DefaultRequestHeaders.Contains(name))
                HttpClient.DefaultRequestHeaders.Remove(name);
            HttpClient.DefaultRequestHeaders.Add(name, value);
        }

        /// <summary>
        /// Gets the value of a header in our requests.
        /// </summary>
        /// <param name="name">Name of the header.</param>
        /// <returns>Value the header is set to.</returns>
        public string? GetHeader(string name) {
            if (HttpClient.DefaultRequestHeaders.Contains(name))
                return HttpClient.DefaultRequestHeaders.GetValues(name).FirstOrDefault();
            return null;
        }


        public void Dispose() {
            HttpClient.CancelPendingRequests();
            HttpClient.Dispose();
            GC.SuppressFinalize(this);
        }




        public Task<HttpResponseMessage> ConnectAsync(string uri) {
            return HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Connect, uri));
        }
        public Task<HttpResponseMessage> ConnectAsync(Uri uri) {
            return HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Connect, uri));
        }


        public Task<HttpResponseMessage> DeleteAsync(string uri) {
            return HttpClient.DeleteAsync(uri);
        }

        public Task<HttpResponseMessage> DeleteAsync(Uri uri) {
            return HttpClient.DeleteAsync(uri);
        }


        public Task<HttpResponseMessage> GetAsync(string uri) {
            return HttpClient.GetAsync(uri);
        }
        public Task<HttpResponseMessage> GetAsync(Uri uri) {
            return HttpClient.GetAsync(uri);
        }
        public Task<string> GetStringAsync(string uri) {
            return HttpClient.GetStringAsync(uri);
        }
        public Task<string> GetStringAsync(Uri uri) {
            return HttpClient.GetStringAsync(uri);
        }


        public Task<HttpResponseMessage> HeadAsync(string uri) {
            return HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, uri));
        }
        public Task<HttpResponseMessage> HeadAsync(Uri uri) {
            return HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, uri));
        }


        public Task<HttpResponseMessage> OptionsAsync(string uri) {
            return HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Options, uri));
        }
        public Task<HttpResponseMessage> OptionsAsync(Uri uri) {
            return HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Options, uri));
        }


        public Task<HttpResponseMessage> PatchAsync(string uri, HttpContent content) {
            return HttpClient.PatchAsync(uri, content);
        }
        public Task<HttpResponseMessage> PatchAsync(Uri uri, HttpContent content) {
            return HttpClient.PatchAsync(uri, content);
        }


        public Task<HttpResponseMessage> PostAsync(string uri, HttpContent content) {
            return HttpClient.PostAsync(uri, content);
        }
        public Task<HttpResponseMessage> PostAsync(Uri uri, HttpContent content) {
            return HttpClient.PostAsync(uri, content);
        }


        public Task<HttpResponseMessage> PutAsync(string uri, HttpContent content) {
            return HttpClient.PutAsync(uri, content);
        }
        public Task<HttpResponseMessage> PutAsync(Uri uri, HttpContent content) {
            return HttpClient.PutAsync(uri, content);
        }


        public Task<HttpResponseMessage> TraceAsync(string uri) {
            return HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Trace, uri));
        }
        public Task<HttpResponseMessage> TraceAsync(Uri uri) {
            return HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Trace, uri));
        }
    }
}
