using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace CNCO.Unify.Security.Credentials {
    /// <summary>
    /// <see cref="ICredentialManager"/> for Windows.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class WindowsCredentialManager : ICredentialManager, ICredentialManagerEndpoint {
        private readonly object _lock = new object();

        public bool Exists(string credentialName) {
            try {
                return !string.IsNullOrEmpty(Get(credentialName));
            } catch {
                return false; // probably
            }
        }

        public string? Get(string credentialName) {
            // credentialName -> appName
            string tag = $"{GetType().Name}::{nameof(Get)}";
            try {
                lock (_lock) {
                    bool read = CredRead(credentialName, CredentialType.Generic, 0, out nint credentialPointer);
                    if (!read)
                        return null;

                    using (CriticalCredentialHandle credentialHandle = new CriticalCredentialHandle(credentialPointer)) {
                        var credential = credentialHandle.GetCredential();
                        credentialHandle.Release();

                        /*
                        string? hash = null;
                        if (credential.UserName != IntPtr.Zero)
                            hash = Marshal.PtrToStringUni(credential.UserName);
                        */

                        string? secret = null;
                        if (credential.CredentialBlob != IntPtr.Zero)
                            secret = Marshal.PtrToStringUni(credential.CredentialBlob, (int)credential.CredentialBlobSize / 2);

                        if (secret == null)
                            return null;

                        // tampered?
                        return CredentialHelpers.GetAndVerifyCredential(secret);

                        /*
                        bool hashNull = string.IsNullOrEmpty(hash);
                        bool hashMatches = Hashing.Sha512(secret ?? string.Empty) == hash;
                        if (!string.IsNullOrEmpty(secret) && (hashNull || !hashMatches)) { // Credential exists, hash invalid
                            SecurityRuntime.Current.Log.Verbose(tag, $"actualHash null? {hashNull}. Hash ok? {hashMatches}");
                            SecurityRuntime.Current.Log.Error(tag, $"Credential \"{credentialName}\"'s contents have been modified!");
                            throw new CredentialTamperException("Credential has been tampered with and is invalid.");
                        }
                        return secret;
                        */
                    }
                }
            } catch (Exception ex) {
                SecurityRuntime.Current.Log.Error(tag, $"Failed to get {credentialName}");
                SecurityRuntime.Current.Log.Error(tag, ex.Message);
                SecurityRuntime.Current.Log.Error(tag, ex.StackTrace ?? "No stack trace available.");

                throw;
            }
        }

        public void Remove(string credentialName) {
            string tag = $"{GetType().Name}::{nameof(Remove)}";

            try {
                if (!Exists(credentialName))
                    return;

                lock (_lock) {
                    if (CredDelete(credentialName, CredentialType.Generic, 0))
                        return;

                    uint lastError = (uint)Marshal.GetLastWin32Error();
                    throw new InvalidOperationException($"Failed to remove credential, error: {lastError:X}");
                }
            } catch (Exception ex) {
                SecurityRuntime.Current.Log.Error(tag, $"Failed to remove {credentialName}.");
                SecurityRuntime.Current.Log.Error(tag, ex.Message);
                SecurityRuntime.Current.Log.Error(tag, ex.StackTrace ?? "No stack trace available.");

                throw;
            }
        }

        public void Set(string credentialName, string value) {
            string tag = $"{GetType().Name}::{nameof(Set)}";

            if (string.IsNullOrEmpty(credentialName))
                throw new ArgumentNullException(nameof(credentialName));
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));

            try {
                value = CredentialHelpers.ApplyTamperHash(value);
                var secretLength = value.Length * UnicodeEncoding.CharSize;
                int maxLength = 2560;
                if (Environment.OSVersion.Version < new Version(6, 1)) // <Win7
                    maxLength = 512;
                if (secretLength > maxLength)
                    throw new ArgumentOutOfRangeException(nameof(value), $"The credential has exceeded {maxLength} bytes.");


                var credentialNamePtr = Marshal.StringToBSTR(credentialName);
                var valuePtr = Marshal.StringToBSTR(value);
                try {
                    var credential = new CREDENTIAL {
                        AttributeCount = 0u,
                        TargetAlias = default,
                        Type = CredentialType.Generic,
                        Persist = (uint)CredentialPersistence.Enterprise,
                        CredentialBlobSize = (uint)secretLength,
                        TargetName = credentialNamePtr,
                        CredentialBlob = valuePtr,
                    };
                    lock (_lock) {
                        if (CredWrite(ref credential, 0))
                            return;
                        uint lastError = (uint)Marshal.GetLastWin32Error();
                        throw new InvalidOperationException($"Failed to set credential, error: {lastError:X}");
                    }
                } finally {
                    Marshal.ZeroFreeBSTR(credentialNamePtr); // Release pointers
                    Marshal.ZeroFreeBSTR(valuePtr);
                }
            } catch (Exception ex) {
                SecurityRuntime.Current.Log.Error(tag, $"Failed to set {credentialName}.");
                SecurityRuntime.Current.Log.Error(tag, ex.Message);
                SecurityRuntime.Current.Log.Error(tag, ex.StackTrace ?? "No stack trace available.");

                throw;
            }
        }





        // PInvoke
        [DllImport("Advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool CredRead(string target, CredentialType type, int reservedFlag, out IntPtr credentialPtr);

        [DllImport("Advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool CredWrite([In] ref CREDENTIAL userCredential, [In] UInt32 flags);

        [DllImport("Advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool CredDelete(string target, CredentialType type, int reservedFlag);

        [DllImport("advapi32", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool CredEnumerate(string filter, int flag, out int count, out IntPtr pCredentials);

        [DllImport("Advapi32.dll", EntryPoint = "CredFree", SetLastError = true)]
        static extern bool CredFree([In] IntPtr cred);



        /// <summary>
        /// See <see href="https://learn.microsoft.com/en-us/windows/win32/api/wincred/ns-wincred-credentiala"/>
        /// </summary>
        private enum CredentialPersistence : uint {
            /// <summary>
            /// The credential persists for the life of the logon session.
            /// It will not be visible to other logon sessions of this same user.
            /// It will not exist after this user logs off and back on.
            /// </summary>
            Session = 1,

            /// <summary>
            /// The credential persists for all subsequent logon sessions on this same computer.
            /// It is visible to other logon sessions of this same user on this same computer and not visible to logon sessions for this user on other computers.
            /// </summary>
            LocalMachine = 2,

            /// <summary>
            /// The credential persists for all subsequent logon sessions on this same computer.
            /// It is visible to other logon sessions of this same user on this same computer and to logon sessions for this user on other computers.
            /// </summary>
            /// <remarks>
            /// This option can be implemented as locally persisted credential if the administrator or user configures the user account to not have roam-able state.
            /// For instance, if the user has no roaming profile, the credential will only persist locally.
            /// </remarks>
            Enterprise = 3,
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CREDENTIAL {
            public uint Flags;
            public CredentialType Type;
            public IntPtr TargetName;
            public IntPtr Comment;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
            public uint CredentialBlobSize;
            public IntPtr CredentialBlob;
            public uint Persist;
            public uint AttributeCount;
            public IntPtr Attributes;
            public IntPtr TargetAlias;
            public IntPtr UserName;
        }

        sealed class CriticalCredentialHandle : CriticalHandleZeroOrMinusOneIsInvalid {
            public CriticalCredentialHandle(IntPtr preexistingHandle) {
                SetHandle(preexistingHandle);
            }

            public CREDENTIAL GetCredential() {
                if (!IsInvalid) {
                    var credential = Marshal.PtrToStructure(handle, typeof(CREDENTIAL)) ?? throw new InvalidOperationException("Invalid CriticalHandle!");
                    return (CREDENTIAL)credential;
                }

                throw new InvalidOperationException("Invalid CriticalHandle!");
            }

            protected override bool ReleaseHandle() {
                if (!IsInvalid) {
                    CredFree(handle);
                    SetHandleAsInvalid();
                    return true;
                }

                return false;
            }

            public void Release() => ReleaseHandle();
        }

        /// <summary>
        /// See <see href="https://learn.microsoft.com/en-us/windows/win32/api/wincred/ns-wincred-credentiala"/>
        /// </summary>
        public enum CredentialType {
            /// <summary>
            /// The credential is a generic credential.
            /// The credential will not be used by any particular authentication package.
            /// The credential will be stored securely but has no other significant characteristics.
            /// </summary>
            Generic = 1,

            /// <summary>
            /// The credential is a password credential and is specific to Microsoft's authentication packages.
            /// The NTLM, Kerberos, and Negotiate authentication packages will automatically use this credential when connecting to the named target.
            /// </summary>
            DomainPassword,

            /// <summary>
            /// The credential is a certificate credential and is specific to Microsoft's authentication packages.
            /// The Kerberos, Negotiate, and Schannel authentication packages automatically use this credential when connecting to the named target.
            /// </summary>
            DomainCertificate,

            // No longer used.
            DomainVisiblePassword,
            // No longer used.
            GenericCertificate,
            // No longer used.
            DomainExtended,
            // No longer used.
            Maximum,
            // No longer used.
            MaximumEx = Maximum + 1000,
        }
    }
}
