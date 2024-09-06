using CNCO.Unify.Storage;
using System.Net;
using System.Text.Json.Nodes;

namespace CNCO.Unify.Communications.Http {
    /// <summary>
    /// Represents an HTTP response.
    /// </summary>
    public interface IWebResponse {
        /// <summary>
        /// Gets or sets the ContentType header for the response.
        /// </summary>
        string? ContentType { get; set; }

        /// <summary>
        /// Collection of cookies returned in the response.
        /// </summary>
        CookieCollection Cookies { get; set; }

        /// <summary>
        /// Whether or not the response stream has closed and the response was sent.
        /// </summary>
        bool HasEnded { get; }

        /// <summary>
        /// Headers to be sent in the response.
        /// </summary>
        WebHeaderCollection Headers { get; set; }

        /// <summary>
        /// Gets whether the server requests a persistent connection.
        /// </summary>
        bool KeepAlive { get; }

        /// <summary>
        /// Value of the Location header, which specifies a URI to which the client
        /// is redirect to obtain the requested resource.
        /// </summary>
        string? RedirectLocation { get; set; }


        /// <summary>
        /// Adds the specified <see cref="Cookie" /> to the collection of cookies for this response.
        /// </summary>
        /// <param name="cookie">Cookie to add to the collection of cookies for this response.</param>
        void AddCookie(Cookie cookie);

        /// <summary>
        /// Adds the specified header and value to the HTTP headers for this response.
        /// </summary>
        /// <param name="name">Name of the HTTP header to set.</param>
        /// <param name="value">Value of the header.</param>
        void AddHeader(string name, string value);

        /// <summary>
        /// Appends a value to the specified HTTP header to be sent with this response.
        /// </summary>
        /// <param name="name">Name of the HTTP header to append to.</param>
        /// <param name="value">Value to append.</param>
        void AppendHeader(string name, string value);

        /// <summary>
        /// Does not send anything, sets the <c>Content-Disposition</c> header to "attachment".
        /// </summary>
        /// <param name="fileName">What the attachment will be named when sent to the user.</param>
        void Attachment(string fileName);

        /// <summary>
        /// Marks the response as being sent and close the response stream.
        /// </summary>
        void End();

        /// <summary>
        /// Sends a redirect response code to request a redirect to <paramref name="uri"/>.
        /// </summary>
        /// <param name="uri">Uri to request the caller to go to.</param>
        void Redirect(string uri);

        /// <summary>
        /// Sends a string back as the response. Ends the response.
        /// </summary>
        /// <param name="data">String to respond with.</param>
        void Send(string? data);

        /// <summary>
        /// Sends a file to the user that will be downloaded.
        /// </summary>
        /// <param name="path">Path to the file to download.</param>
        /// <param name="attachmentOptions">Options related to the file being downloaded.</param>
        void SendAttachment(string path, IFileStorage storage, AttachmentOptions? attachmentOptions = null);

        /// <summary>
        /// This will *serve* a file, not mark is to be downloaded by the user.
        /// All we do is read a file and send it's contents back.
        /// </summary>
        /// <param name="path">Path to the file to send.</param>
        /// <param name="fileType">Use <see cref="System.Net.Mime.MediaTypeNames"/>.</param>
        void SendFile(string path, IFileStorage storage, string? fileType = null);

        /// <summary>
        /// Send a JSON response back. Ends the response.
        /// </summary>
        /// <param name="data">JSON data to send.</param>
        void SendJson(JsonObject? data);

        /// <summary>
        /// Sets the HTTP status for the response.
        /// </summary>
        /// <param name="statusCode"></param>
        void Status(int statusCode);
    }
}