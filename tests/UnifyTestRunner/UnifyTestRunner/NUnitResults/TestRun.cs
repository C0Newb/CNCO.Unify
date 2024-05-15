using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace UnifyTestRunner.NUnitResults {
    [Serializable, XmlRoot("test-run", IsNullable = false)]
    public class TestRun {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlAttribute("runstate")]
        public RunState RunState { get; set; } = RunState.Ignored;

        [XmlAttribute("testcasecount")]
        public int TestCaseCount { get; set; }

        [XmlAttribute("result")]
        public TestResult Result { get; set; } = TestResult.Error;

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

        [XmlAttribute("asserts")]
        public int Asserts { get; set; }

        [XmlAttribute("engine-version")]
        public string? EngineVersion { get; set; }

        [XmlAttribute("clr-version")]
        public string? ClrVersion { get; set; }

        [XmlAttribute("start-time")]
        public string StartTime { get; set; } = string.Empty;

        [XmlAttribute("end-time")]
        public string EndTime { get; set; } = string.Empty;

        [XmlAttribute("duration")]
        public double Duration { get; set; }

        [XmlElement("command-line")]
        public string? CommandLine { get; set; }

        [XmlElement("test-suite", typeof(TestSuite))]
        public List<TestSuite> Tests { get; set; } = [];

        [XmlElement("filter")]
        public XmlNode? Filter { get; set; }

        public TestCase[] GetTestCases() {
            List<TestCase> testCases = [];
            foreach (var test in Tests) {
                if (test == null)
                    continue;

                testCases.AddRange(test.GetTestCases());
            }
            return testCases.ToArray();
        }
    }
}
