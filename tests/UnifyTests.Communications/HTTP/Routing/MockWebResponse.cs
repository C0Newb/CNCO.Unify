using CNCO.Unify.Communications.Http;
using CNCO.Unify.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace UnifyTests.Communications.Http.Routing {
    internal class MockWebResponse : IWebResponse {
        public string? ContentType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CookieCollection Cookies { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool HasEnded => throw new NotImplementedException();

        public WebHeaderCollection Headers { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool KeepAlive => throw new NotImplementedException();

        public string? RedirectLocation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void AddCookie(Cookie cookie) => throw new NotImplementedException();
        public void AddHeader(string name, string value) => throw new NotImplementedException();
        public void AppendHeader(string name, string value) => throw new NotImplementedException();
        public void Attachment(string fileName) => throw new NotImplementedException();
        public void End() => throw new NotImplementedException();
        public void Redirect(string uri) => throw new NotImplementedException();
        public void Send(string? data) => throw new NotImplementedException();
        public void SendAttachment(string path, IFileStorage storage, AttachmentOptions? attachmentOptions = null) => throw new NotImplementedException();
        public void SendFile(string path, IFileStorage storage, string? fileType = null) => throw new NotImplementedException();
        public void SendJson(JsonObject? data) => throw new NotImplementedException();
        public void Status(int statusCode) => throw new NotImplementedException();
    }
}
