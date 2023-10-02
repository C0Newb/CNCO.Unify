using CNCO.Unify.Configuration.Encryption;
using CNCO.Unify.Configuration.Storage;
using CNCO.Unify.Security;
using NUnit.Framework.Constraints;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace UnifyTests.Configuration.Json {
    public class SecureJsonConfiguration {
        private const string TestFileName = "jsonConfigurationTest.json";
        private MyEncryptionKeyProvider myEncryptionKeyProvider;
        private GenericFileEncryption myFileEncryption;
        private LocalFileStorage myFileStorage;


        [SetUp]
        public void Setup() {
            myEncryptionKeyProvider = new MyEncryptionKeyProvider();
            myFileEncryption = new GenericFileEncryption(myEncryptionKeyProvider);
            myFileStorage = new LocalFileStorage();
        }

        [TearDown]
        public void TearDown() {
            myFileStorage.Delete(TestFileName);
        }

        [Test]
        public void CanSave() {
            var myJsonConfig = new MySecureJsonConfig(TestFileName, myFileStorage, myFileEncryption);
            myJsonConfig.Save();

            string stringValue = myJsonConfig.StringValue;
            Guid guid = myJsonConfig.GuidValue;
            int intValue = myJsonConfig.IntValue;
            bool booleanValue = myJsonConfig.BoolValue;

            string jsonString = File.ReadAllText(TestFileName);
            Assert.That(jsonString.StartsWith("1$"));
            jsonString = Encryption.Decrypt(jsonString, myEncryptionKeyProvider.GetEncryptionKey());

            var jsonObject = JsonSerializer.Deserialize<MySecureJsonConfig>(jsonString);
            Assert.IsNotNull(jsonObject);
            Assert.That(jsonObject.BoolValue, Is.EqualTo(booleanValue));
            Assert.That(jsonObject.StringValue, Is.EqualTo(stringValue));
            Assert.That(jsonObject.GuidValue, Is.EqualTo(guid));
            Assert.That(jsonObject.IntValue, Is.EqualTo(intValue));
        }

        [Test]
        public void CanLoad() {
            string stringValue = "MyStringValue";
            Guid guid = new Guid();
            int intValue = new Random().Next(0, 50);
            bool booleanValue = false;

            JsonObject sampleJson = new JsonObject();
            sampleJson["StringValue"] = stringValue;
            sampleJson["GuidValue"] = guid;
            sampleJson["IntValue"] = intValue;
            sampleJson["BoolValue"] = booleanValue;

            string jsonString = JsonSerializer.Serialize(sampleJson);
            jsonString = Encryption.Encrypt(jsonString, myEncryptionKeyProvider.GetEncryptionKey(), myEncryptionKeyProvider.GetProtections(),
                myEncryptionKeyProvider.GetNonce(), myEncryptionKeyProvider.GetAssociationData(), myEncryptionKeyProvider.GetIV());
            File.WriteAllText(TestFileName, jsonString);

            var jsonObject = new MySecureJsonConfig(TestFileName, myFileStorage, myFileEncryption);
            Assert.IsNotNull(jsonObject);
            Assert.That(jsonObject.BoolValue, Is.EqualTo(booleanValue));
            Assert.That(jsonObject.StringValue, Is.EqualTo(stringValue));
            Assert.That(jsonObject.GuidValue, Is.EqualTo(guid));
            Assert.That(jsonObject.IntValue, Is.EqualTo(intValue));
        }

        [Test]
        public void CanProtectSecrets() {
            string superSecretString = "This is a super secret string: " + Encryption.GenerateRandomString(16);

            var myJsonConfig = new MySecureJsonConfig(TestFileName, myFileStorage, myFileEncryption);
            myJsonConfig.StringValue = myJsonConfig.EncryptSecret(superSecretString);
            myJsonConfig.Save();

            string protectedStringValue = myJsonConfig.StringValue;

            var myLoadedJsonConfig = new MySecureJsonConfig(TestFileName, myFileStorage, myFileEncryption);
            Assert.That(protectedStringValue, Is.EqualTo(myLoadedJsonConfig.StringValue));
            Assert.That(myLoadedJsonConfig.DecryptSecret(myJsonConfig.StringValue), Is.EqualTo(superSecretString));
        }
    }
}