namespace CNCO.Unify.Security {
    /// <summary>
    /// Interface used to provide key encryption parameters, such as the protections, key, nonce, associated data, and IV to objects that wish to use encryption.
    /// </summary>
    public interface IEncryptionKeyProvider {
        /// <summary>
        /// Returns the protections to be used when encrypting data.
        /// </summary>
        /// <returns>Encryption methods to be used.</returns>
        public Encryption.Protections GetProtections();

        /// <summary>
        /// Returns the encryption key to be used.
        /// </summary>
        /// <returns>Encryption key.</returns>
        public byte[] GetEncryptionKey();

        /// <summary>
        /// Returns the optional nonce to be used while encrypting via ChaCha20-Poly1305.
        /// </summary>
        /// <returns>Unique nonce.</returns>
        public byte[]? GetNonce();

        /// <summary>
        /// Returns the optional association data to be used while encrypting.
        /// </summary>
        /// <returns>Association data.</returns>
        public byte[]? GetAssociationData();

        /// <summary>
        /// Returns the optional initialization vector to used while encrypting via AES.
        /// If null one will be generated.
        /// </summary>
        /// <returns>AES initialization vector.</returns>
        public byte[]? GetIV();
    }


    [Serializable]
    public class NoEncryptionKeyProvidedException : Exception {
        public NoEncryptionKeyProvidedException() { }
        public NoEncryptionKeyProvidedException(string message) : base(message) { }
        public NoEncryptionKeyProvidedException(string message, Exception inner) : base(message, inner) { }
    }
}
