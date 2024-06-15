using CNCO.Unify.Logging;
using CNCO.Unify.Security.Credentials;
using CNCO.Unify.Storage;
using System.Text;

namespace CNCO.Unify.Security {
    [LinkRuntime(typeof(UnifyRuntime))]
    public class SecurityRuntime : Runtime, IRuntime {
        private static SecurityRuntime? _instance;

        // What in ?
        private readonly ICredentialManagerEndpoint _platformCredentialManager = CredentialManagerFactory.GetPlatformCredentialManager();
        private readonly IEncryptionKeyProvider? _encryptionKeyProvider;
        private readonly IEncryptionProvider? _encryptionProvider;
        private readonly IFileStorage? _fileStorage;
        private readonly ICredentialManager? _credentialManager;

        protected static new readonly object _initializationLock = new object();

        #region Properties
        /// <summary>
        /// Security runtime application log.
        /// </summary>
        internal new ILogger RuntimeLog {
            get => base.RuntimeLog;
        }

        /// <summary>
        /// Runtime configuration.
        /// </summary>
        public SecurityRuntimeConfiguration Configuration { get; private set; }

        /// <summary>
        /// The current (static) application <see cref="SecurityRuntime"/>.
        /// </summary>
        public static SecurityRuntime Current {
            get {
                if (_instance == null) { // Null?
                    lock (_initializationLock) { // Should only hit one time.
                        _instance ??= new SecurityRuntime();
                    }
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
        /// <exception cref="NullReferenceException">CredentialManager was not initialized in the constructor.</exception>
        public ICredentialManager CredentialManager {
            get {
                if (_instance?._credentialManager == null) {
                    RuntimeLog.Error($"{nameof(_credentialManager)} has not been initialized, but should have been in the runtime constructor.");
                    throw new NullReferenceException("The credential manager has not been initialized.");
                }

                return _instance._credentialManager;
            }
        }
        #endregion

        public SecurityRuntime() : this(null) { }

        public SecurityRuntime(SecurityRuntimeConfiguration? runtimeConfiguration = null) {
            if (_instance != null)
                return;

            Configuration = runtimeConfiguration ?? new SecurityRuntimeConfiguration();

            lock (_initializationLock) {
                if (_instance != null)
                    return;

                _instance = this;

                _platformCredentialManager = runtimeConfiguration?.PlatformCredentialManager ?? _platformCredentialManager;
                _encryptionKeyProvider = runtimeConfiguration?.KeyProvider;
                _encryptionProvider = runtimeConfiguration?.EncryptionProvider;
                _fileStorage = runtimeConfiguration?.FileStorage;
                _credentialManager = runtimeConfiguration?.CredentialManager;


                // Initialize up to a credential manager. Use as much of the runtime configuration as we can
                if (_credentialManager != null) // Well..
                    return; // it already exists

                if (_encryptionProvider == null) {
                    // Create platform credMan and providers
                    _platformCredentialManager ??= CredentialManagerFactory.GetPlatformCredentialManager();
                    _encryptionKeyProvider ??= new EncryptionKeyProvider(GetEncryptionKey());
                    _encryptionProvider ??= new EncryptionProvider(_encryptionKeyProvider);
                }

                _fileStorage ??= new LocalFileStorage(runtimeConfiguration?.CredentialFileParentDirectoryName);
                string credentialsPath = runtimeConfiguration?.CredentialsFileName ?? UnifyRuntime.Current.ApplicationId;

                RuntimeLog.Log($"Initialized {nameof(CredentialManager)}.");
                RuntimeLog.Debug(
                    $"{nameof(_platformCredentialManager)}<?>: {_platformCredentialManager.GetType().FullName}" +
                    $"{nameof(_encryptionKeyProvider)}<?>: {_encryptionKeyProvider?.GetType().FullName ?? "none!"}" +
                    $"{nameof(_encryptionProvider)}<?>: {_encryptionProvider.GetType().FullName}" +
                    $"{nameof(_fileStorage)}<?>: {_fileStorage.GetType().FullName}" +
                    $"Path: {credentialsPath}"
                );
                _credentialManager = new FileBasedCredentialManager(
                    _fileStorage,
                    credentialsPath,
                    _encryptionProvider
                );
            }
        }

        public static SecurityRuntime Create(SecurityRuntimeConfiguration? runtimeConfiguration)
            => new SecurityRuntime(runtimeConfiguration);


        #region Key generation
        private string GenerateNewMasterKey() {
            string masterKey = Encryption.GenerateRandomString(256);
            byte[] masterKeyBytes = Encoding.UTF8.GetBytes(masterKey);
            string masterKeyBase64 = Convert.ToBase64String(masterKeyBytes);
            _platformCredentialManager.Set($"{UnifyRuntime.Current.ApplicationId}::Key", masterKeyBase64);
            return masterKey;
        }

        private byte[] GetEncryptionKey() {
            string? masterKey = _platformCredentialManager.Get($"{UnifyRuntime.Current.ApplicationId}::Key");
            if (masterKey != null) {
                byte[] masterKeyBytes = Convert.FromBase64String(masterKey);
                masterKey = Encoding.UTF8.GetString(masterKeyBytes);
            } else {
                masterKey = GenerateNewMasterKey();
            }

            return Encoding.UTF8.GetBytes(masterKey);
        }
        #endregion
    }
}
