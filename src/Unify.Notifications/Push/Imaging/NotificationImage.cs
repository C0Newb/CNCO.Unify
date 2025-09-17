using CNCO.Unify.Storage;
using SkiaSharp;
using System.Text.RegularExpressions;

namespace CNCO.Unify.Notifications.Push.Imaging {
    public class NotificationImage : INotificationImage {
        private ILocalFileStorage? _storage;
        private string? _imageString;
        private string FilePath => Id.ToString();


        public Guid Id { get; } = Guid.NewGuid();
        public string? AlternativeText { get; set; } = null;
        public bool IsWritten => _storage != null && _storage.Exists(FilePath);
        public Uri? Uri => IsWritten ? new Uri(_storage!.GetPath(FilePath)) : null;


        public NotificationImage(string? imageString) {
            _imageString = imageString;
        }


        public void SetImage(string imageString) => _imageString = imageString;

        public void DeleteImage() {
            if (_storage == null)
                return;

            if (_storage.Exists(FilePath)) {
                _storage.Delete(FilePath);
            }
        }

        public bool WriteImage(ILocalFileStorage? fileStorage = null) {
            if (IsWritten)
                return true;
            if (string.IsNullOrEmpty(_imageString))
                return false;

            _storage ??= fileStorage ?? NotificationRuntime.ImageFileStore;

            // Verify the data string contains image data
            Regex regex = new Regex(@"^data:image\/(jpeg|png|gif|tiff|ico|emf|wmf|exif);(base64|hex),");
            Match match = regex.Match(_imageString);

            if (!match.Success) {
                throw new ArgumentException("Invalid image data string format.");
            }

            // Extract information from the data string
            string fileType = match.Groups[1].Value;
            string encodingType = match.Groups[2].Value;
            string encodedData = _imageString.Substring(match.Length).Trim();

            // Decode the image data
            byte[]? imageData = null;

            if (encodingType == "base64") {
                imageData = Convert.FromBase64String(encodedData);
            } else if (encodingType == "hex") {
                int byteCount = encodedData.Length / 2;
                imageData = new byte[byteCount];

                for (int i = 0; i < byteCount; i++) {
                    imageData[i] = Convert.ToByte(encodedData.Substring(i * 2, 2), 16);
                }
            }

            if (imageData == null) {
                throw new NullReferenceException("Unable to decode null image data.");
            }

            // Load the image
            using (MemoryStream imageMemoryStream = new MemoryStream(imageData))
            using (SKBitmap bitmap = SKBitmap.Decode(imageMemoryStream))
            using (SKImage image = SKImage.FromBitmap(bitmap)) {
                SKEncodedImageFormat imageFormat = SKEncodedImageFormat.Bmp;
                imageFormat = fileType.ToLower() switch {
                    "jpg" or "jpeg" => SKEncodedImageFormat.Jpeg,
                    "heif" => SKEncodedImageFormat.Heif,
                    "webp" => SKEncodedImageFormat.Webp,
                    "gif" => SKEncodedImageFormat.Gif,
                    "ico" => SKEncodedImageFormat.Ico,
                    "bmp" => SKEncodedImageFormat.Bmp,
                    _ => SKEncodedImageFormat.Png,
                };

                var data = image.Encode(imageFormat, 75);
                var dataStream = data.AsStream();
                return _storage.Write(FilePath, dataStream);
            }
        }
    }
}
