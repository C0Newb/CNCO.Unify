namespace CNCO.Unify.Security {
    /// <summary>
    /// An interface that can provide data protection services.
    /// </summary>
    public interface IDataProtector : IDataProtectionProvider {
        /// <summary>
        /// Cryptographically protects a piece of plaintext data.
        /// </summary>
        /// <param name="plaintext">The plaintext data to protect.</param>
        /// <returns>The protected form of the plaintext data.</returns>
        byte[] Protect(byte[] plaintext);

        /// <inheritdoc cref="Protect(byte[])"/>
        string Protect(string plaintext);


        /// <summary>
        /// Cryptographically unprotects a piece of protected data.
        /// </summary>
        /// <param name="protectedData">The protected data to unprotect.</param>
        /// <returns>The plaintext form of the protected data.</returns>
        byte[] Unprotect(byte[] protectedData);

        /// <inheritdoc cref="Unprotect(byte[])"/>
        string Unprotect(string protectedData);

    }
}
