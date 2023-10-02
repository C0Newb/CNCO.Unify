using CNCO.Unify.Configuration.Encryption;
using CNCO.Unify.Configuration.Storage;
using CNCO.Unify.Security;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace UnifyTests.Configuration.Json {
    public class JsonConfiguration {

        private const string TestFileName = "jsonConfigurationTest.json";
        private LocalFileStorage myFileStorage;


        [SetUp]
        public void Setup() {
            myFileStorage = new LocalFileStorage();
        }

        [TearDown]
        public void TearDown() {
            myFileStorage.Delete(TestFileName);
        }

        [Test]
        public void CanSave() {
            MyJsonConfig myJsonConfig = new MyJsonConfig(TestFileName, myFileStorage);
            myJsonConfig.Save();

            string stringValue = myJsonConfig.StringValue;
            Guid guid = myJsonConfig.GuidValue;
            int intValue = myJsonConfig.IntValue;
            bool booleanValue = myJsonConfig.BoolValue;

            var fileText = File.ReadAllText(TestFileName);
            var jsonObject = JsonSerializer.Deserialize<MyJsonConfig>(fileText);

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


            File.WriteAllText(TestFileName, JsonSerializer.Serialize(sampleJson));
            MyJsonConfig jsonObject = new MyJsonConfig(TestFileName, myFileStorage);

            Assert.IsNotNull(jsonObject);
            Assert.That(jsonObject.BoolValue, Is.EqualTo(booleanValue));
            Assert.That(jsonObject.StringValue, Is.EqualTo(stringValue));
            Assert.That(jsonObject.GuidValue, Is.EqualTo(guid));
            Assert.That(jsonObject.IntValue, Is.EqualTo(intValue));
        }
    }
}