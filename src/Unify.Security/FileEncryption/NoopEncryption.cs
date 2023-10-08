namespace CNCO.Unify.Security.FileEncryption {
    /// <summary>
    /// Used to provide no file encryption when a <see cref="IFileEncryption"/> object is required.
    /// </summary>
    public class NoopEncryption : IFileEncryption {
        /// <summary>
        /// Returns <paramref name="data"/> without any operations preformed on it.
        /// </summary>
        /// <param name="data">File contents.</param>
        /// <returns><paramref name="data"/>.</returns>
        public byte[] DecryptBytes(byte[] data) {
            return data;
        }

        /// <summary>
        /// Returns <paramref name="data"/> without any operations preformed on it.
        /// </summary>
        /// <param name="data">File contents.</param>
        /// <returns><paramref name="data"/>.</returns>
        public string DecryptString(string data) {
            return data;
        }

        /// <summary>
        /// Returns <paramref name="data"/> without any operations preformed on it.
        /// </summary>
        /// <param name="data">File contents.</param>
        /// <returns><paramref name="data"/>.</returns>
        public byte[] EncryptBytes(byte[] data) {
            return data;
        }

        /// <summary>
        /// Returns <paramref name="data"/> without any operations preformed on it.
        /// </summary>
        /// <param name="data">File contents.</param>
        /// <returns><paramref name="data"/>.</returns>
        public string EncryptString(string data) {
            return data;
        }
    }
}