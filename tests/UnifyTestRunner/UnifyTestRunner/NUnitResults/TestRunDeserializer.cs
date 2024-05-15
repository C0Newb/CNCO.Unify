using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace UnifyTestRunner.NUnitResults {
    public class TestRunDeserializer {
        public static TestRun? DeserializeTestRun(string xmlString) {
            // Deserialize the XML string into a TestRun object
            var serializer = new XmlSerializer(typeof(TestRun));
            using (var stringReader = new StringReader(xmlString)) {
                return (TestRun?)serializer.Deserialize(stringReader);
            }
        }

        public static TestRun? DeserializeTestRun(XmlNode xml) => DeserializeTestRun(xml.OuterXml);
    }
}
