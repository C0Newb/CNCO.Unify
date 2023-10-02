namespace CNCO.Unify.Configuration.Encryption
{
    /// <summary>
    /// Used to process the encryption methods to a configuration item.
    /// </summary>
    public interface IFileEncryption
    {
        /// <summary>
        /// Encrypts a file's contents.
        /// </summary>
        /// <param name="data">Contents of the file to encrypt.</param>
        /// <returns>Encrypted string of <paramref name="data"/>.</returns>
        public string EncryptString(string data);

        /// <summary>
        /// Decrypts a file's contents.
        /// </summary>
        /// <param name="data">Encrypted (or decrypted) contents of the file. Method will determine whether the string actually needs to be decrypted.</param>
        /// <returns>Decrypted string of <paramref name="data"/>.</returns>
        public string DecryptString(string data);



        /// <summary>
        /// Encrypts a file's contents as bytes.
        /// </summary>
        /// <param name="data">Encrypted (or decrypted) contents of the file. Method will determine whether the string actually needs to be decrypted.</param>
        /// <param name="file">File that is being decrypted.</param>
        /// <returns>Encrypted string of <paramref name="data"/>.</returns>
        public byte[] EncryptBytes(byte[] data);

        /// <summary>
        /// Decrypts a file's contents.
        /// </summary>
        /// <param name="data">Encrypted (or decrypted) contents of the file. Method will determine whether the string actually needs to be decrypted.</param>
        /// <returns>Decrypted string of <paramref name="data"/>.</returns>
        public byte[] DecryptBytes(byte[] data);
    }
}
