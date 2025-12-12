

namespace CNCO.Unify.Storage;

/// <summary>
/// Used to provide no storage capabilities when a <see cref="IFileStorage"/> is required.
/// 
/// Will pretend the file was written but cannot be read and does not exist.
/// </summary>
public class NoopFileStorage : IFileStorage
{
  public bool Append(string name, string contents) => true;

  public bool AppendBytes(string name, byte[] contents) => true;

  public bool Delete(string name) => true;

  public bool Exists(string name) => false;
  public IEnumerable<string> GetDirectories(string? path, string? pattern = null) => [];
  public IEnumerable<string> GetFiles(string? path, string? pattern = null) => [];
  public string GetPath(string filename) => filename;
  public bool IsDirectory(string path) => false;
  public FileStream? Open(string name, FileStreamOptions? streamOptions) => null;
  public string? Read(string name) => null;

  public byte[]? ReadBytes(string name) => null;
  public bool Rename(string name, string newName) => true;
  public bool Write(string name, string contents) => true;
  public bool Write(string name, Stream contents) => true;
  public bool WriteBytes(string name, byte[] contents) => true;
  Stream? IFileStorage.Open(string name, FileStreamOptions? streamOptions) => new MemoryStream();
}