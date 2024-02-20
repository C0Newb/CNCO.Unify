using CNCO.Unify.Storage;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CNCO.Unify.Communications.Http {
    public class WebResponse {
        private readonly HttpListenerResponse _response;


        public CookieCollection Cookies {
            get => _response.Cookies;
            set => _response.Cookies = value;
        }
        public WebHeaderCollection Headers {
            get => _response.Headers;
            set => _response.Headers = value;
        }

        public Stream OutputStream {
            get => _response.OutputStream;
        }

        public string? ContentType {
            get => Headers[HttpResponseHeader.ContentType];
            set {
                if (string.IsNullOrEmpty(value))
                    Headers.Remove(HttpResponseHeader.ContentType);
                else
                    Headers.Set(HttpResponseHeader.ContentType, value);
            }
        }

        public bool KeepAlive => _response.KeepAlive;

        public string? RedirectLocation {
            get => Headers[HttpResponseHeader.Location];
            set {
                if (string.IsNullOrEmpty(value))
                    Headers.Remove(HttpResponseHeader.Location);
                else
                    Headers.Set(HttpResponseHeader.Location, value);
            }
        }


        public WebResponse(HttpListenerResponse response) {
            _response = response;
        }

        public void End() {
            _response.Close();
        }

        public void AddCookie(Cookie cookie) {
            ArgumentNullException.ThrowIfNull(cookie, nameof(cookie));
            _response.AppendCookie(cookie);
        }
        public void AddHeader(string name, string value) => _response.AddHeader(name, value);
        public void AppendHeader(string name, string value) => _response.AppendHeader(name, value);


        /// <summary>
        /// Does not send anything, sets the <c>Content-Disposition</c> header to "attachment".
        /// </summary>
        /// <param name="fileName">What the attachment will be named when sent to the user.</param>
        /// <param name="fileType">Use <see cref="System.Net.Mime.MediaTypeNames"/>.</param>
        public void Attachment(string fileName, string fileType) {
            Headers["Content-Disposition"] = "attachment" + (!string.IsNullOrEmpty(fileName) ? $"; filename=\"{fileName}\"" : "");
            if (!string.IsNullOrEmpty(fileType))
                Headers["Content-Type"] = fileType;
        }


        public void Redirect(string uri) {
            RedirectLocation = uri;
            _response.StatusCode = (int)HttpStatusCode.Redirect;
        }

        /// <summary>
        /// Sets the HTTP status for the response.
        /// </summary>
        /// <param name="statusCode"></param>
        public void Status(int statusCode) {
            _response.StatusCode = statusCode;
        }

        /// <summary>
        /// Sends a string back as the response. Ends the response.
        /// </summary>
        /// <param name="data">String to respond with.</param>
        public void Send(string data) {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            OutputStream.Write(bytes);
            _response.Close();
        }

        /// <summary>
        /// Send a JSON response back. Ends the response.
        /// </summary>
        /// <param name="data">JSON data to send.</param>
        public void SendJson(JsonObject data) {
            JsonSerializer.Serialize(OutputStream, data);
            _response.Close();
        }

        /// <summary>
        /// This will *serve* a file, not mark is to be downloaded by the user.
        /// All we do is read a file and send it's contents back.
        /// </summary>
        /// <param name="path"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void SendFile(string path, IFileStorage storage) {
            if (!storage.Exists(path))
                throw new FileNotFoundException(path);

            byte[] bytes = storage.ReadBytes(path) ?? Array.Empty<byte>();
            OutputStream.Write(bytes);
            _response.Close();
        }

        /// <summary>
        /// Sends a file to the user that will be downloaded.
        /// </summary>
        /// <param name="path">Path to the file to download.</param>
        /// <param name="attachmentOptions">Options related to the file being downloaded.</param>
        /// <exception cref="NotImplementedException">Not implemented.</exception>
        public void SendAttachment(string path, IFileStorage storage, AttachmentOptions? attachmentOptions = null) {
            if (!storage.Exists(path))
                throw new FileNotFoundException(path);

            var name = attachmentOptions?.AttachmentName ?? Path.GetFileName(path);

            MimeMapping.TryGetMimeType(path, out string? actualMimeType);
            var mimeType = attachmentOptions?.MimeType ?? actualMimeType ?? "text/plain;charset=UTF-8";
            Attachment(name, mimeType);
            SendFile(path, storage);
        }
    }

    public class AttachmentOptions {
        /// <summary>
        /// Name of the file the user will get.
        /// </summary>
        /// <remarks>
        /// This is not the name of the file, but becomes the file name when the user downloads it.
        /// </remarks>
        public string? AttachmentName;

        /// <summary>
        /// File type.
        /// </summary>
        public string? MimeType;

        public AttachmentOptions() { }
    }
}
