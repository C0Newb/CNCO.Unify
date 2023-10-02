using CNCO.Unify.Configuration.Encryption;
using CNCO.Unify.Configuration.Storage;
using CNCO.Unify.Security;
using System.Runtime.InteropServices;
using System.Security;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace CNCO.Unify.Configuration.Json {

    /// <summary>
    /// Utilizes encryption methods to provide protected properties (secrets).
    /// These properties are encrypted until you use <see cref="DecryptSecret(string)"/> to decrypt them. This provides protection at reset and in memory.
    /// </summary>
    public class SecureJsonConfiguration : JsonConfiguration {
        #region Secrets key data
        // Secrets
        // SecretsKeyEncrypted -> (decrypted) SecretsKey -> (update/regenerate) SecretsEncryptionKey

        /// <summary>
        /// Encrypted version of <see cref="SecretsKey"/>. Encrypted via the AspNetCore DataProtector (similar to DPAPI), cannot be transferred between machines!
        /// </summary>
        [JsonPropertyName("secrets_key")]
        public string SecretsKeyEncrypted {
            get => _secretsKeyEncrypted ?? "";
            set {
                _secretsKeyEncrypted = value;
                SecretsKey.Clear();
                if (!string.IsNullOrEmpty(value)) {
                    var key = Security.Encryption.DecryptAspNetCoreDataProtector(value);
                    var newSecretsKey = new SecureString();
                    foreach (char c in key)
                        newSecretsKey.AppendChar(c);
                    SecretsKey = newSecretsKey;
                }
            }
        }
        private string? _secretsKeyEncrypted;


        /// <summary>
        /// Encryption key for secrets stored in the configuration.
        /// This key is not actually the key, rather it is thrown into <see cref="Encryption.DeriveKey(SecureString, byte[], int, int)"/> and the resulting output is the actual key.
        /// </summary>
        [JsonIgnore]
        public SecureString SecretsKey {
            get => _secretsKey;
            set {
                _secretsKey = value;
                SecretsEncryptionKey = Security.Encryption.DeriveKey(SecretsKey, _secretsSalt);
            }
        }
        private SecureString _secretsKey = new SecureString();

        /// <summary>
        /// Paired with <see cref="SecretsKey"/>, used to help derive the encryption key for secrets.
        /// </summary>
        [JsonPropertyName("secrets_salt")]
        public string SecretsSalt {
            get => Convert.ToBase64String(_secretsSalt);
            set {
                _secretsSalt = Convert.FromBase64String(value);
                SecretsEncryptionKey = Security.Encryption.DeriveKey(SecretsKey, _secretsSalt);
            }
        }
        [JsonIgnore]
        private byte[] _secretsSalt = Security.Encryption.GenerateRandomBytes(32);

        /// <summary>
        /// Encryption methods (protections) applied to secrets.
        /// </summary>
        [JsonPropertyName("secrets_protections")]
        public Security.Encryption.Protections SecretsProtections { get; set; } = Security.Encryption.Protections.AES128_CBC;

        /// <summary>
        /// Actual encryption key used to decrypt configuration secrets.
        /// </summary>
        [JsonIgnore]
        private byte[] SecretsEncryptionKey = Array.Empty<byte>();
        #endregion

        public SecureJsonConfiguration() : base() { }

        public SecureJsonConfiguration(string filePath, IFileStorage fileStorage, IFileEncryption fileEncryption) : base(filePath, fileStorage, fileEncryption) {
            // Generates a new secrets encryption key if one is not already there.
            if (string.IsNullOrEmpty(_secretsKeyEncrypted)) {
                string newKey = Security.Encryption.GenerateRandomString(32);
                _secretsSalt = Security.Encryption.GenerateRandomBytes(32);
                SecretsKeyEncrypted = Security.Encryption.EncryptAspNetCoreDataProtector(newKey);
            }
        }


        /// <summary>
        /// Decrypts a secret using the configuration's secret key.
        /// </summary>
        /// <param name="value">Data to decrypt.</param>
        /// <returns>Decrypted secret.</returns>
        public string DecryptSecret(string value) {
            return Security.Encryption.Decrypt(value, SecretsEncryptionKey);
        }
        /// <summary>
        /// Encrypts a string using the configuration's secret key.
        /// </summary>
        /// <param name="value">Data to encrypt.</param>
        /// <returns>Encrypted secret.</returns>
        public string EncryptSecret(string value) {
            return Security.Encryption.Encrypt(value, SecretsEncryptionKey, SecretsProtections);
        }
    }
}
