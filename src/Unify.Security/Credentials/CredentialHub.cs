using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace CNCO.Unify.Security.Credentials {
    /// <summary>
    /// Stores a collection of credentials in a JSON format as a single credential in a <see cref="ICredentialManagerEndpoint"/>.
    /// </summary>
    internal class CredentialHub : ICredentialManager {
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
            }
        }
        private void PushCredentials() {
            try {
                string credentials = JsonSerializer.Serialize(_credentials);
                if (!_credentialManager.Set(_credentialsName, credentials))
                    throw new InvalidOperationException($"Failed to set credentials in {_credentialManager.GetType().Name}.");
            } catch (Exception ex ) {
                SecurityRuntime.Current.Log.Error(
                    $"{GetType().Name}::{nameof(PullCredentials)}",
                    $"Failed to push JSON credentials for {_credentialsName}."
                );

                SecurityRuntime.Current.Log.Error(ex.Message);
                SecurityRuntime.Current.Log.Error(ex.StackTrace ?? "No stack trace available.");
            }
        }

        public bool Exists(string credentialName) {
            PullCredentials();
            return _credentials.ContainsKey(credentialName);
        }

        public string? Get(string credentialName) {
            PullCredentials();
            _credentials.TryGetValue(credentialName, out string? value);
            return value;
        }

        public bool Remove(string credentialName) {
            bool removed = _credentials.Remove(credentialName);
            PushCredentials();
            return removed;
        }

        public bool Set(string credentialName, string value) {
            PullCredentials();
            _credentials[credentialName] = value;
            PushCredentials();
            return true;
        }
    }
}
