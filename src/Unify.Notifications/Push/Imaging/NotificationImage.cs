using System.Text.RegularExpressions;
using CNCO.Unify.Storage;
using SkiaSharp;

namespace CNCO.Unify.Notifications.Push.Imaging;

public partial class NotificationImage : INotificationImage
{
  [GeneratedRegex(@"^data:image\/[a-zA-Z]+;(base64|hex),")]
  private static partial Regex ImageFileTypeAndEncoding();

  private string? _imageString;
  private readonly byte[]? _imageData;

  private static ILocalFileStorage FileStorage => NotificationRuntime.ImageFileStore;

  private string FileName => Id.ToString();

  public Guid Id { get; } = Guid.NewGuid();
  public string? AlternativeText { get; set; } = null;
  public bool IsWritten => FileStorage != null && FileStorage.Exists(FileName);
  public Uri? Uri
  {
    get
    {
      if (!string.IsNullOrEmpty(_imageString) && _imageString.StartsWith("https://"))
      {
        return new Uri(_imageString);
      }
      return !IsWritten && !WriteImage() ? null : new Uri(FileStorage.GetPath(FileName));
    }
  }

  /// <summary>
  /// Initializes a new instance of the NotificationImage class using the specified image string.
  /// </summary>
  /// <param name="imageString">
  ///   <para>
  ///   A string that represents the image data in the format:
  ///   </para>
  ///
  ///   <para>
  ///   <c>data:image/jpg;base64,...</c>, or
  ///   </para>
  ///   <para>
  ///   <c>data:image/jpg;base64,...</c>, where "<c>...</c>" is the actual encoded image data.
  ///   </para>
  ///
  ///   <remarks>
  ///   If no data provide, no image will be available.
  ///   </remarks>
  /// </param>
  public NotificationImage(string? imageString) => _imageString = imageString;

  public NotificationImage(byte[] imageData) => _imageData = imageData;

  public void DeleteImage()
  {
    if (FileStorage != null && FileStorage.Exists(FileName))
    {
      _ = FileStorage.Delete(FileName);
    }
  }

  public void SetImage(string imageData) => _imageString = imageData;

  public bool WriteImage()
  {
    if (IsWritten)
    {
      return true;
    }
    if (_imageData == null && string.IsNullOrEmpty(_imageString))
    {
      return false;
    }

    byte[] imageBytes = _imageData!;

    if (imageBytes == null)
    {
      // Decode the image data
      DecodeImageData(out imageBytes);
    }

    // Load the image
    using SKImage image = SKImage.FromEncodedData(imageBytes);
    var data = image.Encode(SKEncodedImageFormat.Png, 75);
    var dataStream = data.AsStream();
    return FileStorage.Write(FileName, dataStream);
  }

  private void DecodeImageData(out byte[] imageData)
  {
    // Verify the data string contains image data
    Match match = ImageFileTypeAndEncoding().Match(_imageString ?? string.Empty);

    if (string.IsNullOrEmpty(_imageString) || !match.Success)
    {
      throw new InvalidOperationException("Invalid image data string format.");
    }

    // Extract information from the data string
    var encodingType = match.Groups[2].Value;
    var encodedData = _imageString[match.Length..].Trim();

    if (encodingType == "base64")
    {
      imageData = Convert.FromBase64String(encodedData);
    }
    else if (encodingType == "hex")
    {
      int byteCount = encodedData.Length / 2;
      imageData = new byte[byteCount];

      for (int i = 0; i < byteCount; i++)
      {
        imageData[i] = Convert.ToByte(encodedData.Substring(i * 2, 2), 16);
      }
    }
    else
    {
      imageData = null!;
    }

    if (imageData == null)
    {
      throw new InvalidOperationException("Unable to decode null image data.");
    }
  }
}
