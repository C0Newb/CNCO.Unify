using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace UnifyTestRunner.NUnitResults {
    [Serializable]
    public class TestBase {
        [XmlAttribute("id")]
        public string Id { get; set; } = string.Empty;

        [XmlAttribute("name")]
        public string Name { get; set; } = string.Empty;

        [XmlAttribute("fullname")]
        public string FullName { get; set; } = string.Empty;

        [XmlAttribute("methodname")]
        public string MethodName { get; set; } = string.Empty;

        [XmlAttribute("classname")]
        public string ClassName { get; set; } = string.Empty;

        [XmlAttribute("runstate")]
        public RunState RunState { get; set; } = RunState.Ignored;

        [XmlAttribute("type")]
        public string Type { get; set; } = string.Empty;

        [XmlAttribute("seed")]
        public int Seed { get; set; } = 0;

        [XmlAttribute("result")]
        public TestResult Result { get; set; } = TestResult.Error;

        [XmlAttribute("start-time")]
        public string StartTime { get; set; } = string.Empty;

        [XmlAttribute("end-time")]
        public string EndTime { get; set; } = string.Empty;

        [XmlAttribute("duration")]
        public double Duration { get; set; } = 0;

        [XmlAttribute("asserts")]
        public int Asserts { get; set; } = 0;

        [XmlArray(ElementName = "properties"), XmlArrayItem("property", typeof(NameValuePair), IsNullable = false)]
        public List<NameValuePair> Properties { get; set; } = [];

        [XmlArray(ElementName = "assertions"), XmlArrayItem("assertion", typeof(TestAssertion), IsNullable = false)]
        public List<TestAssertion> Assertions { get; set; } = [];

        [XmlElement("output")]
        public string? Output { get; set; }

        [XmlElement("reason")]
        public TestReason? Reason { get; set; }
    }
}
