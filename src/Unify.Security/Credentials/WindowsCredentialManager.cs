using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace CNCO.Unify.Security.Credentials {
    /// <summary>
    /// <see cref="ICredentialManager"/> for Windows.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class WindowsCredentialManager : ICredentialManager {
        public bool Exists(string credentialName) => throw new NotImplementedException();

        public string? Get(string credentialName) {
            // credentialName -> appName
            try {
                bool read = CredRead(credentialName, CredentialType.Generic, 0, out nint credentialPointer);
                if (read) {
                    using (CriticalCredentialHandle credentialHandle = new CriticalCredentialHandle(credentialPointer)) {
                        var credential = credentialHandle.GetCredential();
                        string? secret = null;
                        
                        if (credential.CredentialBlob != IntPtr.Zero)
                            secret = Marshal.PtrToStringUni(credential.CredentialBlob, (int)credential.CredentialBlobSize / 2);

                        return secret;
                    }
                }
            } catch (Exception e) {
                Runtime.Current.ApplicationLog.Error($"Unify Security: {GetType().Name}", e.Message);
            }

            return null;
        }

        public bool Remove(string credentialName) => throw new NotImplementedException();

        public bool Set(string credentialName, string value) => throw new NotImplementedException();



        // PInvoke
        [DllImport("Advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool CredRead(string target, CredentialType type, int reservedFlag, out IntPtr credentialPtr);

        [DllImport("Advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool CredWrite([In] ref CREDENTIAL userCredential, [In] UInt32 flags);

        [DllImport("advapi32", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool CredEnumerate(string filter, int flag, out int count, out IntPtr pCredentials);

        [DllImport("Advapi32.dll", EntryPoint = "CredFree", SetLastError = true)]
        static extern bool CredFree([In] IntPtr cred);

        private enum CredentialPersistence : uint {
            Session = 1,
            LocalMachine,
            Enterprise
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
        }

        /// <summary>
        /// https://learn.microsoft.com/en-us/windows/win32/api/wincred/ns-wincred-credentiala
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
