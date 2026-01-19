using System.Collections.Concurrent;

namespace CNCO.Unify.Security.Credentials;

internal class InMemoryCredentialManager : ICredentialManagerEndpoint
{
  private readonly ConcurrentDictionary<string, string> _credentials = [];

  public bool Exists(string credentialName) => _credentials.ContainsKey(credentialName);

  public string? Get(string credentialName) =>
    _credentials.TryGetValue(credentialName, out var value) ? value : null;

  public void Remove(string credentialName) => _credentials.TryRemove(credentialName, out _);

  public void Set(string credentialName, string value) => _credentials[credentialName] = value;
}
