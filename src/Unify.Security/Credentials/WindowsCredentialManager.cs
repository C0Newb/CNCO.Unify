using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Security.Cryptography;

namespace CNCO.Unify.Security.Credentials {
    /// <summary>
    /// <see cref="ICredentialManager"/> for Windows.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class WindowsCredentialManager : ICredentialManager, ICredentialManagerEndpoint {
        public bool Exists(string credentialName) {
            try {
                return !string.IsNullOrEmpty(Get(credentialName));
            } catch {
                return false; // probably
            }
        }

        public string? Get(string credentialName) {
            // credentialName -> appName
            try {
                bool read = CredRead(credentialName, CredentialType.Generic, 0, out nint credentialPointer);
                if (read) {
                    using (CriticalCredentialHandle credentialHandle = new CriticalCredentialHandle(credentialPointer)) {
                        var credential = credentialHandle.GetCredential();
                        string? secret = null;

                        string? comment = null;
                        if (credential.Comment != IntPtr.Zero)
                            comment = Marshal.PtrToStringUni(credential.Comment);
                        
                        if (credential.CredentialBlob != IntPtr.Zero)
                            secret = Marshal.PtrToStringUni(credential.CredentialBlob, (int)credential.CredentialBlobSize / 2);

                        // tampered?
                        if (!string.IsNullOrEmpty(secret)
                            &&  (string.IsNullOrEmpty(comment) || Hashing.Sha1(secret ?? string.Empty) != comment))
                        {
                            //Remove(credentialName); // tampered, get it outta here!
                            return null;
                        }

                        credentialHandle.Release();
                        return secret;
                    }
                }
            } catch (Exception ex) {
                string tag = $"{GetType().Name}::{nameof(Get)}";

                SecurityRuntime.Current.Log.Error(tag, $"Failed to get {credentialName}.");
                SecurityRuntime.Current.Log.Error(tag, ex.Message);
                SecurityRuntime.Current.Log.Error(tag, ex.StackTrace ?? "No stack trace available.");
            }

            return null;
        }

        public bool Remove(string credentialName) {
            string tag = $"{GetType().Name}::{nameof(Remove)}";

            try {
                if (!Exists(credentialName))
                    return true;
                bool success = CredDelete(credentialName, CredentialType.Generic, 0);
                if (success)
                    return true;

                uint lastError = (uint)Marshal.GetLastWin32Error();
                SecurityRuntime.Current.Log.Error(tag, $"Failed to remove credential, error: {lastError:X}");
            } catch (Exception ex) {
                SecurityRuntime.Current.Log.Error(tag, $"Failed to remove {credentialName}.");
                SecurityRuntime.Current.Log.Error(tag, ex.Message);
                SecurityRuntime.Current.Log.Error(tag, ex.StackTrace ?? "No stack trace available.");
            }
            return false;
        }

        public bool Set(string credentialName, string credentialValue) {
            string tag = $"{GetType().Name}::{nameof(Set)}";

            if (string.IsNullOrEmpty(credentialName))
                throw new ArgumentNullException(nameof(credentialName));
            if (string.IsNullOrEmpty(credentialValue))
                throw new ArgumentNullException(nameof(credentialValue));

            try {
                // we'll use the comment as a checksum, this prevents user tampering via Control Panel.
                string comment = Hashing.Sha1(credentialValue);
                // sha1 as this doesn't have to be anything crazy.
                
                var secretLength = credentialValue.Length * UnicodeEncoding.CharSize;
                int maxLength = 2560;
                if (Environment.OSVersion.Version < new Version(6, 1)) // <Win7
                    maxLength = 512;
                if (secretLength > maxLength)
                    throw new ArgumentOutOfRangeException(nameof(credentialValue), $"The credential has exceeded {maxLength} bytes.");


                var credentialNamePtr = Marshal.StringToBSTR(credentialName);
                var valuePtr = Marshal.StringToBSTR(credentialValue);
                var commentPtr = Marshal.StringToBSTR(comment);
                try {
                    var credential = new CREDENTIAL {
                        AttributeCount = 0u,
                        Comment = commentPtr,
                        TargetAlias = default,
                        Type = CredentialType.Generic,
                        Persist = (uint)CredentialPersistence.Enterprise,
                        CredentialBlobSize = (uint)secretLength,
                        TargetName = credentialNamePtr,
                        CredentialBlob = valuePtr,
                    };

                    if (CredWrite(ref credential, 0))
                        return true;

                    uint lastError = (uint)Marshal.GetLastWin32Error();
                    SecurityRuntime.Current.Log.Error(tag, $"Failed to set credential, error: {lastError:X}");
                } finally {
                    Marshal.ZeroFreeBSTR(credentialNamePtr); // Release pointers
                    Marshal.ZeroFreeBSTR(valuePtr);
                    Marshal.ZeroFreeBSTR(commentPtr);
                }
            } catch (Exception ex) {
                SecurityRuntime.Current.Log.Error(tag, $"Failed to set {credentialName}.");
                SecurityRuntime.Current.Log.Error(tag, ex.Message);
                SecurityRuntime.Current.Log.Error(tag, ex.StackTrace ?? "No stack trace available.");
            }
            return false;
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
