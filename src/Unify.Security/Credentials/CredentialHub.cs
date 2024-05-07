using System.Text.Json;

namespace CNCO.Unify.Security.Credentials {
    /// <summary>
    /// Stores a collection of credentials in a JSON format as a single credential in a <see cref="ICredentialManagerEndpoint"/>.
    /// </summary>
    public class CredentialHub : ICredentialManager {
        private readonly object _lock = new object();

        private readonly ICredentialManager _credentialManager;
        private readonly string _credentialsName;
        private Dictionary<string, string> _credentials = new Dictionary<string, string>();

        public CredentialHub(ICredentialManagerEndpoint credentialManager, string credentialsName) {
            _credentialManager = credentialManager;
            _credentialsName = credentialsName;
        }

        private void PullCredentials() {
            try {
                string? credentials = _credentialManager.Get(_credentialsName);
                if (string.IsNullOrEmpty(credentials)) {
                    _credentials = new Dictionary<string, string>();
                    return;
                }

                _credentials = JsonSerializer.Deserialize<Dictionary<string, string>>(credentials) ?? new Dictionary<string, string>();
            } catch (Exception ex) {
                SecurityRuntime.Current.Log.Error(
                    $"{GetType().Name}::{nameof(PullCredentials)}",
                    $"Failed to pull JSON credentials for {_credentialsName}."
                );

                SecurityRuntime.Current.Log.Error(ex.Message);
                SecurityRuntime.Current.Log.Error(ex.StackTrace ?? "No stack trace available.");

                throw;
            }
        }
        private void PushCredentials() {
            try {
                string credentials = JsonSerializer.Serialize(_credentials);
                _credentialManager.Set(_credentialsName, credentials);
                //throw new InvalidOperationException($"Failed to set credentials in {_credentialManager.GetType().Name}.");
            } catch (Exception ex) {
                SecurityRuntime.Current.Log.Error(
                    $"{GetType().Name}::{nameof(PullCredentials)}",
                    $"Failed to push JSON credentials for {_credentialsName}."
                );

                SecurityRuntime.Current.Log.Error(ex.Message);
                SecurityRuntime.Current.Log.Error(ex.StackTrace ?? "No stack trace available.");

                throw;
            }
        }

        public bool Exists(string credentialName) {
            lock (_lock) {
                PullCredentials();
                return _credentials.ContainsKey(credentialName);
            }
        }

        public string? Get(string credentialName) {
            lock (_lock) {
                PullCredentials();
                if (!_credentials.TryGetValue(credentialName, out string? value))
                    return null;
                return CredentialHelpers.GetAndVerifyCredential(value);
            }
        }

        public void Remove(string credentialName) {
            lock (_lock) {
                PullCredentials();
                _credentials.Remove(credentialName);
                PushCredentials();
            }
        }

        public void Set(string credentialName, string value) {
            lock (_lock) {
                PullCredentials();
                _credentials[credentialName] = CredentialHelpers.ApplyTamperHash(value);
                PushCredentials();
            }
        }
    }
}
