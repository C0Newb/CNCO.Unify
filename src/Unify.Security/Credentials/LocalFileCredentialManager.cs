using CNCO.Unify.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CNCO.Unify.Security.Credentials {
    /// <summary>
    /// <see cref="ICredentialManager"/> using a JSON file.
    /// This is not secure and should not be used unless the file is guaranteed to be protected.
    /// </summary>
    public class FileBasedCredentialManager : ICredentialManager, ICredentialManagerEndpoint {
        private readonly IFileStorage _fileStorage;
        private readonly IFileEncryption? _fileEncryption;
        private readonly string _fileName = "Unify.Credentials.json";
        private Dictionary<string, string> _credentials = new Dictionary<string, string>();

        public FileBasedCredentialManager() {
            _fileStorage = new LocalFileStorage();
        }

        public FileBasedCredentialManager(IFileStorage fileStorage, string fileName) {
            _fileStorage = fileStorage;
            _fileName = fileName;
        }

        public FileBasedCredentialManager(IFileStorage fileStorage, string fileName, IFileEncryption fileEncryption) {
            _fileStorage = fileStorage;
            _fileName = fileName;
            _fileEncryption = fileEncryption;
        }

        private void PullCredentials() {
            try {
                string? credentials = _fileStorage.Read(_fileName);
                if (string.IsNullOrEmpty(credentials)) {
                    _credentials = new Dictionary<string, string>();
                    return;
                }
                if (_fileEncryption != null)
                    credentials = _fileEncryption.DecryptString(credentials);

                _credentials = JsonSerializer.Deserialize<Dictionary<string, string>>(credentials) ?? new Dictionary<string, string>();
            } catch (Exception ex) {
                string tag = $"{GetType().Name}::{nameof(PullCredentials)}";
                SecurityRuntime.Current.Log.Error(tag, $"Failed to pull JSON credentials to disk for {_fileName}.");

                SecurityRuntime.Current.Log.Error(tag, ex.Message);
                SecurityRuntime.Current.Log.Error(tag, ex.StackTrace ?? "No stack trace available.");
            }
        }
        private void PushCredentials() {
            try {
                string credentials = JsonSerializer.Serialize(_credentials);
                if (_fileEncryption != null)
                    credentials = _fileEncryption.EncryptString(credentials);

                if (!_fileStorage.Write(_fileName, credentials))
                    throw new InvalidOperationException($"Failed to write credentials to \"{_fileStorage.GetPath(_fileName)}\".");
            } catch (Exception ex) {
                string tag = $"{GetType().Name}::{nameof(PullCredentials)}";
                SecurityRuntime.Current.Log.Error(tag, $"Failed to push JSON credentials to disk for {_fileName}.");

                SecurityRuntime.Current.Log.Error(tag, ex.Message);
                SecurityRuntime.Current.Log.Error(tag, ex.StackTrace ?? "No stack trace available.");
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
