using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace CNCO.Unify.Security {
    /// <summary>
    /// Encryption helper functions.
    /// </summary>
    public static class Encryption {

        /// <summary>
        /// Encryption methods.
        /// </summary>
        /// <remarks>
        /// AES256_CBC will be, generally speaking, the most secure for most applications.
        /// ChatCha20Poly1305 is not widely supported!
        /// DataProtection has less portability!
        /// GCM can be compromised if the same nonce is used ever!
        /// </remarks>
        [Flags]
        public enum Protections {
            /// <summary>
            /// Will not use any encryption.
            /// </summary>
            None = 0x0,

            /// <summary>
            /// Use <see cref="DataProtectionProvider"/> for encryption.
            /// </summary>
            DataProtection = 0x1,

            /// <summary>
            /// Use ChaCha20-Poly1305 for encryption.
            /// </summary>
            /// <remarks>
            /// This is not widely supported! Please use a different protection unless you are for sure ChaCha20 is supported in your environment.
            /// </remarks>
            ChaCha20Poly1305 = 0x2,

            /// <summary>
            /// Use 128 bit AES (CBC mode) for encryption.
            /// </summary>
            AES128_CBC = 0x4,

            /// <summary>
            /// Use 128 bit AES (CBC mode) for encryption.
            /// </summary>
            AES256_CBC = 0x8,

            /// <summary>
            /// Use AES (GCM/AEAD mode) for encryption.
            /// </summary>
            AES128_GCM = 0x10,

            /// <summary>
            /// Use AES (GCM/AEAD mode) for encryption.
            /// </summary>
            /// <remarks>
            /// Same as <see cref="Protections.AES128_GCM"/>.
            /// </remarks>
            AES256_GCM = 0x20,
        }

        private static readonly object _dpLock = new object();
        private static DataProtector? _dataProtector;
        private static DataProtector DataProtector {
            get {
                if (_dataProtector == null) {
                    lock (_dpLock) {
                        _dataProtector ??= new DataProtector($"Unify:DataProtector:");
                    }
                }
                return _dataProtector;
            }
        }

        #region Encryption


        #region ChaCha20Poly1305
        public static bool SupportsChaCha20Poly1305() {
#if IOS
            return false;
#else
            return ChaCha20Poly1305.IsSupported;
#endif
        }

#if !IOS
        /// <summary>
        /// Encrypts a string using ChaCha20Poly1305.
        /// Generates a string that includes the associated data, cipher (encrypted) text, tag and none separated by a dollar sign ($).
        /// The associate data is stored in plain text, but will be validated during data decryption.
        /// <br></br>
        /// So, like so: <c>{version}$chacha20-poly1305${associated data}${cipher text}${none}${tag}</c>
        /// </summary>
        /// <remarks>
        /// Associated data and the cipher text will be encoded as base64, nonce and tag will be encoded as hex.
        /// 
        /// This is only supported on Windows 11.
        /// </remarks>
        /// <param name="plainText">Data to encrypt</param>
        /// <param name="key">Encryption key, must be size 32 (256 bit)</param>
        /// <param name="nonce">Unique 12 byte (96 bit) array. This MUST be unique and different each time if you reuse the same key!</param>
        /// <param name="associatedData">Data that will not be encrypted, but will be included in the final output and will be validate to ensure it has not been modified.</param>
        /// <returns>Encrypted string following this format: <c>{version}$chacha20-poly1305${associated data}${cipher text}${none}${tag}</c></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static string EncryptChaCha20Poly1305(string plainText, byte[] key, byte[] nonce, byte[] associatedData) {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (nonce == null)
                throw new ArgumentNullException(nameof(nonce));

            if (key.Length != 32)
                throw new ArgumentException("Key must be 32 bytes (256 bit).", nameof(key));
            if (nonce.Length != 12)
                throw new ArgumentException("Nonce must be 12 bytes (96 bit).", nameof(nonce));

            ChaCha20Poly1305 cipher = new(key);

            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherText = new byte[plainTextBytes.Length];
            byte[] tag = new byte[16];
            cipher.Encrypt(nonce, plainTextBytes, cipherText, tag, associatedData);
            cipher.Dispose();

            string encryptedString = "1$chacha20-poly1305";
            encryptedString += "$" + Convert.ToBase64String(associatedData);
            encryptedString += "$" + Convert.ToBase64String(cipherText);
            encryptedString += "$" + Convert.ToHexString(nonce);
            encryptedString += "$" + Convert.ToHexString(tag);

            GC.Collect();

            return encryptedString;
        }

        /// <summary>
        /// Decrypts an encrypted string produced by <see cref="EncryptChaCha20Poly1305(string, byte[], byte[], byte[])"/>.
        /// Encrypted data string must follow this format: <c>{version}$chacha20-poly1305${associated data}${cipher text}${none}${tag}</c>
        /// </summary>
        /// <param name="encryptedData">Encrypted string (<c>{version}$chacha20-poly1305${associated data}${cipher text}${none}${tag}</c>)</param>
        /// <param name="key">Decryption key. Must be of size 32!</param>
        /// <returns>Decrypted data, the decryption of cipher text</returns>
        /// <exception cref="ArgumentNullException">Null arguments</exception>
        /// <exception cref="ArgumentException">Invalid encryptedData string</exception>
        public static string DecryptChaCha20Poly1305(string encryptedData, byte[] key) {
            if (string.IsNullOrEmpty(encryptedData))
                throw new ArgumentNullException(nameof(encryptedData));
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (key.Length != 32)
                throw new ArgumentException("Key must be of size 32.", nameof(key));

            string[] encryptedDataParts = encryptedData.Split('$');
            if (encryptedDataParts.Length != 6)
                throw new ArgumentException(nameof(encryptedData) + " must have 6 parts separated by dollar signs ($).");

            // {associated data}${cipher text}${none}${tag}
            //string versionString = encryptedDataParts[0];
            string usedCipher = encryptedDataParts[1];
            if (!string.Equals(usedCipher, "chacha20-poly1305", StringComparison.OrdinalIgnoreCase)) {
                throw new ArgumentOutOfRangeException("Data encrypted using \"" + usedCipher + "\" not ChaCha20-Poly1305.");
            }

            byte[] associatedData = Convert.FromBase64String(encryptedDataParts[2]);
            byte[] cipherText = Convert.FromBase64String(encryptedDataParts[3]);
            byte[] nonce = Convert.FromHexString(encryptedDataParts[4]);
            byte[] tag = Convert.FromHexString(encryptedDataParts[5]);

            if (nonce.Length != 12)
                throw new NullReferenceException(nameof(encryptedData) + " is invalid, nonce must be 12 bytes (96 bit)");
            if (tag.Length != 16)
                throw new NullReferenceException(nameof(encryptedData) + " is invalid, tag must be 16 bytes (128 bit)");

            byte[] plainText = new byte[cipherText.Length];
            using (ChaCha20Poly1305 cipher = new(key)) {
                cipher.Decrypt(nonce, cipherText, tag, associatedData);
            };

            return Encoding.UTF8.GetString(plainText);
        }
#endif
        #endregion


        #region AES
        /// <summary>
        /// Encrypts a string and returns the encrypted string as a combination of the cipher (encrypted text) and iv.
        /// Cipher is encoded in base64, iv in hex. Both separate by a dollar sign ($) like so: <c>{version}$aes{key size}.{cipher mode}${cipher text}${iv}</c>
        /// </summary>
        /// <remarks>
        /// This does NOT include AES GCM.
        /// </remarks>
        /// <param name="plainText">Data to encrypt</param>
        /// <param name="key">Secret key to encrypt the data with</param>
        /// <param name="iv">Initialization vector. If null, a random 128 bit one will be generated</param>
        /// <param name="keySize">Key size to use, such as 128 (for AES 128) or 256 (for AES 256).</param>
        /// <param name="cipherMode">Which cipher mode to encrypt via.</param>
        /// <returns>Encrypted data string: <c>{version}$aes{key size}.{cipher mode}${cipher text}${iv}</c></returns>
        /// <exception cref="ArgumentNullException">An argument is null or of length 0 or less.</exception>
        public static string EncryptAes(string plainText, byte[] key, byte[]? iv, int keySize = 256, CipherMode cipherMode = CipherMode.CBC) {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException(nameof(plainText));
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException(nameof(key));
            if (iv == null || iv.Length != 16)
                iv = GenerateRandomBytes(16); // 16 bytes for AES-256

            byte[] cipherText;

            using (Aes aesAlg = Aes.Create()) {
                aesAlg.KeySize = keySize;
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.Mode = cipherMode;

                aesAlg.Key = key;
                aesAlg.IV = iv;
                ICryptoTransform cryptoTransform = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using MemoryStream msEncrypt = new MemoryStream();
                using CryptoStream csEncrypt = new CryptoStream(msEncrypt, cryptoTransform, CryptoStreamMode.Write);
                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt)) {
                    swEncrypt.Write(plainText);
                }
                cipherText = msEncrypt.ToArray();
            }

            GC.Collect();

            string encryptedData = $"1$aes{keySize}.{cipherMode}";
            encryptedData += "$" + Convert.ToBase64String(cipherText);
            encryptedData += "$" + Convert.ToHexString(iv);
            return encryptedData;
        }
        /// <inheritdoc cref="EncryptAes(string, byte[], byte[], int, CipherMode)"/>
        /// <remarks>
        /// This first converts <paramref name="plainText"/> to base64, encrypts, then converts the encrypted string to bytes via <see cref="Encoding.UTF8"/>
        /// </remarks>
        public static byte[] EncryptAes(byte[] plainText, byte[] key, byte[]? iv, int keySize = 256, CipherMode cipherMode = CipherMode.CBC) {
            var str = Convert.ToBase64String(plainText);
            var encryptedStr = EncryptAes(str, key, iv, keySize, cipherMode);
            return Encoding.UTF8.GetBytes(encryptedStr);
        }

        /// <inheritdoc cref="EncryptAes(string, byte[], byte[], int, CipherMode)"/>
        /// <returns>Encrypted data string: <c>{version}$aes256.CBC${cipher text}${iv}</c></returns>
        public static string EncryptAes256_Cbc(string plainText, byte[] key, byte[]? iv = null) => EncryptAes(plainText, key, iv, 256, CipherMode.CBC);

        /// <inheritdoc cref="EncryptAes256_Cbc(string, byte[], byte[])" />
        public static byte[] EncryptAes256_Cbc(byte[] plainText, byte[] key, byte[]? iv = null) => EncryptAes(plainText, key, iv, 256, CipherMode.CBC);


        /// <inheritdoc cref="EncryptAes(string, byte[], byte[], int, CipherMode)"/>
        /// <returns>Encrypted data string: <c>{version}$aes128.CBC${cipher text}${iv}</c></returns>
        public static string EncryptAes128_Cbc(string plainText, byte[] key, byte[]? iv) => EncryptAes(plainText, key, iv, 128, CipherMode.CBC);
        /// <inheritdoc cref="EncryptAes128_Cbc(string, byte[], byte[])" />
        public static byte[] EncryptAes128_Cbc(byte[] plainText, byte[] key, byte[]? iv = null) => EncryptAes(plainText, key, iv, 128, CipherMode.CBC);

        /// <summary>
        /// Decrypts a encrypted data string string and returns the decrypted data.
        /// Encrypted data string must follow this format: <c>{version}$aes256${cipher text}${iv}</c>
        /// </summary>
        /// <param name="encryptedData">Base64 string to decrypt, must follow this format: <c>{version}$aes256${cipher text}${iv}</c></param>
        /// <param name="key">Secret key to encrypt the data with</param>
        /// <returns>Encrypted data as base64 string</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string DecryptAes(string encryptedData, byte[] key) {
            // Check arguments.
            if (encryptedData == null)
                throw new ArgumentNullException(nameof(encryptedData));
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException(nameof(key));

            string[] encryptedDataParts = encryptedData.Split('$');
            if (encryptedDataParts.Length != 4)
                throw new ArgumentException(nameof(encryptedData) + " must have 4 parts separated by dollar signs ($).");

            // {version}${cipher}${cipher text}${iv}
            //string versionString = encryptedDataParts[0];
            string cipher = encryptedDataParts[1];
            if (!cipher.StartsWith("aes", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentOutOfRangeException("Data encrypted using \"" + cipher + "\" not AES.");
            var cipherParts = cipher.Split('.');

            // Determine which type of AES
            if (cipherParts.Length != 2
                || !int.TryParse(cipherParts[0][3..], out int keySize)
                || !Enum.TryParse(cipherParts[1], true, out CipherMode mode)) {
                throw new ArgumentOutOfRangeException("Unable to determine the key size or cipher mode of \"" + cipher + "\".");
            }

            byte[] cipherText = Convert.FromBase64String(encryptedDataParts[2]);
            byte[] iv = Convert.FromHexString(encryptedDataParts[3]);

            if (iv == null || iv.Length <= 0)
                throw new NullReferenceException(nameof(iv));

            string plainText = "";
            using (Aes aesAlg = Aes.Create()) {
                aesAlg.KeySize = keySize;
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.Mode = mode;

                aesAlg.Key = key;
                aesAlg.IV = iv;

                using MemoryStream msDecrypt = new MemoryStream(cipherText);
                using CryptoStream csDecrypt = new CryptoStream(msDecrypt, aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV), CryptoStreamMode.Read);
                using StreamReader srDecrypt = new StreamReader(csDecrypt);
                plainText = srDecrypt.ReadToEnd();
            }

            return plainText;
        }

        /// <inheritdoc cref="DecryptAes(string, byte[])"/>
        /// <remarks>
        /// This first uses <see cref="Encoding.UTF8"/> to get a string from <paramref name="encryptedData"/>, decrypts, then converts the encrypted string to bytes from base64.
        /// </remarks>
        public static byte[] DecryptAes(byte[] encryptedData, byte[] key) {
            var utf8 = new UTF8Encoding(false, true);
            var encryptedStr = utf8.GetString(encryptedData);
            var str = DecryptAes(encryptedStr, key);
            return Convert.FromBase64String(str);
        }
        #endregion

        #region AES GCM
        /// <summary>
        /// Encrypts a string and returns the encrypted string as a combination of the cipher (encrypted text) and iv.
        /// Cipher is encoded in base64, iv in hex. Both separate by a dollar sign ($) like so: <c>{version}$aes128.GCM${cipher text}${nonce}${tag}${associateData}</c>
        /// </summary>
        /// <remarks>
        /// If associate data is empty, 64 random bytes will be used.
        /// </remarks>
        /// <param name="plainText">Data to encrypt</param>
        /// <param name="key">Secret key to encrypt the data with</param>
        /// <param name="nonce">Random nonce (IV) to use. If blank, a random nonce of the the largest possible size will be generated.</param>
        /// <param name="associateData">Associated data to use to generate the tag. <see href="https://en.wikipedia.org/wiki/Authenticated_encryption"/>.</param>
        /// <param name="tagSize">How large the tag will be.</param>
        /// <returns>Encrypted data string: <c>{version}$aes.GCM${cipher text}${nonce}${tag}${associateData}</c></returns>
        /// <exception cref="ArgumentNullException">An argument is null or of length 0 or less.</exception>
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("tvos")]
        [UnsupportedOSPlatform("ios")]
        public static string EncryptAes_Gcm(byte[] plainText, byte[] key, byte[]? nonce, byte[]? associateData, int tagSize = -1) {
            // unsupported platforms
            if (Platform.IsTvOS() || Platform.IsIOS() || Platform.IsBrowser() || !AesGcm.IsSupported)
                throw new PlatformNotSupportedException();

            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException(nameof(plainText));
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException(nameof(key));

            if (tagSize == -1)
                tagSize = AesGcm.TagByteSizes.MaxSize;
            if (tagSize < AesGcm.TagByteSizes.MinSize)
                throw new ArgumentException($"Tag size too small (<{AesGcm.TagByteSizes.MinSize}).", nameof(tagSize));
            if (tagSize > AesGcm.TagByteSizes.MaxSize)
                throw new ArgumentException($"Tag size too too large (>{AesGcm.TagByteSizes.MaxSize}).", nameof(tagSize));

            nonce ??= GenerateRandomBytes(AesGcm.NonceByteSizes.MaxSize);
            if (nonce.Length < AesGcm.NonceByteSizes.MinSize)
                throw new ArgumentException($"Nonce size too small (<{AesGcm.NonceByteSizes.MinSize}).");
            if (nonce.Length > AesGcm.NonceByteSizes.MaxSize)
                throw new ArgumentException($"Nonce sie is too large (>{AesGcm.NonceByteSizes.MaxSize}).", nameof(nonce));


            if (associateData == null || associateData.Length <= 0)
                associateData = GenerateRandomBytes(64);

            byte[] cipherText = new byte[plainText.Length];
            byte[] tag = new byte[tagSize];

            using (AesGcm aes = new AesGcm(key, tagSize)) {
                aes.Encrypt(nonce, plainText, cipherText, tag, associateData);
            }

            CryptographicOperations.ZeroMemory(plainText);

            GC.Collect();

            string cipherTextBase64 = Convert.ToBase64String(cipherText);
            string nonceHex = Convert.ToHexString(nonce);
            string tagHex = Convert.ToHexString(tag);
            string associatedDataHex = Convert.ToHexString(associateData ?? []);

            int size = cipherTextBase64.Length + nonceHex.Length + tagHex.Length + 12; // 2 $'s, 10 for ver+aes.GCM
            if (associateData != null)
                size += associatedDataHex.Length + 1;

            StringBuilder encryptedData = new StringBuilder("1$aes.GCM$", size);
            encryptedData.Append(cipherTextBase64);
            encryptedData.Append('$');
            encryptedData.Append(nonceHex);
            encryptedData.Append('$');
            encryptedData.Append(tagHex);
            if (associateData != null) {
                encryptedData.Append('$');
                encryptedData.Append(associatedDataHex);
            }

            return encryptedData.ToString();
        }

        /// <inheritdoc cref="EncryptAes_Gcm(byte[], byte[], byte[], byte[], int)"/>
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("tvos")]
        [UnsupportedOSPlatform("ios")]
        public static string EncryptAes_Gcm(byte[] plainText, byte[] key, byte[]? nonce, string? associateData, int tagSize = -1) {
            var utf8 = new UTF8Encoding(false, true);
            byte[]? associatedDataBytes = null;
            if (associateData != null) {
                associatedDataBytes = utf8.GetBytes(associateData);
            }

            string cipherText = EncryptAes_Gcm(plainText, key, nonce, associatedDataBytes, tagSize);

            return cipherText;
        }

        /// <inheritdoc cref="EncryptAes_Gcm(byte[], byte[], byte[], byte[], int)"/>
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("tvos")]
        [UnsupportedOSPlatform("ios")]
        public static string EncryptAes_Gcm(string plainText, byte[] key, byte[]? nonce, byte[]? associateData, int tagSize = -1) {
            var utf8 = new UTF8Encoding(false, true);
            byte[] plainTextBytes = utf8.GetBytes(plainText);
            string cipherText = EncryptAes_Gcm(plainTextBytes, key, nonce, associateData, tagSize);
            CryptographicOperations.ZeroMemory(plainTextBytes);
            return cipherText;
        }

        /// <inheritdoc cref="EncryptAes_Gcm(byte[], byte[], byte[], byte[], int)"/>
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("tvos")]
        [UnsupportedOSPlatform("ios")]
        public static string EncryptAes_Gcm(string plainText, byte[] key, byte[]? nonce, string? associateData = null, int tagSize = -1) {
            var utf8 = new UTF8Encoding(false, true);
            byte[] plainTextBytes = utf8.GetBytes(plainText);
            byte[]? associatedDataBytes = null;
            if (associateData != null) {
                associatedDataBytes = utf8.GetBytes(associateData);
            }

            string cipherText = EncryptAes_Gcm(plainTextBytes, key, nonce, associatedDataBytes, tagSize);

            CryptographicOperations.ZeroMemory(plainTextBytes);
            return cipherText;
        }


        /// <summary>
        /// Decrypts a encrypted data string string and returns the decrypted data.
        /// Encrypted data string must follow this format: <c>{version}$aes.GCM${cipher text}${nonce}${tag}${associateData}</c>
        /// </summary>
        /// <param name="encryptedData">Base64 string to decrypt, must follow this format: <c>{version}$aes.GCM${cipher text}${nonce}${tag}${associateData}</c></param>
        /// <param name="key">Secret key to encrypt the data with</param>
        /// <param name="associatedData">Associate data to use. This ignores whatever associate data is in the encrypted data string.</param>
        /// <returns>Encrypted data as base64 string</returns>
        /// <exception cref="ArgumentNullException"></exception>
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("tvos")]
        [UnsupportedOSPlatform("ios")]
        public static byte[] DecryptAes_Gcm(string encryptedData, byte[] key, byte[]? associatedData) {
            // unsupported platforms
            if (Platform.IsTvOS() || Platform.IsIOS() || Platform.IsBrowser())
                throw new PlatformNotSupportedException();

            // Check arguments.
            ArgumentNullException.ThrowIfNull(encryptedData);
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException(nameof(key));

            string[] encryptedDataParts = encryptedData.Split('$');
            if (associatedData != null && encryptedDataParts.Length == 5)
                throw new CryptographicException("Associated data mismatch. No associated data within cipher text, but " + nameof(associatedData) + " was supplied.");
            if (encryptedDataParts.Length != 6)
                throw new ArgumentException(nameof(encryptedData) + " must have 6 parts separated by dollar signs ($).");

            // {version}$aes.GCM${cipher text}${nonce}${tag}${associateData}
            //string versionString = encryptedDataParts[0];

            string cipher = encryptedDataParts[1];
            if (!cipher.StartsWith("aes", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentOutOfRangeException("Data encrypted using \"" + cipher + "\" not AES.");
            var cipherParts = cipher.Split('.');
            // Determine which type of AES
            if (cipherParts.Length != 2 || !cipherParts[1].Equals("GCM", StringComparison.OrdinalIgnoreCase)) {
                throw new ArgumentOutOfRangeException("Unable to determine cipher mode of \"" + cipher + "\".");
            }

            byte[] cipherText = Convert.FromBase64String(encryptedDataParts[2]);
            byte[] nonce = Convert.FromHexString(encryptedDataParts[3]);
            byte[] tag = Convert.FromHexString(encryptedDataParts[4]);
            if (encryptedDataParts.Length == 6) {
                byte[] ADBytes = Convert.FromHexString(encryptedDataParts[5]);
                if (associatedData != null && associatedData != ADBytes) {
                    throw new CryptographicException("Associated data provided does not match associated data in cipher text.");
                }
                associatedData = ADBytes;
            }

            if (nonce == null || nonce.Length <= 0)
                throw new NullReferenceException($"{nameof(nonce)} was not found or is empty.");
            if (tag == null || tag.Length <= 0)
                throw new NullReferenceException($"{nameof(tag)} was not found or is empty.");

            byte[] plainTextBytes = new byte[cipherText.Length];
            using (AesGcm aes = new AesGcm(key, tag.Length)) {
                aes.Decrypt(nonce, cipherText, tag, plainTextBytes, associatedData);
            }

            return plainTextBytes;
        }

        /// <inheritdoc cref="DecryptAes_Gcm(string, byte[], byte[])"/>
        /// <remarks>
        /// This first uses <see cref="Encoding.UTF8"/> to get a string from <paramref name="encryptedData"/>, decrypts, then converts the encrypted string to bytes from base64.
        /// </remarks>
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("tvos")]
        [UnsupportedOSPlatform("ios")]
        public static byte[] DecryptAes_Gcm(byte[] encryptedData, byte[] key, byte[]? associatedData = null) {
            var utf8 = new UTF8Encoding(false, true);
            var encryptedStr = utf8.GetString(encryptedData);
            return DecryptAes_Gcm(encryptedStr, key, associatedData);
        }

        /// <inheritdoc cref="DecryptAes_Gcm(string, byte[], byte[])"/>
        /// <remarks>
        /// This uses <see cref="Encoding.UTF8"/> to get converts the encrypted bytes to string.
        /// </remarks>
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("tvos")]
        [UnsupportedOSPlatform("ios")]
        public static string DecryptAesString_Gcm(string encryptedData, byte[] key, byte[]? associatedData = null) {
            var utf8 = new UTF8Encoding(false, true);
            byte[] bytes = DecryptAes_Gcm(encryptedData, key, associatedData);
            return utf8.GetString(bytes);
        }

        /// <inheritdoc cref="DecryptAes_Gcm(string, byte[], byte[])"/>
        /// <remarks>
        /// This uses <see cref="Encoding.UTF8"/> to get converts the encrypted bytes to string.
        /// </remarks>
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("tvos")]
        [UnsupportedOSPlatform("ios")]
        public static string DecryptAesString_Gcm(byte[] encryptedData, byte[] key, byte[]? associatedData = null) {
            var utf8 = new UTF8Encoding(false, true);
            var encryptedStr = utf8.GetString(encryptedData);
            byte[] bytes = DecryptAes_Gcm(encryptedStr, key, associatedData);
            return utf8.GetString(bytes);
        }
        #endregion


        #region ASP.Net Data Protector
        /// <summary>
        /// Uses <see cref="IDataProtector"/> to encrypt data (<c>{version}$unifydp${encrypted data}</c>).
        /// </summary>
        /// <param name="plainText">Data to encrypt.</param>
        /// <returns>Encrypt data string in the form of <c>{version}$unifydp${encrypted data}</c></returns>
        public static string EncryptDataProtector(string plainText) => EncryptDataProtector(DataProtector, plainText);

        /// <inheritdoc cref="EncryptDataProtector(string)"/>
        /// <param name="dataProtectorPurpose">The purpose provided to <see cref="Security.DataProtector(string)"/></param>
        public static string EncryptDataProtector(string dataProtectorPurpose, string plainText) => EncryptDataProtector(new DataProtector(dataProtectorPurpose), plainText);

        /// <inheritdoc cref="EncryptDataProtector(string)"/>
        /// <param name="dataProtector">The <see cref="IDataProtector"/> to use for data protection.</param>
        public static string EncryptDataProtector(IDataProtector dataProtector, string plainText) => $"1$unifydp${dataProtector.Protect(plainText)}";


        /// <summary>
        /// Uses <see cref="IDataProtector"/> to decrypt data (<c>{version}$unifydp${encrypted data}</c>)
        /// </summary>
        /// <param name="protectedText">Data to decrypt (<c>{version}$unifydp${encrypted data}</c>)</param>
        /// <returns>Decrypted data string</returns>
        public static string DecryptDataProtector(string protectedText) => DecryptDataProtector(DataProtector, protectedText);


        /// <inheritdoc cref="DecryptDataProtector(string)"/>
        /// <param name="dataProtectorPurpose">The purpose provided to <see cref="Security.DataProtector(string)"/></param>
        public static string DecryptDataProtector(string dataProtectorPurpose, string protectedText) => DecryptDataProtector(new DataProtector(dataProtectorPurpose), protectedText);


        /// <inheritdoc cref="DecryptDataProtector(string)"/>
        /// <param name="dataProtector">The <see cref="IDataProtector"/> to use for data protection.</param>
        public static string DecryptDataProtector(IDataProtector dataProtector, string protectedText) {
            string[] encryptedDataParts = protectedText.Split('$');
            if (encryptedDataParts.Length != 3)
                throw new ArgumentException(nameof(protectedText) + " must have 3 parts separated by dollar signs ($).");

            //string versionString = encryptedDataParts[0];
            string cipher = encryptedDataParts[1];
            if (!string.Equals(cipher, "unifydp", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentOutOfRangeException("Data encrypted using \"" + cipher + "\" not the Unify Data Protector.");

            string cipherText = encryptedDataParts[2];
            return dataProtector.Unprotect(cipherText);
        }
        #endregion


        #endregion



        #region Hashing / key deriving / IV generation
        /// <summary>
        /// Generates a byte array of <paramref name="size"/>.
        /// </summary>
        /// <param name="size">Number of random bytes to generate</param>
        /// <returns>Byte array filled with random bytes from <see cref="Random.NextBytes(byte[])"/></returns>
        public static byte[] GenerateRandomBytes(int size = 1) {
            byte[] bytes = new byte[size];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create()) {
                rng.GetBytes(bytes);
            }
            return bytes;
        }

        /// <summary>
        /// Generates a random string of <paramref name="size"/>.
        /// 
        /// String will contain the following characters, unless blocked by the <paramref name="blacklistedCharacters"/> parameter:
        /// <c><![CDATA[ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+-=[]{}|;:,.<>?]]></c>
        /// </summary>
        /// <param name="size">Length of the new string.</param>
        /// <param name="blacklistedCharacters"></param>
        /// <returns></returns>
        public static string GenerateRandomString(int size = 32, string? blacklistedCharacters = null) {
            string allowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+-=[]{}|;:,.<>?";
            string filteredCharacters = allowedCharacters;
            if (!string.IsNullOrEmpty(blacklistedCharacters))
                filteredCharacters = new string(allowedCharacters.Except(blacklistedCharacters).ToArray());

            StringBuilder randomString = new StringBuilder(size);
            byte[] randomBytes = new byte[size];

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create()) {
                rng.GetBytes(randomBytes);
            }

            foreach (byte randomByte in randomBytes) {
                int randomIndex = randomByte % filteredCharacters.Length;
                randomString.Append(filteredCharacters[randomIndex]);
            }

            return randomString.ToString();
        }



        /// <summary>
        /// Generates a BCrypt hash from a string given a work factor.
        /// </summary>
        /// <param name="password">Data to hash</param>
        /// <param name="workFactor">BCrypt work factor</param>
        /// <returns>The hashed password.</returns>
        public static string GenerateBCryptHash(SecureString password, int workFactor) {
            IntPtr bstr = Marshal.SecureStringToBSTR(password);
            try {
                return BCrypt.Net.BCrypt.EnhancedHashPassword(Marshal.PtrToStringBSTR(bstr), workFactor);
            } finally {
                Marshal.ZeroFreeBSTR(bstr);
            }
        }

        /// <inheritdoc cref="GenerateBCryptHash(SecureString, int)"/>
        public static string GenerateBCryptHash(string password, int workFactor) => BCrypt.Net.BCrypt.EnhancedHashPassword(password, workFactor);


        /// <summary>
        /// Used to verify a BCrypt hash was derived from a given password
        /// </summary>
        /// <param name="password">Data to verify</param>
        /// <param name="hash">Hash to verify against</param>
        /// <returns>Whether the hash is generated from the password.</returns>
        public static bool VerifyBCryptHash(SecureString password, string hash) {
            IntPtr bstr = Marshal.SecureStringToBSTR(password);
            try {
                return BCrypt.Net.BCrypt.EnhancedVerify(Marshal.PtrToStringBSTR(bstr), hash);
            } finally {
                Marshal.ZeroFreeBSTR(bstr);
            }
        }

        /// <inheritdoc cref="VerifyBCryptHash(SecureString, string)"/>
        public static bool VerifyBCryptHash(string password, string hash) => BCrypt.Net.BCrypt.EnhancedVerify(password, hash);


        /// <summary>
        /// Uses <see cref="Rfc2898DeriveBytes"/> to derive an encryption key given the password and salt.
        /// </summary>
        /// <param name="password">User's encryption password</param>
        /// <param name="salt">Password salt</param>
        /// <param name="size">Final key length</param>
        /// <param name="iterations">Number of iterations used</param>
        /// <returns>Key derived from the user's password</returns>
        public static byte[] DeriveKey(SecureString password, byte[] salt, int size = 32, int iterations = 100000) {
            IntPtr bstr = Marshal.SecureStringToBSTR(password);

            byte[] key = new byte[size];
            try {
                string plainTextPassword = Marshal.PtrToStringBSTR(bstr);
                using (var deriveBytes = new Rfc2898DeriveBytes(plainTextPassword, salt, iterations, HashAlgorithmName.SHA512)) {
                    key = deriveBytes.GetBytes(size);
                }
                return key;
            } finally {
                Marshal.ZeroFreeBSTR(bstr);
            }
        }

        /// <summary>
        /// Uses <see cref="Rfc2898DeriveBytes"/> to derive an encryption key given the password and salt.
        /// </summary>
        /// <param name="password">User's encryption password</param>
        /// <param name="salt">Password salt</param>
        /// <param name="iterations">Number of iterations used</param>
        /// <param name="size">Final key length</param>
        /// <returns>Key derived from the user's password</returns>
        public static byte[] DeriveKey(byte[] password, byte[] salt, int size = 32, int iterations = 100000) {
            byte[] key = new byte[size];
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA512)) {
                key = deriveBytes.GetBytes(size);
            }
            return key;
        }
        #endregion


        /// <summary>
        /// Encrypts a string using one or more <see cref="Protections"/>. If multiple protections are selected, the encrypted data is passed from one into the next in a layered fashion.
        /// </summary>
        /// <remarks>
        /// The protections are layered like so: <see cref="Protections.ChaCha20Poly1305"/> -> <see cref="Protections.AES256_CBC"/> -> <see cref="Protections.DataProtection"/>.
        /// </remarks>
        /// <param name="plainText">Data to encrypt.</param>
        /// <param name="key">Encryption key. A key will be derived if your key is not the correct length (32).</param>
        /// <param name="protectionsToUse">Encryption methods to apply</param>
        /// <param name="nonce">If using ChaCha20-Poly1305, this is the required 96 bit (12 bytes) nonce to be used. Must be unique each time!</param>
        /// <param name="iv">Optional iv to provide to the AES protector. If null, a 128 bit one will be generated</param>
        /// <returns>Encrypted data string from the last protector</returns>
        /// <exception cref="ArgumentException"></exception>
        public static string Encrypt(string plainText, byte[] key, Protections protectionsToUse, byte[]? nonce = null, byte[]? associationData = null, byte[]? iv = null) {
            string cipherText = plainText;

            byte[] actualKey = new byte[32];
            if (key.Length != 32) {
                actualKey = DeriveKey(key, Array.Empty<byte>(), 32, 50000);
            } else {
                Array.Copy(key, actualKey, 32);
            }


            if (protectionsToUse.HasFlag(Protections.AES256_CBC))
                cipherText = EncryptAes256_Cbc(cipherText, actualKey, iv);

            if (protectionsToUse.HasFlag(Protections.AES128_CBC))
                cipherText = EncryptAes128_Cbc(cipherText, actualKey, iv);

#if !IOS
            if ((protectionsToUse.HasFlag(Protections.AES256_GCM) || protectionsToUse.HasFlag(Protections.AES128_GCM)) && AesGcm.IsSupported) {
                cipherText = EncryptAes_Gcm(cipherText, actualKey, nonce, associationData);
            }

            if (protectionsToUse.HasFlag(Protections.ChaCha20Poly1305) && ChaCha20Poly1305.IsSupported) {
                if (nonce == null || nonce.Length != 12)
                    throw new ArgumentException("Nonce must be 12 bytes (96 bit)", nameof(nonce));

                cipherText = EncryptChaCha20Poly1305(cipherText, actualKey, nonce, associationData ?? Array.Empty<byte>());
            }
#endif

            //actualKey = GenerateRandomBytes(32);

            if (protectionsToUse.HasFlag(Protections.DataProtection))
                cipherText = EncryptDataProtector(cipherText);

            return cipherText;
        }

        /// <summary>
        /// Decrypts cipher text using one or more <see cref="Protections"/> depending on what was used in encryption.
        /// </summary>
        /// <param name="cipherText">Data to decrypt</param>
        /// <param name="key">Decryption key</param>
        /// <param name="associatedData">Data that must match the associate data on the <paramref name="cipherText"/>.</param>
        /// <returns></returns>
        public static string Decrypt(string cipherText, byte[] key, byte[]? associatedData = null) {
            string[] parts;
            bool loop = true;

            byte[] actualKey = new byte[32];
            if (key.Length != 32) {
                actualKey = DeriveKey(key, Array.Empty<byte>(), 32, 50000);
            } else {
                Array.Copy(key, actualKey, 32);
            }

            while (loop) {
                if (string.IsNullOrEmpty(cipherText) || !cipherText.StartsWith("1$")) {
                    break;
                }

                parts = cipherText.Split("$");
                if (parts.Length < 2) {
                    break;
                }

                switch (parts[1]) {
#if !IOS
                    case "chacha20-poly1305":
                        cipherText = DecryptChaCha20Poly1305(cipherText, actualKey);
                        break;

                    case "aes256.GCM":
                    case "aes128.GCM":
                    case "aes.GCM":
                        cipherText = DecryptAesString_Gcm(cipherText, actualKey, associatedData);
                        break;
#endif

                    case "aes256.CBC":
                    case "aes128.CBC":
                        cipherText = DecryptAes(cipherText, actualKey);
                        break;

                    case "unifydp":
                        cipherText = DecryptDataProtector(cipherText);
                        break;

                    default:
                        loop = false;
                        break;
                }
            }
            return cipherText;
        }
    }
}
