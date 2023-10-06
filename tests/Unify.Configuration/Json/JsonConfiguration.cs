using CNCO.Unify.Storage;
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
            _ = myFileStorage.Delete(TestFileName);
        }

        [Test]
        public void CanSave() {
            var myJsonConfig = new MyJsonConfig(TestFileName, myFileStorage);
            myJsonConfig.Save();

            string stringValue = myJsonConfig.StringValue;
            Guid guid = myJsonConfig.GuidValue;
            int intValue = myJsonConfig.IntValue;
            bool booleanValue = myJsonConfig.BoolValue;

            var fileText = File.ReadAllText(TestFileName);
            var jsonObject = JsonSerializer.Deserialize<MyJsonConfig>(fileText);

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
            var guid = new Guid();
            int intValue = new Random().Next(0, 50);
            bool booleanValue = false;

            var sampleJson = new JsonObject {
                ["StringValue"] = stringValue,
                ["GuidValue"] = guid,
                ["IntValue"] = intValue,
                ["BoolValue"] = booleanValue
            };


            File.WriteAllText(TestFileName, JsonSerializer.Serialize(sampleJson));
            var jsonObject = new MyJsonConfig(TestFileName, myFileStorage);

            Assert.That(jsonObject, Is.Not.Null);
            Assert.Multiple(() => {
                Assert.That(jsonObject.BoolValue, Is.EqualTo(booleanValue));
                Assert.That(jsonObject.StringValue, Is.EqualTo(stringValue));
                Assert.That(jsonObject.GuidValue, Is.EqualTo(guid));
                Assert.That(jsonObject.IntValue, Is.EqualTo(intValue));
            });
        }
    }
}