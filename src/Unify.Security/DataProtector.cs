using System.Text;
using CNCO.Unify.Security.Credentials;

namespace CNCO.Unify.Security {
    /// <summary>
    /// Use this to protect (encrypt) secrets. Encrypted via AES256.
    /// </summary>
    public class DataProtector : DataProtectionProvider, IDataProtector {
        /// <summary>
        /// The "tag" for the key and salt stored using <see cref="ICredentialManager"/>.
        /// </summary>
        private readonly string _purpose;

        #region Key stuff
        // This is the "key" stored to the filesystem. Used to derive the actual encryption key.
        private byte[]? _sourceKey;
        private byte[] SourceKey {
            get {
                if (_sourceKey != null && _sourceKey.Length > 0)
                    return _sourceKey;
                _sourceKey = GetProtectionItem($"{_purpose}_key");
                return _sourceKey;
            }
        }

        // This is the salt stored to the filesystem, used to derive the actual encryption key.
        private byte[]? _sourceSalt;
        private byte[] SourceSalt {
            get {
                if (_sourceSalt != null && _sourceSalt.Length > 0)
                    return _sourceSalt;
                _sourceSalt = GetProtectionItem($"{_purpose}_salt");
                return _sourceSalt;
            }
        }


        /// <summary>
        /// Key used to protect/unprotect
        /// </summary>
        private byte[] Key {
            get => Encryption.DeriveKey(SourceKey, SourceSalt, 32);
        }

        /// <summary>
        /// Grabs a key (or salt) from the <see cref="ICredentialManager"/>.
        /// </summary>
        /// <param name="name">The "key" to grab. This should be <see cref="_purpose"/>_key or _salt.</param>
        /// <returns>Either the stored key or stored salt.</returns>
        private static byte[] GetProtectionItem(string name) {
            /*
            var task = SecureStorage.GetAsync(name);
            task.Wait();
            if (task.Result != null)
                return Convert.FromBase64String(task.Result);

            // generate and save item
            var protectionItem = Encryption.GenerateRandomBytes(32);
            string keyInBase64 = Convert.ToBase64String(protectionItem);
            SecureStorage.SetAsync(name, keyInBase64).Wait();

            return protectionItem;
            */
            return Encoding.UTF8.GetBytes("pineapple :)"); // secure, decided by random toss
        }
        #endregion


        public DataProtector(string purpose) {
            _purpose = purpose;
        }

        public override IDataProtector CreateProtector(string purpose) => base.CreateProtector($"{_purpose}:{purpose}");


        public virtual byte[] Protect(byte[] plaintext) => Encryption.EncryptAES256_CBC(plaintext, Key);

        public virtual string Protect(string plaintext) {
            string protectedString = Encryption.EncryptAES256_CBC(plaintext, Key);
            byte[] protectedBytes = Encoding.UTF8.GetBytes(protectedString);
            return Convert.ToBase64String(protectedBytes);
        }


        public virtual byte[] Unprotect(byte[] protectedData) => Encryption.DecryptAES(protectedData, Key);
        public virtual string Unprotect(string protectedData) {
            byte[] protectedBytes = Convert.FromBase64String(protectedData);
            string protectedString = Encoding.UTF8.GetString(protectedBytes);
            return Encryption.DecryptAES(protectedString, Key);
        }
    }
}
