using CNCO.Unify.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace CNCO.Unify.Communications.Http {
    /// <summary>
    /// Represents a blank, no-operation WebResponse.
    /// </summary>
    /// <remarks>
    /// This is primarily used by <see cref="Routing.ControllerContext"/> when a <see cref="WebSocket"/> is in use.
    /// </remarks>
    public class NoopWebResponse : IWebResponse {
        public string? ContentType {
            get => null;
            set => throw new InvalidOperationException();
        }
        public CookieCollection Cookies {
            get => throw new InvalidOperationException();
            set => throw new InvalidOperationException();
        }
        public bool HasEnded => true;
        public WebHeaderCollection Headers {
            get => throw new InvalidOperationException();
            set => throw new InvalidOperationException();
        }
        public bool KeepAlive => false;
        public string? RedirectLocation {
            get => null;
            set => throw new InvalidOperationException();
        }

        public void AddCookie(Cookie cookie) => Cookies.Add(cookie);
        public void AddHeader(string name, string value) => Headers.Add(name, value);
        public void AppendHeader(string name, string value) => Headers.Add(name, value);
        public void Attachment(string fileName) => throw new InvalidOperationException();
        public void End() => throw new InvalidOperationException();
        public void Redirect(string uri) => throw new InvalidOperationException();
        public void Send(string? data) => throw new InvalidOperationException();
        public void SendAttachment(string path, IFileStorage storage, AttachmentOptions? attachmentOptions = null) => throw new InvalidOperationException();
        public void SendFile(string path, IFileStorage storage, string? fileType = null) => throw new InvalidOperationException();
        public void SendJson(JsonObject? data) => throw new InvalidOperationException();
        public void Status(int statusCode) => throw new InvalidOperationException();
    }
}
