using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CNCO.Unify.Security.Antivirus.Internals {
    internal static class Amsi {
        internal const string AmsiDllName = "Amsi.dll";


        internal static bool AmsiResultIsMalware(AmsiResult result) => result >= AmsiResult.AMSI_RESULT_DETECTED;

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport(AmsiDllName, EntryPoint = "AmsiInitialize", CallingConvention = CallingConvention.StdCall)]
        internal static extern int AmsiInitialize([MarshalAs(UnmanagedType.LPWStr)] string appName, out AmsiContextSafeHandle amsiContext);

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport(AmsiDllName, EntryPoint = "AmsiUninitialize", CallingConvention = CallingConvention.StdCall)]
        internal static extern void AmsiUninitialize(IntPtr amsiContext);

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport(AmsiDllName, EntryPoint = "AmsiOpenSession", CallingConvention = CallingConvention.StdCall)]
        internal static extern int AmsiOpenSession(AmsiContextSafeHandle amsiContext, out AmsiSessionSafeHandle session);

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport("Amsi.dll", EntryPoint = "AmsiCloseSession", CallingConvention = CallingConvention.StdCall)]
        internal static extern void AmsiCloseSession(AmsiContextSafeHandle amsiContext, IntPtr session);

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport(AmsiDllName, EntryPoint = "AmsiScanString", CallingConvention = CallingConvention.StdCall)]
        internal static extern int AmsiScanString(AmsiContextSafeHandle amsiContext, [In, MarshalAs(UnmanagedType.LPWStr)] string payload, [In, MarshalAs(UnmanagedType.LPWStr)] string contentName, AmsiSessionSafeHandle session, out AmsiResult result);

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport(AmsiDllName, EntryPoint = "AmsiScanBuffer", CallingConvention = CallingConvention.StdCall)]
        internal static extern int AmsiScanBuffer(AmsiContextSafeHandle amsiContext, byte[] buffer, uint length, [In, MarshalAs(UnmanagedType.LPWStr)] string contentName, AmsiSessionSafeHandle session, out AmsiResult result);


        internal static bool IsDllImportPossible() {
            try {
                Marshal.PrelinkAll(typeof(Amsi));
                return true;
            } catch {
                return false;
            }
        }
    }

    internal enum AmsiResult {
        AMSI_RESULT_CLEAN = 0,
        AMSI_RESULT_NOT_DETECTED = 1,
        AMSI_RESULT_BLOCKED_BY_ADMIN_START = 16384,
        AMSI_RESULT_BLOCKED_BY_ADMIN_END = 20479,
        AMSI_RESULT_DETECTED = 32768,
    }

    internal sealed class AmsiContextSafeHandle : SafeHandleZeroOrMinusOneIsInvalid {
        public AmsiContextSafeHandle() : base(ownsHandle: true) { }
        protected override bool ReleaseHandle() {
            Amsi.AmsiUninitialize(handle);
            return true;
        }
    }

    internal sealed class AmsiSessionSafeHandle : SafeHandleZeroOrMinusOneIsInvalid {
        internal AmsiContextSafeHandle? Context { get; set; }
        public AmsiSessionSafeHandle() : base(ownsHandle: true) { }
        public override bool IsInvalid => Context == null || Context.IsInvalid || base.IsInvalid;
        protected override bool ReleaseHandle() {
            Debug.Assert(Context != null);
            Amsi.AmsiCloseSession(Context, handle);
            return true;
        }
    }
}
