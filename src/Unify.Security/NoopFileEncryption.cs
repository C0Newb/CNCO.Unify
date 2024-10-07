using System.Text;

namespace CNCO.Unify.Security {
    /// <summary>
    /// Used to provide no file encryption when a <see cref="IEncryptionProvider"/> object is required.
    /// </summary>
    public class NoopFileEncryption : IEncryptionProvider {
        /// <summary>
        /// Returns <paramref name="data"/> without any operations preformed on it.
        /// </summary>
        /// <param name="data">File contents.</param>
        /// <returns><paramref name="data"/>.</returns>
        public byte[] DecryptBytes(byte[] data) => data;
        /// <inheritdoc cref="DecryptBytes(byte[])"/>
        public byte[] DecryptBytes(string data) => Encoding.UTF8.GetBytes(data);
        /// <inheritdoc cref="DecryptBytes(byte[])"/>
        /// <param name="associatedData">Associated data (is ignored).</param>
        public byte[] DecryptBytes(string data, string? associatedData) => Encoding.UTF8.GetBytes(data);
        /// <inheritdoc cref="DecryptBytes(string, string)"/>
        public byte[] DecryptBytes(string data, byte[]? associatedData) => Encoding.UTF8.GetBytes(data);
        /// <inheritdoc cref="DecryptBytes(string, string)"/>
        public byte[] DecryptBytes(byte[] data, string? associatedData) => data;
        /// <inheritdoc cref="DecryptBytes(string, string)"/>
        public byte[] DecryptBytes(byte[] data, byte[]? associatedData) => data;

        /// <inheritdoc cref="DecryptBytes(byte[])"/>
        public string DecryptString(string data) => data;
        /// <inheritdoc cref="DecryptBytes(byte[])"/>
        public string DecryptString(byte[] data) => Encoding.UTF8.GetString(data);
        /// <inheritdoc cref="DecryptBytes(string, string)"/>
        public string DecryptString(string data, string? associatedData) => data;
        /// <inheritdoc cref="DecryptBytes(string, string)"/>
        public string DecryptString(byte[] data, string? associatedData) => Encoding.UTF8.GetString(data);
        /// <inheritdoc cref="DecryptBytes(string, string)"/>
        public string DecryptString(string data, byte[]? associatedData) => data;
        /// <inheritdoc cref="DecryptBytes(string, string)"/>
        public string DecryptString(byte[] data, byte[]? associatedData) => Encoding.UTF8.GetString(data);



        /// <summary>
        /// Returns <paramref name="data"/> without any operations preformed on it.
        /// </summary>
        /// <param name="data">File contents.</param>
        /// <returns><paramref name="data"/>.</returns>
        public byte[] EncryptBytes(byte[] data) => data;
        /// <inheritdoc cref="EncryptBytes(byte[])"/>
        public byte[] EncryptBytes(string data) => Encoding.UTF8.GetBytes(data);
        /// <inheritdoc cref="EncryptBytes(string, string)"/>
        /// <param name="associatedData">Associated data (is ignored).</param>
        public byte[] EncryptBytes(string data, string associatedData) => Encoding.UTF8.GetBytes(data);
        /// <inheritdoc cref="EncryptBytes(string, string)"/>
        public byte[] EncryptBytes(string data, byte[]? associatedData) => Encoding.UTF8.GetBytes(data);
        /// <inheritdoc cref="EncryptBytes(string, string)"/>
        public byte[] EncryptBytes(byte[] data, string associatedData) => data;
        /// <inheritdoc cref="EncryptBytes(string, string)"/>
        public byte[] EncryptBytes(byte[] data, byte[]? associatedData) => data;

        /// <inheritdoc cref="EncryptBytes(byte[])"/>
        public string EncryptString(string data) => data;
        /// <inheritdoc cref="EncryptBytes(byte[])"/>
        public string EncryptString(byte[] data) => Encoding.UTF8.GetString(data);
        /// <inheritdoc cref="EncryptBytes(string, string)"/>
        public string EncryptString(string data, string? associatedData) => data;
        /// <inheritdoc cref="EncryptBytes(string, string)"/>
        public string EncryptString(byte[] data, string? associatedData) => Encoding.UTF8.GetString(data);
        /// <inheritdoc cref="EncryptBytes(string, string)"/>
        public string EncryptString(string data, byte[]? associatedData) => data;
        /// <inheritdoc cref="EncryptBytes(string, string)"/>
        public string EncryptString(byte[] data, byte[]? associatedData) => Encoding.UTF8.GetString(data);
    }
}