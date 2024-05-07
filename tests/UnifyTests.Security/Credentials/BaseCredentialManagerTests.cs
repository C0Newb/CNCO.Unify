using CNCO.Unify.Security.Credentials;

namespace UnifyTests.Security.Credentials {
    [Ignore("Base class.")]
    internal abstract class BaseCredentialManagerTests {
        internal readonly string CredentialName = $"Unify.TestCredential_{Guid.NewGuid()}";
        internal readonly string CredentialValue = Guid.NewGuid().ToString();

        public abstract ICredentialManager GetCredentialManager();

        [SetUp]
        public void SetUp() {

        }

        [TearDown]
        public void TearDown() {
            if (GetCredentialManager().Exists(CredentialName))
                GetCredentialManager().Remove(CredentialName);
        }


        [Test]
        public void SetAndGet_New_CanAddCredential() {
            GetCredentialManager().Set(CredentialName, CredentialValue);
            // Verify value
            string? value = GetCredentialManager().Get(CredentialName);
            Assert.That(value, Is.EqualTo(CredentialValue), "Failed to add or retrieve credential.");
        }
        [Test]
        public void SetAndGet_Update_CanUpdateCredential() {
            GetCredentialManager().Set(CredentialName, CredentialValue);
            GetCredentialManager().Set(CredentialName, CredentialValue + "-123456");
            // Verify value
            string? value = GetCredentialManager().Get(CredentialName);
            Assert.That(value, Is.EqualTo(CredentialValue + "-123456"), "Failed to either add or update credential.");
        }


        [Test]
        public void Remove_NoCredential_DoNothing() {
            GetCredentialManager().Remove(CredentialName);
            Assert.Pass();
        }
        [Test]
        public void Remove_ExistingCredential_Removes() {
            GetCredentialManager().Set(CredentialName, CredentialValue);
            bool existsPreRemove = GetCredentialManager().Exists(CredentialName);
            Assert.That(existsPreRemove, Is.True, "Credential successfully setup.");

            GetCredentialManager().Remove(CredentialName);
            bool existsPostRemove = GetCredentialManager().Exists(CredentialName);

            Assert.That(existsPostRemove, Is.False, "Credential was NOT removed, still exists in the credential manager!");
        }


        [Test]
        public void Exists_NoCredential_ReturnsFalse() {
            bool exists = GetCredentialManager().Exists(CredentialName);
            Assert.That(exists, Is.False, "Credential does not exist.");
        }
        [Test]
        public void Exists_ExistingCredential_ReturnsTrue() {
            GetCredentialManager().Set(CredentialName, CredentialValue);
            bool exists = GetCredentialManager().Exists(CredentialName);
            Assert.That(exists, Is.True, "Credential does NOT exist!");
        }
    }
}