using System.Security.Cryptography;
using System.Text;

namespace CNCO.Unify.Security {
    /// <summary>
    /// Hashing helper library.
    /// </summary>
    public class Hashing {
        private static string Hash(HashAlgorithm algorithm, string data) {
            byte[] credentialBytes = Encoding.UTF8.GetBytes(data);
            byte[] hashBytes = algorithm.ComputeHash(credentialBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        public static string Sha1(string data) {
            using (var sha = SHA1.Create())
                return Hash(sha, data);
        }

        public static string Sha256(string data) {
            using (var sha = SHA256.Create())
                return Hash(sha, data);
        }

        public static string Sha512(string data) {
            using (var sha = SHA512.Create())
                return Hash(sha, data);
        }
    }
}
