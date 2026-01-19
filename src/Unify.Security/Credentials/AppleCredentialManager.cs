namespace CNCO.Unify.Security.Credentials;

internal class AppleCredentialManager : ICredentialManagerEndpoint
{
  public bool Exists(string credentialName) => throw new NotImplementedException();

  public string? Get(string credentialName) => throw new NotImplementedException();

  public void Remove(string credentialName) => throw new NotImplementedException();

  public void Set(string credentialName, string value) => throw new NotImplementedException();
}
