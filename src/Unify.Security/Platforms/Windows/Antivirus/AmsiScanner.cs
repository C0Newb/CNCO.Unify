using CNCO.Unify.Security.Platforms.Windows.Antivirus.Internals;
using System.ComponentModel;
using System.Diagnostics;

namespace CNCO.Unify.Security.Platforms.Windows.Antivirus {
    // This needs work.
    public class AmsiScanner : IDisposable {
        private readonly AmsiContext amsiContext;
        private readonly AmsiSession amsiSession;


        public AmsiScanner() {
            try {
                using (var process = Process.GetCurrentProcess()) {
                    amsiContext = AmsiContext.Create($"{AppDomain.CurrentDomain.FriendlyName} (PID: {process.Id})");
                }

                amsiSession = amsiContext.CreateSession();
            } catch (Win32Exception) {
                throw AmsiException.FailedToInitialize();
            }

            // AttachmentService here...
        }


        /// <summary>
        /// Scans a string using the AMSI Win32 API.
        /// 
        /// Note, this has a max input restriction of 16MBs!
        /// </summary>
        /// <param name="data">String to scan.</param>
        /// <param name="contentName">Optional name of the string your scanning. Used as the "file name" for most antivirus scanners.</param>
        /// <returns>The results of the scan.</returns>
        public ScanResult ScanString(string data, string? contentName) {
            DateTime startTime = DateTime.Now;

            var result = amsiSession.Scan(data, contentName ?? string.Empty);

            var scanResult = new ScanResult() {
                IsSafe = result == AmsiResult.AMSI_RESULT_CLEAN || result == AmsiResult.AMSI_RESULT_NOT_DETECTED,
                TimeStamp = startTime,
            };
            switch (result) {
                case AmsiResult.AMSI_RESULT_CLEAN:
                    scanResult.Result = DetectionResult.Clean;
                    break;
                case AmsiResult.AMSI_RESULT_NOT_DETECTED:
                    scanResult.Result = DetectionResult.NotDetected;
                    break;
                case AmsiResult.AMSI_RESULT_BLOCKED_BY_ADMIN_START:
                case AmsiResult.AMSI_RESULT_BLOCKED_BY_ADMIN_END:
                    scanResult.Result = DetectionResult.BlockedByAdministrator;
                    break;

                case AmsiResult.AMSI_RESULT_DETECTED:
                    scanResult.Result = DetectionResult.IdentifiedAsMalware;
                    break;
            }

            return scanResult;
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
            amsiSession.Dispose();
            amsiContext.Dispose();
        }
    }
}
