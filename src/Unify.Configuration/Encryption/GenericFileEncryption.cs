using CNCO.Unify.Security;
using System.Text;

namespace CNCO.Unify.Configuration.Encryption {
    /// <summary>
    /// Basic file encryption using a <see cref="IEncryptionKeyProvider"/> and <see cref="Security.Encryption"/>.
    /// </summary>
    public class GenericFileEncryption : IFileEncryption {
        private readonly IEncryptionKeyProvider? _encryptionKeyProvider;

        public GenericFileEncryption(IEncryptionKeyProvider encryptionKeyProvider) {
            _encryptionKeyProvider = encryptionKeyProvider;
        }


        public string EncryptString(string data) {
            if (_encryptionKeyProvider == null)
                return data;

            return Security.Encryption.Encrypt(data,
                _encryptionKeyProvider.GetEncryptionKey(),
                _encryptionKeyProvider.GetProtections(),
                _encryptionKeyProvider.GetNonce());
        }
        public string DecryptString(string data) {
            if (_encryptionKeyProvider == null)
                return data;

            return Security.Encryption.Decrypt(data, _encryptionKeyProvider.GetEncryptionKey());
        }


        public byte[] EncryptBytes(byte[] data) {
            if (_encryptionKeyProvider == null)
                return data;

            string dataString = Encoding.UTF8.GetString(data);
            string encryptedData = Security.Encryption.Encrypt(dataString,
                _encryptionKeyProvider.GetEncryptionKey(),
                _encryptionKeyProvider.GetProtections(),
                _encryptionKeyProvider.GetNonce());

            return Encoding.UTF8.GetBytes(encryptedData);
        }
        public byte[] DecryptBytes(byte[] data) {
            if (_encryptionKeyProvider == null)
                return data;

            string dataString = Encoding.UTF8.GetString(data);
            string encryptedData = Security.Encryption.Decrypt(dataString, _encryptionKeyProvider.GetEncryptionKey());

            return Encoding.UTF8.GetBytes(encryptedData);
        }
    }
}