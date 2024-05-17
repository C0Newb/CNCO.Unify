using System.Text;

namespace CNCO.Unify.Security {
    /// <summary>
    /// Basic encryption provider using a <see cref="IEncryptionKeyProvider"/> and <see cref="Encryption"/>.
    /// </summary>
    /// <remarks>
    /// This provides a wrapper around <see cref="Encryption"/> by using the key
    /// and <see cref="Encryption.Protections"/> set by the <see cref="EncryptionKeyProvider"/>.
    /// </remarks>
    public class EncryptionProvider(IEncryptionKeyProvider encryptionKeyProvider) : IEncryptionProvider {
        private readonly IEncryptionKeyProvider _encryptionKeyProvider = encryptionKeyProvider;

        #region String
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
        #endregion

        #region Bytes
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
        #endregion
    }
}