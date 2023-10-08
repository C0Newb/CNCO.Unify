using System.Runtime.Versioning;

namespace CNCO.Unify.Security.Credentials {
    /// <summary>
    /// <see cref="ICredentialManager"/> for Android.
    /// </summary>
    [SupportedOSPlatform("android")]
    public class AndroidCredentialManager : ICredentialManager {
        public bool Exists(string credentialName) => throw new NotImplementedException();
        public string Get(string credentialName) => throw new NotImplementedException();
        public bool Remove(string credentialName) => throw new NotImplementedException();
        public bool Set(string credentialName, string value) => throw new NotImplementedException();
    }
}
