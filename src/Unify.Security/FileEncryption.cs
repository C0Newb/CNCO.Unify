using System.Text;

namespace CNCO.Unify.Security {
    /// <summary>
    /// Basic file encryption using a <see cref="IEncryptionKeyProvider"/> and <see cref="Encryption"/>.
    /// </summary>
    public class FileEncryption : IFileEncryption {
        private readonly IEncryptionKeyProvider _encryptionKeyProvider;

        public FileEncryption(IEncryptionKeyProvider encryptionKeyProvider) {
            _encryptionKeyProvider = encryptionKeyProvider;
        }


        public string EncryptString(string data) {
            if (_encryptionKeyProvider == null)
                return data;

            return Encryption.Encrypt(data,
                _encryptionKeyProvider.GetEncryptionKey(),
                _encryptionKeyProvider.GetProtections(),
                _encryptionKeyProvider.GetNonce());
        }
        public string DecryptString(string data) {
            if (_encryptionKeyProvider == null)
                return data;

            return Encryption.Decrypt(data, _encryptionKeyProvider.GetEncryptionKey());
        }


        public byte[] EncryptBytes(byte[] data) {
            if (_encryptionKeyProvider == null)
                return data;

            string dataString = Encoding.UTF8.GetString(data);
            string encryptedData = Encryption.Encrypt(dataString,
                _encryptionKeyProvider.GetEncryptionKey(),
                _encryptionKeyProvider.GetProtections(),
                _encryptionKeyProvider.GetNonce());

            return Encoding.UTF8.GetBytes(encryptedData);
        }
        public byte[] DecryptBytes(byte[] data) {
            if (_encryptionKeyProvider == null)
                return data;

            string dataString = Encoding.UTF8.GetString(data);
            string encryptedData = Encryption.Decrypt(dataString, _encryptionKeyProvider.GetEncryptionKey());

            return Encoding.UTF8.GetBytes(encryptedData);
        }
    }
}