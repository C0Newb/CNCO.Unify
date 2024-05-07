using CNCO.Unify.Security.Credentials;
using CNCO.Unify.Storage;

namespace UnifyTests.Security.Credentials {
    internal class FileBasedCredentialManagerTests : BaseCredentialManagerTests {
        private ICredentialManager? CredentialManager;

        public override ICredentialManager GetCredentialManager() {
            CredentialManager ??= new FileBasedCredentialManager(new InMemoryFileStorage(), "Unify.TestCredentials.json");
            return CredentialManager;
        }
    }
}
