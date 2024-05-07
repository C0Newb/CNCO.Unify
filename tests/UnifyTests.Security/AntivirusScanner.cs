/*using CNCO.Unify.Security.Antivirus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifyTests.Security {
    public class AntivirusScanner {
        private CNCO.Unify.Security.Antivirus.AmsiScanner Scanner = new CNCO.Unify.Security.Antivirus.AmsiScanner();

        [SetUp]
        public void Setup() { }

        [TearDown]
        public void TearDown() { }

        [Test]
        public void TestAmsiScanString() {
            var result = Scanner.ScanString(@"X5O!P%@AP[4\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*", "EICAR");
            Assert.That(result.IsSafe, Is.False);
            Assert.That(result.Result, Is.EqualTo(DetectionResult.IdentifiedAsMalware));

            var clean = Scanner.ScanString("00112233", "EICAR");
            Assert.That(clean.IsSafe, Is.True);
            bool isCleanResult = (clean.Result == DetectionResult.Clean) || (clean.Result == DetectionResult.NotDetected);
            Assert.True(isCleanResult);
        }
    }
}
*/
