using CNCO.Unify.Storage;
using System.Text.Json;

namespace CNCO.Unify.Security.Credentials {
    /// <summary>
    /// <see cref="ICredentialManager"/> using a JSON file.
    /// This is not secure and should not be used unless the file is guaranteed to be protected.
    /// </summary>
    public class FileBasedCredentialManager : ICredentialManager, ICredentialManagerEndpoint {
        private readonly object _lock = new object();

        private readonly IFileStorage _fileStorage;
        private IEncryptionProvider? _fileEncryption;
        private readonly string _fileName = "Unify.Credentials.json";
        private Dictionary<string, string> _credentials = new Dictionary<string, string>();


        /// <summary>
        /// Initializes a new instance of <see cref="FileBasedCredentialManager"/>.
        /// NOTE!! This will default to NO encryption. Please provide a <see cref="IEncryptionProvider"/> via a different constructor!
        /// </summary>
        public FileBasedCredentialManager() {
            _fileStorage = new LocalFileStorage();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="FileBasedCredentialManager"/>.
        /// NOTE!! This will default to NO encryption. Please provide a <see cref="IEncryptionProvider"/> via a different constructor!
        /// </summary>
        /// <param name="fileEncryption">File encryption scheme used when writing the credentials to the local storage.</param>
        public FileBasedCredentialManager(IEncryptionProvider fileEncryption) : this() {
            _fileEncryption = fileEncryption;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="FileBasedCredentialManager"/>.
        /// NOTE!! This will default to NO encryption. Please provide a <see cref="IEncryptionProvider"/> via a different constructor!
        /// </summary>
        /// <param name="fileStorage">File storage backing the credentials will be written to.</param>
        /// <param name="fileName">Name of the file the credentials will be written to.</param>
        public FileBasedCredentialManager(IFileStorage fileStorage, string fileName) {
            _fileStorage = fileStorage;
            _fileName = fileName;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="FileBasedCredentialManager"/>.
        /// </summary>
        /// <param name="fileStorage">File storage backing the credentials will be written to.</param>
        /// <param name="fileName">Name of the file the credentials will be written to.</param>
        /// <param name="fileEncryption">File encryption scheme used when writing the credentials to <paramref name="fileStorage"/>.</param>
        public FileBasedCredentialManager(IFileStorage fileStorage, string fileName, IEncryptionProvider fileEncryption) {
            _fileStorage = fileStorage;
            _fileName = fileName;
            _fileEncryption = fileEncryption;
        }

        /// <summary>
        /// Loads the credentials to <see cref="IFileStorage"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
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

        /// <summary>
        /// Saves the credentials to <see cref="IFileStorage"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
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


        public void SetFileEncryption(IEncryptionProvider newFileEncryption) {
            lock (_lock) {
                PullCredentials();
                _fileEncryption = newFileEncryption;
                PushCredentials();
            }
        }
    }
}
