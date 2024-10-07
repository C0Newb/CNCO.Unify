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


        #region Common encrypt/decrypt
        private string EncryptStringCommon(string? data, byte[]? dataBytes, string? associatedData, byte[]? associatedDataBytes) {
            if (data == null && dataBytes == null)
                return string.Empty;
            if (data == null && dataBytes != null)
                data = Encoding.UTF8.GetString(dataBytes);


            if (_encryptionKeyProvider == null)
                return data ?? string.Empty;

            if (associatedData != null && associatedDataBytes == null)
                associatedDataBytes = Encoding.UTF8.GetBytes(associatedData);

            return Encryption.Encrypt(
                data!,
                _encryptionKeyProvider.GetEncryptionKey(),
                _encryptionKeyProvider.GetProtections(),
                _encryptionKeyProvider.GetNonce(),
                associatedDataBytes
            );
        }
        private byte[] EncryptBytesCommon(string? data, byte[]? dataBytes, string? associatedData, byte[]? associatedDataBytes) {
            string encryptedData = EncryptStringCommon(data, dataBytes, associatedData, associatedDataBytes);
            byte[] bytes = Encoding.UTF8.GetBytes(encryptedData);
            return bytes;
        }

        private string DecryptStringCommon(string? data, byte[]? dataBytes, string? associatedData, byte[]? associatedDataBytes) {
            if (data == null && dataBytes == null)
                return string.Empty;
            if (data == null && dataBytes != null)
                data = Encoding.UTF8.GetString(dataBytes);

            if (_encryptionKeyProvider == null)
                return data ?? string.Empty;

            if (associatedData != null && associatedDataBytes == null)
                associatedDataBytes = Encoding.UTF8.GetBytes(associatedData);

            return Encryption.Decrypt(data!, _encryptionKeyProvider.GetEncryptionKey(), associatedDataBytes);
        }

        private byte[] DecryptBytesCommon(string? data, byte[]? dataBytes, string? associatedData, byte[]? associatedDataBytes) {
            string decryptedData = DecryptStringCommon(data, dataBytes, associatedData, associatedDataBytes);
            byte[] bytes = Encoding.UTF8.GetBytes(decryptedData);
            return bytes;
        }
        #endregion


        #region String
        public string EncryptString(string data) => EncryptStringCommon(data, null, null, null);
        public string EncryptString(byte[] data) => EncryptStringCommon(null, data, null, null);
        public string EncryptString(string data, string? associatedData) => EncryptStringCommon(data, null, associatedData, null);
        public string EncryptString(string data, byte[]? associatedData) => EncryptStringCommon(data, null, null, associatedData);
        public string EncryptString(byte[] data, string? associatedData) => EncryptStringCommon(null, data, associatedData, null);
        public string EncryptString(byte[] data, byte[]? associatedData) => EncryptStringCommon(null, data, null, associatedData);

        public string DecryptString(string data) => DecryptStringCommon(data, null, null, null);
        public string DecryptString(byte[] data) => DecryptStringCommon(null, data, null, null);
        public string DecryptString(string data, string? associatedData) => DecryptStringCommon(data, null, associatedData, null);
        public string DecryptString(string data, byte[]? associatedData) => DecryptStringCommon(data, null, null, associatedData);
        public string DecryptString(byte[] data, string? associatedData) => DecryptStringCommon(null, data, associatedData, null);
        public string DecryptString(byte[] data, byte[]? associatedData) => DecryptStringCommon(null, data, null, associatedData);
        #endregion



        #region Bytes
        public byte[] EncryptBytes(string data) => EncryptBytesCommon(data, null, null, null);
        public byte[] EncryptBytes(byte[] data) => EncryptBytesCommon(null, data, null, null);
        public byte[] EncryptBytes(string data, string? associatedData) => EncryptBytesCommon(data, null, associatedData, null);
        public byte[] EncryptBytes(string data, byte[]? associatedData) => EncryptBytesCommon(data, null, null, associatedData);
        public byte[] EncryptBytes(byte[] data, string? associatedData) => EncryptBytesCommon(null, data, associatedData, null);
        public byte[] EncryptBytes(byte[] data, byte[]? associatedData) => EncryptBytesCommon(null, data, null, associatedData);


        public byte[] DecryptBytes(string data) => DecryptBytesCommon(data, null, null, null);
        public byte[] DecryptBytes(byte[] data) => DecryptBytesCommon(null, data, null, null);
        public byte[] DecryptBytes(string data, string? associatedData) => DecryptBytesCommon(data, null, associatedData, null);
        public byte[] DecryptBytes(string data, byte[]? associatedData) => DecryptBytesCommon(data, null, null, associatedData);
        public byte[] DecryptBytes(byte[] data, string? associatedData) => DecryptBytesCommon(null, data, associatedData, null);
        public byte[] DecryptBytes(byte[] data, byte[]? associatedData) => DecryptBytesCommon(null, data, null, associatedData);
        #endregion
    }
}