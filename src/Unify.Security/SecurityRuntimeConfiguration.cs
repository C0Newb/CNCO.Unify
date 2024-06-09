using CNCO.Unify.Security.Credentials;
using CNCO.Unify.Storage;
using System.Text.Json.Serialization;

namespace CNCO.Unify.Security {
    public sealed class SecurityRuntimeConfiguration : IRuntimeConfiguration {
        #region Credential manager DI
        /// <summary>
        /// The credential manager the primary application security key is stored to.
        /// By default, we use the <see cref="CredentialManagerFactory"/> to get the current platform's credential manager.
        /// </summary>
        /// <remarks>
        /// This is used to retrieve the encryption key used in <see cref="KeyProvider"/>.
        /// </remarks>
        [JsonIgnore]
        public ICredentialManagerEndpoint? PlatformCredentialManager { get; set; }

        /// <summary>
        /// Key provider used in the <see cref="EncryptionProvider"/>.
        /// By default, this is setup with the application key pulled from the <see cref="PlatformCredentialManager"/>.
        /// </summary>
        /// <remarks>
        /// By specifying this, you can ignore <see cref="PlatformCredentialManager"/> as it wont be needed.
        /// </remarks>
        [JsonIgnore]
        public IEncryptionKeyProvider? KeyProvider { get; set; }

        /// <summary>
        /// Encryption used to protect the <see cref="FileBasedCredentialManager"/> (<see cref="CredentialManager"/>) details saved to <see cref="FileStorage"/>.
        /// </summary>
        /// <remarks>
        /// Notice, this directly influences the security of the default instance of <see cref="CredentialManager"/> unless you provide your own instance.
        /// </remarks>
        [JsonIgnore]
        public IEncryptionProvider? EncryptionProvider { get; set; }

        /// <summary>
        /// File storage strategy used for the default <see cref="CredentialManager"/>, <see cref="FileBasedCredentialManager"/>.
        /// </summary>
        /// <remarks>
        /// If using the default instance of <see cref="CredentialManager"/>, this is where the application secrets are stored to.
        /// </remarks>
        [JsonIgnore]
        public IFileStorage? FileStorage { get; set; }

        /// <summary>
        /// The application wide <see cref="ICredentialManager"/>.
        /// </summary>
        /// <remarks>
        /// By default, this is an instance of <see cref="FileBasedCredentialManager"/> using the storage backing <see cref="FileStorage"/>
        /// and encryption provider <see cref="EncryptionProvider"/>.
        /// </remarks>
        [JsonIgnore]
        public ICredentialManager? CredentialManager { get; set; }
        #endregion


        /// <summary>
        /// File name used for the default instance of <see cref="CredentialManager"/>, <see cref="FileBasedCredentialManager"/>.
        /// </summary>
        public string? CredentialsFileName { get; set; }

        /// <summary>
        /// The directory the <see cref="CredentialsFileName"/> lives under.
        /// </summary>
        /// <remarks>
        /// This will be created if it does not exist. This must be a legal directory path.
        /// </remarks>
        public string? CredentialFileParentDirectoryName { get; set; }

        /// <summary>
        /// Disables checking the hash of credentials when pulling them from the credential manager.
        /// Used by Unify.Security.
        /// </summary>
        public bool DisableCredentialManagerHashChecking { get; set; } = false;
    }
}
