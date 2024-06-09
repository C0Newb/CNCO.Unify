using CNCO.Unify.Security;
using CNCO.Unify.Storage;
using System.Security;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace CNCO.Unify.Configuration.Json {
    /// <summary>
    /// Utilizes encryption methods to provide protected properties (secrets).
    /// </summary>
    public class SecureJsonConfiguration : JsonConfiguration {
        #region Secrets key data
        // Secrets
        // SecretsKeyEncrypted -> (decrypted) SecretsKey -> (update/regenerate) SecretsEncryptionKey

        /// <summary>
        /// Encrypted version of <see cref="SecretsKey"/>. Encrypted via the DataProtector, cannot be transferred between machines!
        /// </summary>
        [JsonPropertyName("secrets_key")]
        public string SecretsKeyEncrypted {
            get => _secretsKeyEncrypted ?? "";
            set {
                _secretsKeyEncrypted = value;
                SecretsKey.Clear();
                if (!string.IsNullOrEmpty(value)) {
                    try {
                        var key = Encryption.DecryptDataProtector(UnifyRuntime.Current.ApplicationId, value);
                        var newSecretsKey = new SecureString();
                        foreach (char c in key)
                            newSecretsKey.AppendChar(c);
                        SecretsKey = newSecretsKey;
                    } catch (Exception ex) {
                        string tag = $"{GetType().Name}::{nameof(SecretsKeyEncrypted)}-{GetFilePath()}";
                        UnifyRuntime.ApplicationLog.Error(tag, "Failed to decrypt secrets_key, secrets potentially lost for good!");
                        UnifyRuntime.ApplicationLog.Error(tag, ex.Message);
                        UnifyRuntime.ApplicationLog.Error(tag, ex.StackTrace ?? "No stack trace.");

                        string newKey = Encryption.GenerateRandomString(32);
                        var newSecretsKey = new SecureString();
                        foreach (char c in newKey)
                            newSecretsKey.AppendChar(c);
                        SecretsKey = newSecretsKey;

                        _secretsSalt = Encryption.GenerateRandomBytes(32);
                        _secretsKeyEncrypted = Encryption.EncryptDataProtector(UnifyRuntime.Current.ApplicationId, newKey);
                    }
                }
            }
        }
        private string? _secretsKeyEncrypted;


        /// <summary>
        /// Encryption key for secrets stored in the configuration. Use this to set a custom secrets key.
        /// This key is not actually the key, rather it is thrown into <see cref="Security.Encryption.DeriveKey(SecureString, byte[], int, int)"/> and the resulting output is the actual key.
        /// </summary>
        [JsonIgnore]
        public SecureString SecretsKey {
            get => _secretsKey;
            set {
                _secretsKey = value;
                SecretsEncryptionKey = Encryption.DeriveKey(value, _secretsSalt);
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
                SecretsEncryptionKey = Encryption.DeriveKey(SecretsKey, _secretsSalt);
            }
        }
        [JsonIgnore]
        private byte[] _secretsSalt = Encryption.GenerateRandomBytes(32);

        /// <summary>
        /// Encryption methods (protections) applied to secrets.
        /// </summary>
        [JsonPropertyName("secrets_protections")]
        public Encryption.Protections SecretsProtections { get; set; } = Encryption.Protections.AES128_CBC;

        /// <summary>
        /// Actual encryption key used to decrypt configuration secrets.
        /// </summary>
        [JsonIgnore]
        private byte[] SecretsEncryptionKey = Array.Empty<byte>();

        /*
         What is this? Removable??
        /// <summary>
        /// An encrypted version of <see cref="ProtectedKeys"/>.
        /// </summary>
        [JsonPropertyName("secrets_protected_keys")]
        private string ProtectedKeysJson {
            get {
                string keys = string.Join("\x1E", _secretsSalt); // x1E -> RS -> record separator
                return Encryption.Encrypt(keys, SecretsEncryptionKey, SecretsProtections);
            }
            set {
                string keysUnprotected = Encryption.Decrypt(value, SecretsEncryptionKey);
                string[] keys = keysUnprotected.Split("\x1E", StringSplitOptions.RemoveEmptyEntries);
                ProtectedKeys.Clear();
                ProtectedKeys.AddRange(keys);
            }
        }
        private List<string> ProtectedKeys { get; set; } = new List<string>();
        */
        #endregion


        #region SecureAttribute stuff
        // So we don't setup twice.
        private bool _secureAttributeSetupCompleted = false;

        /// <summary>
        /// Sets up the <see cref="JsonSerializerOptions"/> and dynamically creates the private properties for properties with the <see cref="SecureAttribute"/>.
        /// </summary>
        private void Setup() {
            if (_secureAttributeSetupCompleted || string.IsNullOrEmpty(GetFilePath()))
                return;

            try {
                var encryptionFunction = new Func<string, string?>(EncryptSecret);
                var decryptionFunction = new Func<string, string?>(DecryptSecret);

                var secureAttributeTypeInfoResolver = new JsonHelpers.SecureJsonTypeInfoModifier(encryptionFunction, decryptionFunction);
                JsonSerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver() {
                    Modifiers = {
                        secureAttributeTypeInfoResolver.Modify
                    }
                };

                _secureAttributeSetupCompleted = true;
            } catch {
                string tag = $"{GetType().Name}::{nameof(Setup)}-{GetFilePath()}";
                /*Runtime.ApplicationLog.Debug(tag, 
                    $"Failed to set JsonSerializerOptions? Possible this was already done! _secureAttributeSetupCompleted? {_secureAttributeSetupCompleted}"
                    + Environment.NewLine
                    + ex.Message
                    + Environment.NewLine
                    + ex.StackTrace ?? "No stack trace."
                );*/
            }
        }
        #endregion

        public SecureJsonConfiguration() : base() {
            Setup();
        }

        public SecureJsonConfiguration(string filePath, IFileStorage fileStorage, IEncryptionProvider fileEncryption) : base(filePath, fileStorage, fileEncryption) {
            // Generates a new secrets encryption key if one is not already there.
            if (string.IsNullOrEmpty(_secretsKeyEncrypted)) {
                string newKey = Encryption.GenerateRandomString(32);
                _secretsSalt = Encryption.GenerateRandomBytes(32);
                SecretsKeyEncrypted = Encryption.EncryptDataProtector(UnifyRuntime.Current.ApplicationId, newKey);
            }

            Setup();
        }

        /// <summary>
        /// Decrypts a secret using the configuration's secret key.
        /// </summary>
        /// <param name="value">Data to decrypt.</param>
        /// <returns>Decrypted secret.</returns>
        public string? DecryptSecret(string value) {
            try {
                return Encryption.Decrypt(value, SecretsEncryptionKey);
            } catch (Exception ex) {
                string tag = $"{GetType().Name}::{nameof(DecryptSecret)}-{GetFilePath()}";
                UnifyRuntime.ApplicationLog.Error(tag, "Failed to decrypt secret, invalid key?");
                UnifyRuntime.ApplicationLog.Error(tag, ex.Message);
                UnifyRuntime.ApplicationLog.Error(tag, ex.StackTrace ?? "No stack trace.");
            }
            return null;
        }
        /// <summary>
        /// Encrypts a string using the configuration's secret key.
        /// </summary>
        /// <param name="value">Data to encrypt.</param>
        /// <returns>Encrypted secret.</returns>
        public string? EncryptSecret(string value) {
            try {
                return Encryption.Encrypt(value, SecretsEncryptionKey, SecretsProtections);
            } catch (Exception ex) {
                string tag = $"{GetType().Name}::{nameof(EncryptSecret)}-{GetFilePath()}";
                UnifyRuntime.ApplicationLog.Error(tag, "Failed to encrypt secret?");
                UnifyRuntime.ApplicationLog.Error(tag, ex.Message);
                UnifyRuntime.ApplicationLog.Error(tag, ex.StackTrace ?? "No stack trace.");
            }
            return null;
        }
    }
}
