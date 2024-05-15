using System;
using System.Xml;
using System.Xml.Serialization;

namespace UnifyTestRunner.NUnitResults {
    [Serializable]
    public class NameValuePair {
        [XmlAttribute("name")]
        public string Name { get; set; } = string.Empty;

        [XmlAttribute("value")]
        public string Value { get; set; } = string.Empty;
    }
}
