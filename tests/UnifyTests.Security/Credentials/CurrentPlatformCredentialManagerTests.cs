using CNCO.Unify.Security.Credentials;

namespace UnifyTests.Security.Credentials {
    internal class CurrentPlatformCredentialManagerTests : BaseCredentialManagerTests {
        private ICredentialManager? CredentialManager;

        public override ICredentialManager GetCredentialManager() {
            CredentialManager ??= CredentialManagerFactory.GetPlatformCredentialManager();
            return CredentialManager;
        }
    }
}
