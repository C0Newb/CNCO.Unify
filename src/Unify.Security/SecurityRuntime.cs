using CNCO.Unify.Logging;
using CNCO.Unify.Security.Credentials;
using CNCO.Unify.Storage;
using System.Text;

namespace CNCO.Unify.Security {
    public class SecurityRuntime : IRuntime {
        private static SecurityRuntime? _instance;
        private ProxyLogger? _log;


        // What in ?
        private readonly ICredentialManagerEndpoint _platformCredentialManager = CredentialManagerFactory.GetPlatformCredentialManager();
        private IEncryptionKeyProvider? _credentialManagerKeyProvider;
        private FileEncryption? _credentialManagerFileEncryption;
        private IFileStorage? _credentialFileStorage;
        private FileBasedCredentialManager? _credentialManager;

        /// <summary>
        /// Security runtime application log.
        /// </summary>
        internal ProxyLogger Log {
            get {
                _log ??= new ProxyLogger(Runtime.ApplicationLog, "Unify-Security");
                return _log;
            }
        }

        /// <summary>
        /// The current (static) application <see cref="SecurityRuntime"/>.
        /// </summary>
        public static SecurityRuntime Current {
            get {
                if (_instance == null) {
                    _instance ??= new SecurityRuntime();
                    Runtime.AddRuntimeLink(new RuntimeLink(_instance));
                }
                return _instance;
            }
        }

        /// <summary>
        /// The current credential manager for this platform.
        /// </summary>
        /// <remarks>
        /// Credential manager here is an instance of <see cref="FileBasedCredentialManager"/>, and all credentials are stored to a credentials JSON file.
        /// The encryption key to protect the credentials JSON file is 256 characters long, converted to bytes, encoded as a base64 string and then stored using the current platforms <see cref="ICredentialManagerEndpoint"/>.
        /// This is done as some <see cref="ICredentialManagerEndpoint"/>'s may have a limit on the length of stored data.
        /// </remarks>
        public ICredentialManager CredentialManager {
            get {
                _credentialManagerKeyProvider ??= new EncryptionKeyProvider(GetEncryptionKey(), Encryption.Protections.AES256_CBC);
                _credentialManagerFileEncryption ??= new FileEncryption(_credentialManagerKeyProvider);
                _credentialFileStorage ??= new LocalFileStorage(Runtime.Current.Configuration.CredentialFileParentDirectoryName);
                _credentialManager ??= new FileBasedCredentialManager(_credentialFileStorage, Runtime.Current.Configuration.CredentialFileName, _credentialManagerFileEncryption);
                return _credentialManager;
            }
        }

        public SecurityRuntime() { }

        public void Initialize() { }


        private string GenerateNewMasterKey() {
            string masterKey = Encryption.GenerateRandomString(256);
            byte[] masterKeyBytes = Encoding.UTF8.GetBytes(masterKey);
            string masterKeyBase64 = Convert.ToBase64String(masterKeyBytes);
            _platformCredentialManager.Set($"{Runtime.Current.ApplicationId}::Key", masterKeyBase64);
            return masterKey;
        }

        private byte[] GetEncryptionKey() {
            string? masterKey = _platformCredentialManager.Get($"{Runtime.Current.ApplicationId}::Key");
            if (masterKey != null) {
                byte[] masterKeyBytes = Convert.FromBase64String(masterKey);
                masterKey = Encoding.UTF8.GetString(masterKeyBytes);
            } else {
                masterKey = GenerateNewMasterKey();
            }

            return Encoding.UTF8.GetBytes(masterKey);
        }
    }
}
