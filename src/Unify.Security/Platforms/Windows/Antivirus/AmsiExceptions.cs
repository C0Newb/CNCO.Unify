using CNCO.Unify.Security.Platforms.Windows.Antivirus.Internals;

namespace CNCO.Unify.Security.Platforms.Windows.Antivirus {
    public class AmsiException : Exception {
        public AmsiException() { }
        public AmsiException(string message) : base(message) { }
        public AmsiException(string message, Exception innerException) : base(message, innerException) { }

        public static AmsiException AmsiNotFound => new AmsiException($"Amsi ({Amsi.AmsiDllName}) was not found on this machine");
        public static AmsiException AmsiFailedToExecute(string methodName, Exception innerException) => new AmsiException($"Amsi method {methodName} failed to execute", innerException);
        public static AmsiException AmsiInvalidState => new AmsiException("Amsi is in invalid state to perform operation. Check configuration of available Amsi providers");
        public static AmsiException NoDetectionEngineFound => new AmsiException("No detection engine found. Amsi call cannot be executed");
        public static AmsiException FailedToInitialize() => new AmsiException("Amsi failed to initialize");
        public static AmsiException FailedToInitializeSession() => new AmsiException("Amsi failed to initialize session");
        public static AmsiException UnsupportedOperation() => new AmsiException("This operation is not supported");
    }
}
