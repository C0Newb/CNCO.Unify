using System;
using System.Xml;
using System.Xml.Serialization;

namespace UnifyTestRunner.NUnitResults {
    [Serializable]
    public class TestEnvironment {
        [XmlAttribute("framework-version")]
        public string? FrameworkVersion { get; set; }

        [XmlAttribute("clr-version")]
        public string? ClrVersion { get; set; }

        [XmlAttribute("os-version")]
        public string? OsVersion { get; set; }

        [XmlAttribute("platform")]
        public string? Platform { get; set; }

        [XmlAttribute("cwd")]
        public string? Cwd { get; set; }

        [XmlAttribute("machine-name")]
        public string? MachineName { get; set; }

        [XmlAttribute("user")]
        public string? User { get; set; }

        [XmlAttribute("user-domain")]
        public string? UserDomain { get; set; }

        [XmlAttribute("culture")]
        public string? Culture { get; set; }

        [XmlAttribute("uiculture")]
        public string? UiCulture { get; set; }

        [XmlAttribute("os-architecture")]
        public string? OsArchitecture { get; set; }
    }
}
