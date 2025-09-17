namespace CNCO.Unify.Security {
public static partial class Encryption {
        /// <summary>
        /// Encryption methods.
        /// </summary>
        /// <remarks>
        /// AES256_CBC will be, generally speaking, the most secure for most applications.
        /// ChatCha20Poly1305 is not widely supported!
        /// DataProtection has less portability!
        /// GCM can be compromised if the same nonce is used ever!
        /// </remarks>
        [Flags]
        public enum Protections {
            /// <summary>
            /// Will not use any encryption.
            /// </summary>
            None = 0x0,

            /// <summary>
            /// Use <see cref="DataProtectionProvider"/> for encryption.
            /// </summary>
            DataProtection = 0x1,

            /// <summary>
            /// Use ChaCha20-Poly1305 for encryption.
            /// </summary>
            /// <remarks>
            /// This is not widely supported! Please use a different protection unless you are for sure ChaCha20 is supported in your environment.
            /// </remarks>
            ChaCha20Poly1305 = 0x2,

            /// <summary>
            /// Use 128 bit AES (CBC mode) for encryption.
            /// </summary>
            AES128_CBC = 0x4,

            /// <summary>
            /// Use 128 bit AES (CBC mode) for encryption.
            /// </summary>
            AES256_CBC = 0x8,

            /// <summary>
            /// Use AES (GCM/AEAD mode) for encryption.
            /// </summary>
            AES128_GCM = 0x10,

            /// <summary>
            /// Use AES (GCM/AEAD mode) for encryption.
            /// </summary>
            /// <remarks>
            /// Same as <see cref="Protections.AES128_GCM"/>.
            /// </remarks>
            AES256_GCM = 0x20,
        }
    }
}
