using CNCO.Unify.Security.Antivirus.Internals;
using System.ComponentModel;

namespace CNCO.Unify.Security.Antivirus {
    public sealed class AmsiSession : IDisposable {
        private readonly AmsiContextSafeHandle _context;
        private readonly AmsiSessionSafeHandle _session;

        internal AmsiSession(AmsiContextSafeHandle context, AmsiSessionSafeHandle session) {
            _context = context;
            _session = session;
        }

        internal AmsiResult Scan(string content, string contentName) {
            var returnValue = Amsi.AmsiScanString(_context, content, contentName, _session, out var result);
            if (returnValue != 0)
                throw new Win32Exception(returnValue);

            return result;
        }
        internal AmsiResult Scan(byte[] content, string contentName) {
            var returnValue = Amsi.AmsiScanBuffer(_context, content, (uint)content.Length, contentName, _session, out var result);
            if (returnValue != 0)
                throw new Win32Exception(returnValue);

            return result;
        }

        public bool IsMalware(string content, string contentName) => Amsi.AmsiResultIsMalware(Scan(content, contentName));
        public bool IsMalware(byte[] content, string contentName) => Amsi.AmsiResultIsMalware(Scan(content, contentName));

        

        public static bool IsAvailable() => Amsi.IsDllImportPossible();

        public void Dispose() {
            _session.Dispose();
        }
    }
}
