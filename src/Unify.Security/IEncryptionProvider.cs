namespace CNCO.Unify.Security {
    /// <summary>
    /// Used to process the encryption methods to a configuration item.
    /// </summary>
    public interface IEncryptionProvider {
        /// <summary>
        /// Encrypts contents as a string.
        /// </summary>
        /// <param name="data">Contents to encrypt.</param>
        /// <returns>Encrypted string of <paramref name="data"/>.</returns>
        public string EncryptString(string data);
        /// <inheritdoc cref="EncryptString(string)"/>
        public string EncryptString(byte[] data);

        /// <inheritdoc cref="EncryptString(string)"/>
        /// <param name="associatedData">Associated data to force check if using AEAD encryption.</param>
        public string EncryptString(string data, string? associatedData);
        /// <inheritdoc cref="EncryptString(string, string)"/>
        public string EncryptString(byte[] data, string? associatedData);
        /// <inheritdoc cref="EncryptString(string, string)"/>
        public string EncryptString(string data, byte[]? associatedData);
        /// <inheritdoc cref="EncryptString(string, string)"/>
        public string EncryptString(byte[] data, byte[]? associatedData);


        /// <summary>
        /// Decrypts contents as a string.
        /// </summary>
        /// <param name="data">Encrypted (or decrypted) contents. Method will determine whether the string actually needs to be decrypted.</param>
        /// <returns>Decrypted string of <paramref name="data"/>.</returns>
        public string DecryptString(string data);
        /// <inheritdoc cref="DecryptString(string)"/>
        public string DecryptString(byte[] data);

        /// <inheritdoc cref="DecryptString(string)"/>
        /// <param name="associatedData">Associated data to force check if using AEAD encryption.</param>
        public string DecryptString(string data, string? associatedData);
        /// <inheritdoc cref="DecryptString(string, string)"/>
        public string DecryptString(byte[] data, string? associatedData);
        /// <inheritdoc cref="DecryptString(string, string)"/>
        public string DecryptString(string data, byte[]? associatedData);
        /// <inheritdoc cref="DecryptString(string, string)"/>
        public string DecryptString(byte[] data, byte[]? associatedData);



        /// <summary>
        /// Encrypts content as bytes.
        /// </summary>
        /// <param name="data">Contents to encrypt.</param>
        /// <returns>Encrypted string of <paramref name="data"/>.</returns>
        public byte[] EncryptBytes(string data);
        /// <inheritdoc cref="EncryptBytes(string)"/>
        public byte[] EncryptBytes(byte[] data);

        /// <inheritdoc cref="EncryptBytes(string)"/>
        /// <param name="associatedData">Associated data to force check if using AEAD encryption.</param>
        public byte[] EncryptBytes(string data, string associatedData);
        /// <inheritdoc cref="EncryptBytes(string, string)"/>
        public byte[] EncryptBytes(string data, byte[]? associatedData);
        /// <inheritdoc cref="EncryptBytes(string, string)"/>
        public byte[] EncryptBytes(byte[] data, string associatedData);
        /// <inheritdoc cref="EncryptBytes(string, string)"/>
        public byte[] EncryptBytes(byte[] data, byte[]? associatedData);



        /// <summary>
        /// Decrypts contents as bytes.
        /// </summary>
        /// <param name="data">Encrypted (or decrypted) contents. Method will determine whether the string actually needs to be decrypted.</param>
        /// <returns>Decrypted string of <paramref name="data"/>.</returns>
        public byte[] DecryptBytes(string data);
        /// <inheritdoc cref="DecryptBytes(string)"/>
        public byte[] DecryptBytes(byte[] data);

        /// <inheritdoc cref="DecryptBytes(string)"/>
        /// <param name="associatedData">Associated data to force check if using AEAD encryption.</param>
        public byte[] DecryptBytes(string data, string? associatedData);
        /// <inheritdoc cref="DecryptBytes(string, string)"/>
        public byte[] DecryptBytes(string data, byte[]? associatedData);
        /// <inheritdoc cref="DecryptBytes(string, string)"/>
        public byte[] DecryptBytes(byte[] data, string? associatedData);
        /// <inheritdoc cref="DecryptBytes(string, string)"/>
        public byte[] DecryptBytes(byte[] data, byte[]? associatedData);
    }
}
