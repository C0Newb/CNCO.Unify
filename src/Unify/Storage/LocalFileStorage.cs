
namespace CNCO.Unify.Storage;

/// <summary>
/// Saves/loads file from the local filesystem.
/// </summary>
public class LocalFileStorage : ILocalFileStorage
{
  private readonly bool _throwErrors = UnifyRuntime.Current.Configuration.SuppressFileStorageExceptions;

  private readonly string _directory = string.Empty;

  public string Directory
  {
    get => _directory;
  }

  /// <summary>
  /// Initializes an instance of <see cref="LocalFileStorage"/>.
  /// </summary>
  /// <param name="directory">
  /// Parent directory files are written to.
  /// Defaults to current working directory.
  /// </param>
  public LocalFileStorage(string? directory = null)
  {
    if (string.IsNullOrEmpty(directory) || !Path.IsPathRooted(directory))
    {
      // make it rooted
      string root = Platform.GetApplicationRootDirectory();

      directory = Path.Combine(root, directory ?? string.Empty);
    }
    _directory = directory;
  }

  private T Wrap<T>(Func<T> action, T failedResponse)
  {
    try
    {
      return action.Invoke();
    }
    catch
    {
      if (_throwErrors)
        throw;
      return failedResponse;
    }
  }
  private bool Wrap(Func<bool> action) => Wrap(action, false);
  private bool Wrap(Action action)
      => Wrap(() =>
      {
        action.Invoke();
        return true;
      }, false);


  public string GetPath(string name)
  {
    try
    {
      if (!System.IO.Directory.Exists(Directory))
        System.IO.Directory.CreateDirectory(Directory);
    }
    catch { }
    if (!name.StartsWith(Directory))
      return Path.Combine(_directory, name);
    else
      return name;
  }


  public bool Delete(string name) => Wrap(() => File.Delete(GetPath(name)));

  public bool Exists(string name) => Wrap(() => File.Exists(GetPath(name)));

  public string? Read(string name) => Wrap(() => File.ReadAllText(GetPath(name)), null);
  public byte[]? ReadBytes(string name) => Wrap(() => File.ReadAllBytes(GetPath(name)), null);

  public bool Write(string name, string contents) => Wrap(() => File.WriteAllText(GetPath(name), contents));
  public bool Write(string name, Stream contents)
      => Wrap(() =>
      {
        using (var fileStream = File.OpenWrite(GetPath(name)))
          contents.CopyTo(fileStream);
      });
  public bool WriteBytes(string name, byte[] contents) => Wrap(() => File.WriteAllBytes(GetPath(name), contents));

  public bool Append(string name, string contents) => Wrap(() => File.AppendAllText(GetPath(name), contents));

  public bool AppendBytes(string name, byte[] contents)
      => Wrap(() =>
      {
        byte[] current = ReadBytes(name) ?? Array.Empty<byte>();
        byte[] newBytes = new byte[current.Length + contents.Length];

        Buffer.BlockCopy(current, 0, newBytes, 0, current.Length);
        Buffer.BlockCopy(contents, 0, newBytes, current.Length, contents.Length);

        WriteBytes(name, newBytes);
      });

  public bool Rename(string name, string newName) => Wrap(() => File.Move(GetPath(name), GetPath(newName)));

  public Stream? Open(string name, FileStreamOptions? streamOptions)
      => Wrap(() =>
      {
        streamOptions ??= new FileStreamOptions()
        {
          Mode = FileMode.OpenOrCreate,
          Access = FileAccess.ReadWrite,
        };
        return File.Open(GetPath(name), streamOptions);
      }, null);

  public IEnumerable<string> GetFiles(string? path = null, string? searchPattern = null)
      => Wrap(
          () => searchPattern == null
              ? System.IO.Directory.EnumerateFiles(GetPath(path ?? string.Empty))
              : System.IO.Directory.EnumerateFiles(GetPath(path ?? string.Empty), searchPattern),
          []
      );

  public IEnumerable<string> GetDirectories(string? path, string? searchPattern = null)
      => Wrap(
          () => searchPattern == null
              ? System.IO.Directory.EnumerateDirectories(GetPath(path ?? string.Empty))
              : System.IO.Directory.EnumerateDirectories(GetPath(path ?? string.Empty), searchPattern),
          []
      );

  public bool IsDirectory(string path) => Wrap(() => File.GetAttributes(GetPath(path)).HasFlag(FileAttributes.Directory));
}
