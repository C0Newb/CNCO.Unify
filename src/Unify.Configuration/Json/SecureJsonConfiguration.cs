﻿using CNCO.Unify.Security;
using CNCO.Unify.Storage;
using System.Security;
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
                        var key = Encryption.DecryptDataProtector(Runtime.Current.ApplicationId, value);
                        var newSecretsKey = new SecureString();
                        foreach (char c in key)
                            newSecretsKey.AppendChar(c);
                        SecretsKey = newSecretsKey;
                    } catch (Exception ex) {
                        string tag = $"{GetType().Name}::{nameof(SecretsKeyEncrypted)}-{GetFilePath()}";
                        Runtime.ApplicationLog.Error(tag, "Failed to decrypt secrets_key, secrets potentially lost for good!");
                        Runtime.ApplicationLog.Error(tag, ex.Message);
                        Runtime.ApplicationLog.Error(tag, ex.StackTrace ?? "Not stack trace.");

                        string newKey = Encryption.GenerateRandomString(32);
                        var newSecretsKey = new SecureString();
                        foreach (char c in newKey)
                            newSecretsKey.AppendChar(c);
                        SecretsKey = newSecretsKey;

                        _secretsSalt = Encryption.GenerateRandomBytes(32);
                        _secretsKeyEncrypted = Encryption.EncryptDataProtector(Runtime.Current.ApplicationId, newKey);
                    }
                }
            }
        }
        private string? _secretsKeyEncrypted;


        /// <summary>
        /// Encryption key for secrets stored in the configuration.
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
        #endregion

        public SecureJsonConfiguration() : base() { }

        public SecureJsonConfiguration(string filePath, IFileStorage fileStorage, IFileEncryption fileEncryption) : base(filePath, fileStorage, fileEncryption) {
            // Generates a new secrets encryption key if one is not already there.
            if (string.IsNullOrEmpty(_secretsKeyEncrypted)) {
                string newKey = Encryption.GenerateRandomString(32);
                _secretsSalt = Encryption.GenerateRandomBytes(32);
                SecretsKeyEncrypted = Encryption.EncryptDataProtector(Runtime.Current.ApplicationId, newKey);
            }
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
                Runtime.ApplicationLog.Error(tag, "Failed to decrypt secret, invalid key?");
                Runtime.ApplicationLog.Error(tag, ex.Message);
                Runtime.ApplicationLog.Error(tag, ex.StackTrace ?? "Not stack trace.");
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
                Runtime.ApplicationLog.Error(tag, "Failed to encrypt secret?");
                Runtime.ApplicationLog.Error(tag, ex.Message);
                Runtime.ApplicationLog.Error(tag, ex.StackTrace ?? "Not stack trace.");
            }
            return null;
        }
    }
}
