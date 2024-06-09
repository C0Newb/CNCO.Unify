using System.Text;

namespace CNCO.Unify.Security.Credentials {
    internal class CredentialHelpers {
        /// <summary>
        /// Takes a string and returns the hash of the string and the string (in base64).
        /// Like so: <c>HASH$base64String</c>. Use this to detect tampering.
        /// </summary>
        /// <param name="value">Credential to hash.</param>
        /// <returns><c>Sha512$base64OfValue</c>.</returns>
        internal static string ApplyTamperHash(string value) {
            string hash = Hashing.Sha512(value);

            byte[] valueBytes = Encoding.UTF8.GetBytes(value);
            string base64 = Convert.ToBase64String(valueBytes);

            return $"{hash}${base64}";
        }

        /// <summary>
        /// Gets the credential's value and verifies the "tamper hash" (checks if the credential was modified).
        /// </summary>
        /// <param name="value">Value from the <see cref="ICredentialManager"/>'s backing.</param>
        /// <returns>The credential's value.</returns>
        /// <exception cref="CredentialTamperException">Someone tampered with the credential.</exception>
        internal static string GetAndVerifyCredential(string value) {
            string tag = $"{nameof(CredentialHelpers)}::{nameof(GetAndVerifyCredential)}";

            string[] components = value.Split('$');
            if (components.Length != 2) {
                SecurityRuntime.Current.RuntimeLog.Warning(tag, $"Expected 2 components, hash and credential value. Got {components.Length} components.");
                if (SecurityRuntime.Current.Configuration.DisableCredentialManagerHashChecking)
                    return value;
                else
                    throw new CredentialTamperException("Credential has been tampered with and is in an invalid format.");
            }

            byte[] credentialBytes = Convert.FromBase64String(components[1]);
            string credentialValue = Encoding.UTF8.GetString(credentialBytes);
            string actualHash = Hashing.Sha512(credentialValue);

            bool hashNull = string.IsNullOrEmpty(actualHash);
            bool hashMatches = components[0] != actualHash;
            if (hashNull || hashMatches) {
                SecurityRuntime.Current.RuntimeLog.Verbose(tag, $"Uh-oh.. actualHash null? {hashNull}. Hash ok? {hashMatches}");
                if (SecurityRuntime.Current.Configuration.DisableCredentialManagerHashChecking)
                    return credentialValue;
                else
                    throw new CredentialTamperException("Credential has been tampered with and is invalid.");
            }

            return credentialValue;
        }
    }
}
