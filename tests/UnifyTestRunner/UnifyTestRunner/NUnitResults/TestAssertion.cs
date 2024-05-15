using System.Xml;
using System.Xml.Serialization;

namespace UnifyTestRunner.NUnitResults {
    public class TestAssertion {
        [XmlAttribute("result")]
        public TestResult Result { get; set; }

        [XmlElement("message")]
        public string Message { get; set; } = string.Empty;

        [XmlElement("stack-trace")]
        public string? StackTrace { get; set; }
    }
}
