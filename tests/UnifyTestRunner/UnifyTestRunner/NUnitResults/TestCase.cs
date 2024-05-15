using System;
using System.Xml;
using System.Xml.Serialization;

namespace UnifyTestRunner.NUnitResults {
    [Serializable, XmlRoot("test-case")]
    public class TestCase : TestBase {
        [XmlAttribute("label")]
        public string? Label { get; set; }

        [XmlAttribute("site")]
        public string Site { get; set; } = "Test";

        [XmlElement("failure", typeof(TestFailure))]
        public TestFailure? Failure { get; set; }
    }
}
