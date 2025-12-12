namespace CNCO.Unify;

[Serializable]
public class UnsupportedPlatformException : Exception
{
  public UnsupportedPlatformException(string tag)
    : base($"{tag} is unsupported on this platform!") { }

  public UnsupportedPlatformException(string tag, string message)
    : base(message) { }
}
