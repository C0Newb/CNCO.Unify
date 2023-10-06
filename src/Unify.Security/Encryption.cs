using Microsoft.AspNetCore.DataProtection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace CNCO.Unify.Security {
    /// <summary>
    /// Encryption helper functions.
    /// </summary>
    public static class Encryption {
        [Flags]
        public enum Protections {
            None = 0x0,
            AspNetCoreDataProtection = 0x1,
            ChaCha20Poly1305 = 0x2,
            AES128_CBC = 0x4,
            AES256_CBC = 0x8,
            AES128_GCM = 0x10,
            AES256_GCM = 0x20,
        }

        #region Encryption


        #region ChaCha20Poly1305
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
            string versionString = encryptedDataParts[0];
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
        public static string EncryptAES(string plainText, byte[] key, byte[]? iv, int keySize = 256, CipherMode cipherMode = CipherMode.CBC) {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException(nameof(plainText));
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException(nameof(key));
            if (iv == null || iv.Length != 16)
                iv = GenerateRandomBytes(16); // 16 bytes for AES-256

            byte[] cipherText;

            using (Aes aesAlg = Aes.Create()) {
                aesAlg.KeySize = 128;
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

        /// <inheritdoc cref="EncryptAES"/>
        /// <returns>Encrypted data string: <c>{version}$aes256.CBC${cipher text}${iv}</c></returns>
        public static string EncryptAES256_CBC(string plainText, byte[] key, byte[]? iv) => EncryptAES(plainText, key, iv, 256, CipherMode.CBC);
        /// <inheritdoc cref="EncryptAES"/>
        /// <returns>Encrypted data string: <c>{version}$aes128.CBC${cipher text}${iv}</c></returns>
        public static string EncryptAES128_CBC(string plainText, byte[] key, byte[]? iv) => EncryptAES(plainText, key, iv, 128, CipherMode.CBC);

        /// <summary>
        /// Decrypts a encrypted data string string and returns the decrypted data.
        /// Encrypted data string must follow this format: <c>{version}$aes256${cipher text}${iv}</c>
        /// </summary>
        /// <param name="encryptedData">Base64 string to decrypt, must follow this format: <c>{version}$aes256${cipher text}${iv}</c></param>
        /// <param name="key">Secret key to encrypt the data with</param>
        /// <returns>Encrypted data as base64 string</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string DecryptAES(string encryptedData, byte[] key) {
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
        #endregion


        #region ASP.Net Data Protector
        /// <summary>
        /// Uses the ASP.Net Core data protector to encrypt data (<c>{version}$aspnet${encrypted data}</c>).
        /// </summary>
        /// <param name="plainText">Data to encrypt</param>
        /// <returns>Encrypt data string in the form of <c>{version}$aspnet${encrypted data}</c></returns>
        public static string EncryptAspNetCoreDataProtector(string plainText) {
            DataProtector dataProtector = GetAspNetCoreDataProtector();
            return "1$aspnet$" + dataProtector.Protect(plainText); // dead simple :p
        }

        /// <summary>
        /// Uses the ASP.Net Core data protector to decrypt data (<c>{version}$aspnet${encrypted data}</c>)
        /// </summary>
        /// <param name="encryptedData">Data to decrypt (<c>{version}$aspnet${encrypted data}</c>)</param>
        /// <returns>Decrypted data string</returns>
        public static string DecryptAspNetCoreDataProtector(string encryptedData) {
            string[] encryptedDataParts = encryptedData.Split('$');
            if (encryptedDataParts.Length != 3)
                throw new ArgumentException(nameof(encryptedData) + " must have 3 parts separated by dollar signs ($).");

            //string versionString = encryptedDataParts[0];
            string cipher = encryptedDataParts[1];
            if (!string.Equals(cipher, "aspnet", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentOutOfRangeException("Data encrypted using \"" + cipher + "\" not the ASP.Net Core Data Protector.");

            string cipherText = encryptedDataParts[2];
            DataProtector dataProtector = GetAspNetCoreDataProtector();
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
        /// Returns an instance of the ASP.Net Core data protector.
        /// </summary>
        /// <returns></returns>
        public static DataProtector GetAspNetCoreDataProtector() {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDataProtection();
            var services = serviceCollection.BuildServiceProvider();

            return ActivatorUtilities.CreateInstance<DataProtector>(services);
        }

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
                using (var deriveBytes = new Rfc2898DeriveBytes(plainTextPassword, salt, iterations)) {
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
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, iterations)) {
                key = deriveBytes.GetBytes(size);
            }
            return key;
        }
        #endregion


        /// <summary>
        /// Encrypts a string using one or more <see cref="Protections"/>. If multiple protections are selected, the encrypted data is passed from one into the next in a layered fasion.
        /// </summary>
        /// <remarks>
        /// The protections are layered like so: <see cref="Protections.ChaCha20Poly1305"/> -> <see cref="Protections.AES256_CBC"/> -> <see cref="Protections.AspNetCoreDataProtection"/>.
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

            if (protectionsToUse.HasFlag(Protections.AES256_CBC) || protectionsToUse.HasFlag(Protections.AES128_CBC)) {
                if (protectionsToUse.HasFlag(Protections.AES256_CBC))
                    cipherText = EncryptAES256_CBC(cipherText, actualKey, iv);
                else
                    cipherText = EncryptAES128_CBC(cipherText, actualKey, iv);
            }

            if (protectionsToUse.HasFlag(Protections.ChaCha20Poly1305) && ChaCha20Poly1305.IsSupported) {
                if (nonce == null || nonce.Length != 12)
                    throw new ArgumentException("Nonce must be 12 bytes (96 bit)", nameof(nonce));

                cipherText = EncryptChaCha20Poly1305(cipherText, actualKey, nonce, associationData ?? Array.Empty<byte>());
            }

            //actualKey = GenerateRandomBytes(32);

            if (protectionsToUse.HasFlag(Protections.AspNetCoreDataProtection))
                cipherText = EncryptAspNetCoreDataProtector(cipherText);

            return cipherText;
        }

        /// <summary>
        /// Decrypts cipher text using one or more <see cref="Protections"/> depending on what was used in encryption.
        /// </summary>
        /// <param name="cipherText">Data to decrypt</param>
        /// <param name="key">Decryption key</param>
        /// <returns></returns>
        public static string Decrypt(string cipherText, byte[] key) {
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
                    case "chacha20-poly1305":
                        cipherText = DecryptChaCha20Poly1305(cipherText, actualKey);
                        break;

                    case "aes256.CBC":
                    case "aes128.CBC":
                        cipherText = DecryptAES(cipherText, actualKey);
                        break;

                    case "aes256.GCM":
                    case "aes128.GCM":
                        throw new NotImplementedException();

                    case "aspnet":
                        cipherText = DecryptAspNetCoreDataProtector(cipherText);
                        break;

                    default:
                        loop = false;
                        break;
                }
            }
            return cipherText;
        }
    }



    #region DPAPI (like) helpers
    public class DataProtector {
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private IDataProtector _dataProtector;

        public DataProtector(IDataProtectionProvider dataProtectionProvider) {
            _dataProtectionProvider = dataProtectionProvider;
            _dataProtector = _dataProtectionProvider.CreateProtector("SteamAuthenticator.Core.Providers");
        }

        /// <summary>
        /// Allows you to set the subPurpose for the <see cref="IDataProtector"/>.
        /// </summary>
        /// <param name="purpose">Optional sub purpose passed to <see cref="IDataProtectionProvider.CreateProtector"/>.</param>
        public void CreateProtector(string? purpose) {
            if (purpose is null)
                _dataProtector = _dataProtectionProvider.CreateProtector("SteamAuthenticator.Core.Providers");
            else
                _dataProtector = _dataProtectionProvider.CreateProtector("SteamAuthenticator.Core.Providers", new string[] { purpose });
        }

        /// <summary>
        /// Protects and encrypts a string.
        /// </summary>
        /// <param name="data">String to encrypt</param>
        /// <returns>Encrypted string.</returns>
        public string Protect(string data) {
            if (data is null)
                return "";

            return _dataProtector.Protect(data);
        }

        /// <summary>
        /// Decrypts a protected string.
        /// </summary>
        /// <param name="data">String to decrypt</param>
        /// <returns>Decrypted string.</returns>
        public string Unprotect(string data) {
            if (data is null)
                return "";

            return _dataProtector.Unprotect(data);
        }
    }
    #endregion
}
