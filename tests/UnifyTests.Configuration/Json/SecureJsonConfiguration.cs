using CNCO.Unify.Security;
using CNCO.Unify.Storage;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace UnifyTests.Configuration.Json {
    public class SecureJsonConfiguration {
        private const string TestFileName = "jsonConfigurationTest.json";
        private MyEncryptionKeyProvider myEncryptionKeyProvider;
        private IEncryptionProvider myFileEncryption;
        private InMemoryFileStorage myFileStorage;


        [SetUp]
        public void Setup() {
            myEncryptionKeyProvider = new MyEncryptionKeyProvider();
            myFileEncryption = new EncryptionProvider(myEncryptionKeyProvider);
            myFileStorage = new InMemoryFileStorage();
        }

        [TearDown]
        public void TearDown() {
            myFileStorage.Dispose();
        }

        [Test]
        public void CanSave() {
            var myJsonConfig = new MySecureJsonConfig(TestFileName, myFileStorage, myFileEncryption);
            myJsonConfig.Save();

            string stringValue = myJsonConfig.StringValue;
            Guid guid = myJsonConfig.GuidValue;
            int intValue = myJsonConfig.IntValue;
            bool booleanValue = myJsonConfig.BoolValue;

            string jsonString = myFileStorage.Read(TestFileName) ?? string.Empty;
            Assert.That(jsonString, Does.StartWith("1$"));
            jsonString = Encryption.Decrypt(jsonString, myEncryptionKeyProvider.GetEncryptionKey());

            var jsonObject = JsonSerializer.Deserialize<MySecureJsonConfig>(jsonString);
            Assert.That(jsonObject, Is.Not.Null);
            Assert.Multiple(() => {
                Assert.That(jsonObject.BoolValue, Is.EqualTo(booleanValue));
                Assert.That(jsonObject.StringValue, Is.EqualTo(stringValue));
                Assert.That(jsonObject.GuidValue, Is.EqualTo(guid));
                Assert.That(jsonObject.IntValue, Is.EqualTo(intValue));
            });
        }

        [Test]
        public void CanLoad() {
            string stringValue = "MyStringValue";
            Guid guid = new Guid();
            int intValue = new Random().Next(0, 50);
            bool booleanValue = false;

            JsonObject sampleJson = new JsonObject {
                ["StringValue"] = stringValue,
                ["GuidValue"] = guid,
                ["IntValue"] = intValue,
                ["BoolValue"] = booleanValue
            };

            string jsonString = JsonSerializer.Serialize(sampleJson);
            jsonString = Encryption.Encrypt(jsonString, myEncryptionKeyProvider.GetEncryptionKey(), myEncryptionKeyProvider.GetProtections(),
                myEncryptionKeyProvider.GetNonce(), myEncryptionKeyProvider.GetAssociationData(), myEncryptionKeyProvider.GetIV());
            myFileStorage.Write(TestFileName, jsonString);

            var jsonObject = new MySecureJsonConfig(TestFileName, myFileStorage, myFileEncryption);
            Assert.That(jsonObject, Is.Not.Null);
            Assert.Multiple(() => {
                Assert.That(jsonObject.BoolValue, Is.EqualTo(booleanValue));
                Assert.That(jsonObject.StringValue, Is.EqualTo(stringValue));
                Assert.That(jsonObject.GuidValue, Is.EqualTo(guid));
                Assert.That(jsonObject.IntValue, Is.EqualTo(intValue));
            });
        }

        [Test]
        public void SecureAttribute_Validate() {
            string superSecretString = "This is a super secret string: " + Encryption.GenerateRandomString(16);
            var mySecureJsonConfig = new MySecureJsonConfig(TestFileName, myFileStorage, myFileEncryption) {
                StringValue = superSecretString
            };
            mySecureJsonConfig.BoolValue = !mySecureJsonConfig.BoolValue;
            mySecureJsonConfig.Save();

            var myJsonConfig = new MyJsonConfig(TestFileName, myFileStorage, myFileEncryption);
            Assert.Multiple(() => {
                Assert.That(mySecureJsonConfig.DecryptSecret(myJsonConfig.StringValue), Is.EqualTo(mySecureJsonConfig.StringValue), "String is properly encrypted and can be decrypted.");

                mySecureJsonConfig.Load();
                Assert.That(mySecureJsonConfig.StringValue, Is.EqualTo(superSecretString), "SecureJsonConfig can load Secure properties correctly.");
            });
        }

        [Test]
        public void CanProtectSecrets() {
            string superSecretString = "This is a super secret string: " + Encryption.GenerateRandomString(16);

            var myJsonConfig = new MySecureJsonConfig(TestFileName, myFileStorage, myFileEncryption);
            myJsonConfig.StringValue = myJsonConfig.EncryptSecret(superSecretString) ?? string.Empty;
            myJsonConfig.Save();

            string protectedStringValue = myJsonConfig.StringValue;

            var myLoadedJsonConfig = new MySecureJsonConfig(TestFileName, myFileStorage, myFileEncryption);
            Assert.Multiple(() => {
                Assert.That(protectedStringValue, Is.EqualTo(myLoadedJsonConfig.StringValue));
                Assert.That(myLoadedJsonConfig.DecryptSecret(myJsonConfig.StringValue), Is.EqualTo(superSecretString));
            });
        }
    }
}