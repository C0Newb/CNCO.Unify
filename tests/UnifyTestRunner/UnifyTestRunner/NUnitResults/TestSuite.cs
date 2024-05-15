using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace UnifyTestRunner.NUnitResults {
    [Serializable, XmlRoot("test-suite")]
    public class TestSuite : TestBase {
        [XmlAttribute("testcasecount")]
        public int TestCaseCount { get; set; }
        [XmlAttribute("total")]
        public int Total { get; set; }

        [XmlAttribute("passed")]
        public int Passed { get; set; }

        [XmlAttribute("failed")]
        public int Failed { get; set; }

        [XmlAttribute("warnings")]
        public int Warnings { get; set; }

        [XmlAttribute("inconclusive")]
        public int Inconclusive { get; set; }

        [XmlAttribute("skipped")]
        public int Skipped { get; set; }

        [XmlElement("environment", typeof(TestEnvironment))]
        public TestEnvironment? Environment { get; set; }

        [XmlArray(ElementName = "settings"), XmlArrayItem("setting", typeof(NameValuePair), IsNullable = false)]
        public List<NameValuePair> Settings { get; set; } = [];

        [XmlElement("test-suite", typeof(TestSuite))]
        [XmlElement("test-case", typeof(TestCase))]
        public List<TestBase> Tests { get; set; } = [];


        public TestCase[] GetTestCases() {
            List<TestCase> testCases = [];
            foreach (var test in Tests) {
                if (test == null)
                    continue;

                if (test.GetType() == typeof(TestCase)) {
                    testCases.Add((TestCase)test);
                } else {
                    testCases.AddRange(((TestSuite)test).GetTestCases());
                }
            }
            return testCases.ToArray();
        }
    }
}
