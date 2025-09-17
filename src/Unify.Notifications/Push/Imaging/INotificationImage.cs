using CNCO.Unify.Storage;

namespace CNCO.Unify.Notifications.Push.Imaging {
    /// <summary>
    /// Notification image.
    /// </summary>
    public interface INotificationImage {
        /// <summary>
        /// Image id.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Alternative text for the image.
        /// </summary>
        public string? AlternativeText { get; set; }

        /// <summary>
        /// Location of the image on the local disk
        /// </summary>
        public Uri? Uri { get; }

        /// <summary>
        /// Whether the image is written to disk or not.
        /// </summary>
        public bool IsWritten { get; }

        /// <summary>
        /// Sets the image data.
        /// </summary>
        /// <param name="imageData">Image data in the following format: <code>data:image/png;base64,{image data}</code>.</param>
        public void SetImage(string imageData);

        /// <summary>
        /// Writes the image to local storage.
        /// </summary>
        /// <param name="fileStorage">Local file storage handler.</param>
        /// <returns>
        /// Whether the image is/was written to disk.
        /// </returns>
        public bool WriteImage(ILocalFileStorage? fileStorage = null);

        /// <summary>
        /// Deletes the image from local storage.
        /// </summary>
        public void DeleteImage();
    }
}
