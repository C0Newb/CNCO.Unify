using CNCO.Unify.Configuration;
using CNCO.Unify.Security;
using CNCO.Unify.Storage;
using System.Text;

namespace UnifyTests.Configuration.Json {
    internal class MyJsonConfig : CNCO.Unify.Configuration.Json.JsonConfiguration {
        public string StringValue { get; set; } = "MyStringValue";
        public bool BoolValue { get; set; } = true;
        public Guid GuidValue { get; set; } = Guid.NewGuid();
        public int IntValue { get; set; } = 10;

        public MyJsonConfig() { }

        public MyJsonConfig(string FilePath, IFileStorage fileStorage) : base(FilePath, fileStorage) { }
        public MyJsonConfig(string FilePath, IFileStorage fileStorage, IFileEncryption fileEncryption) : base(FilePath, fileStorage, fileEncryption) { }
    }

    public class MySecureJsonConfig : CNCO.Unify.Configuration.Json.SecureJsonConfiguration {
        [Secure]
        public string StringValue { get; set; } = "MyStringValue";
        public bool BoolValue { get; set; } = true;
        [Secure]
        public Guid GuidValue { get; set; } = Guid.NewGuid();
        public int IntValue { get; set; } = 10;

        public MySecureJsonConfig() { }

        public MySecureJsonConfig(string FilePath, IFileStorage fileStorage, IFileEncryption fileEncryption) : base(FilePath, fileStorage, fileEncryption) { }
    }

    internal class MyEncryptionKeyProvider : IEncryptionKeyProvider {
        public Encryption.Protections Protections = Encryption.Protections.DataProtection | Encryption.Protections.AES256_CBC;

        private readonly byte[] _encryptionKey;

        public MyEncryptionKeyProvider() {
            _encryptionKey = Encryption.GenerateRandomBytes(64);
        }

        public MyEncryptionKeyProvider(string Key) {
            _encryptionKey = Encoding.Default.GetBytes(Key);
        }

        public byte[]? GetAssociationData() {
            return null;
        }

        public byte[] GetEncryptionKey() {
            return _encryptionKey;
        }

        public byte[]? GetIV() {
            return null;
        }

        public byte[]? GetNonce() {
            return null;
        }

        public Encryption.Protections GetProtections() {
            return Protections;
        }
    }
}
