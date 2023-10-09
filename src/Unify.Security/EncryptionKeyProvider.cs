namespace CNCO.Unify.Security {
    /// <summary>
    /// Basic implementation of <see cref="IEncryptionKeyProvider"/>.
    /// </summary>
    public class EncryptionKeyProvider : IEncryptionKeyProvider {
        private byte[]? AssociationData { get; set; }
        private byte[]? Nonce { get; set; }
        private byte[] Key { get; set; }
        private byte[]? Iv { get; set; }
        private Encryption.Protections Protections { get; set; } = Encryption.Protections.None;

        /// <summary>
        /// Creates a new instance of <see cref="EncryptionKeyProvider"/> given a key.
        /// </summary>
        /// <param name="key">Encryption key to provide.</param>
        public EncryptionKeyProvider(byte[] key) {
            Key = key;
        }

        /// <summary>
        /// Creates a new instance of <see cref="EncryptionKeyProvider"/> given a key.
        /// </summary>
        /// <param name="key">Encryption key to provide.</param>
        /// <param name="protections">Encryption methods to apply.</param>
        public EncryptionKeyProvider(byte[] key, Encryption.Protections protections) {
            Key = key;
            Protections = protections;
        }

        public byte[]? GetAssociationData() => AssociationData;
        public byte[] GetEncryptionKey() => Key;
        public byte[]? GetIV() => Iv;
        public byte[]? GetNonce() => Nonce ?? Encryption.GenerateRandomBytes(12);

        public Encryption.Protections GetProtections() => Protections;
    }
}
