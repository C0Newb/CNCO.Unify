using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace UnifyTestRunner.NUnitResults {
    public class TestReason {
        [XmlElement("message")]
        public List<string> Messages { get; set; } = [];
    }
}
