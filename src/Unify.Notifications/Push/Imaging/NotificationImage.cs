using System.Text.RegularExpressions;
using CNCO.Unify.Storage;
using SkiaSharp;

namespace CNCO.Unify.Notifications.Push.Imaging;

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
public partial class NotificationImage(string? imageString) : INotificationImage
{
  [GeneratedRegex(@"^data:image\/(jpeg|png|gif|tiff|ico|emf|wmf|exif);(base64|hex),")]
  private static partial Regex ImageFileTypeAndEncoding();

  private static ILocalFileStorage FileStorage => NotificationRuntime.ImageFileStore;

  private string FileName => Id.ToString();

  public Guid Id { get; } = Guid.NewGuid();
  public string? AlternativeText { get; set; } = null;
  public bool IsWritten => FileStorage != null && FileStorage.Exists(FileName);
  public Uri? Uri => !IsWritten && !WriteImage() ? null : new Uri(FileStorage.GetPath(FileName));

  public void DeleteImage()
  {
    if (FileStorage != null && FileStorage.Exists(FileName))
    {
      _ = FileStorage.Delete(FileName);
    }
  }

  public void SetImage(string imageData) => imageString = imageData;

  public bool WriteImage()
  {
    if (IsWritten)
    {
      return true;
    }
    if (string.IsNullOrEmpty(imageString))
    {
      return false;
    }

    // Decode the image data
    var imageFormat = DecodeImageData(out byte[] imageBytes);

    // Load the image
    using MemoryStream imageMemoryStream = new MemoryStream(imageBytes);
    using SKBitmap bitmap = SKBitmap.Decode(imageMemoryStream);
    using SKImage image = SKImage.FromBitmap(bitmap);
    var data = image.Encode(imageFormat, 75);
    var dataStream = data.AsStream();
    return FileStorage.Write(FileName, dataStream);
  }

  private SKEncodedImageFormat DecodeImageData(out byte[] imageData)
  {
    // Verify the data string contains image data
    Match match = ImageFileTypeAndEncoding().Match(imageString ?? string.Empty);

    if (string.IsNullOrEmpty(imageString) || !match.Success)
    {
      throw new InvalidOperationException("Invalid image data string format.");
    }

    // Extract information from the data string
    var encodingType = match.Groups[2].Value;
    var encodedData = imageString[match.Length..].Trim();

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

    return match.Groups[1].Value.ToLower() switch
    {
      "jpg" or "jpeg" => SKEncodedImageFormat.Jpeg,
      "heif" => SKEncodedImageFormat.Heif,
      "webp" => SKEncodedImageFormat.Webp,
      "gif" => SKEncodedImageFormat.Gif,
      "ico" => SKEncodedImageFormat.Ico,
      "png" => SKEncodedImageFormat.Png,
      _ => SKEncodedImageFormat.Bmp, // Unsupported?
    };
  }
}
