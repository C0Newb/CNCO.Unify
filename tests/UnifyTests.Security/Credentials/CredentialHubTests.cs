using CNCO.Unify.Security.Credentials;
using CNCO.Unify.Storage;

namespace UnifyTests.Security.Credentials {
    internal class CredentialHubTests : BaseCredentialManagerTests {
        private ICredentialManager? CredentialManager;

        public override ICredentialManager GetCredentialManager() {
            CredentialManager ??= new CredentialHub(
                new FileBasedCredentialManager(new InMemoryFileStorage(), "Unify.TestCredentials.json"),
                "Testing"
            );
            return CredentialManager;
        }
    }
}
