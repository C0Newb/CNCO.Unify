namespace CNCO.Unify.Storage;

/// <summary>
/// Storage written to the local machine.
/// </summary>
public interface ILocalFileStorage : IFileStorage
{
  /// <summary>
  /// Directory all files are stored in.
  /// </summary>
  /// <remarks>
  /// This is the path everything is written to.
  /// </remarks>
  public string Directory { get; }
}
