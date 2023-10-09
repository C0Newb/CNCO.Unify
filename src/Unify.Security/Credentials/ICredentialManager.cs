namespace CNCO.Unify.Security.Credentials {
    /// <summary>
    /// Used to distinguish platform specific <see cref="ICredentialManager"/>.
    /// </summary>
    public interface ICredentialManagerEndpoint : ICredentialManager { }

    /// <summary>
    /// Interface to interact with a credential manager, such as Windows Credential Manager, Apple's KeyChain, KeyStore, etc.
    /// </summary>
    public interface ICredentialManager {
        /// <summary>
        /// Sets a credential in the credential manager/keychain to a given value.
        /// </summary>
        /// <param name="credentialName">The name of the value to set.</param>
        /// <param name="value">The credential's value.</param>
        /// <returns>Whether the key was successfully updated or not.</returns>
        public bool Set(string credentialName, string value);

        /// <summary>
        /// Returns the value for a given credential.
        /// A null returns signifies the credential does not exist.
        /// </summary>
        /// <param name="credentialName">The name of the credential to get.</param>
        /// <returns>The credential's value.</returns>
        public string? Get(string credentialName);

        /// <summary>
        /// Remove a credential from the credential manager/keychain.
        /// </summary>
        /// <param name="credentialName">Name of the credential to remove.</param>
        /// <returns>Whether the credential was successfully removed or not.</returns>
        public bool Remove(string credentialName);

        /// <summary>
        /// Checks whether a credential exists in the credential manager/keychain.
        /// </summary>
        /// <param name="credentialName">Credential to check for.</param>
        /// <returns>Whether the credential exists in the credential manager.</returns>
        public bool Exists(string credentialName);
    }
}
