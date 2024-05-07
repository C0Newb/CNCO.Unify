#if ANDROID
using Android.Content;
using AndroidX.Security.Crypto;
using java.io;
using java.security;
using java.security.spec;
using javax.crypto;
using System.Text;
#endif

using System.Runtime.Versioning;

namespace CNCO.Unify.Security.Credentials {
    /// <summary>
    /// <see cref="ICredentialManager"/> for Android.
    /// </summary>
    [SupportedOSPlatform("android")]
    public class AndroidCredentialManager : ICredentialManager, ICredentialManagerEndpoint {
#if ANDROID
        private readonly object _lock = new object();
        private const string SHARED_PREFERENCES_FILENAME = "Unify.AndroidCredentials.json";

        private readonly ISharedPreferences _sharedPreferences;
#endif


        public AndroidCredentialManager() {
#if ANDROID
#pragma warning disable CS8604 // Possible null reference argument.
            _sharedPreferences = EncryptedSharedPreferences.Create(
                SHARED_PREFERENCES_FILENAME,
                Runtime.Current.ApplicationId,
                Application.Context,
                EncryptedSharedPreferences.PrefKeyEncryptionScheme.Aes256Siv,
                EncryptedSharedPreferences.PrefValueEncryptionScheme.Aes256Gcm
            );
#pragma warning restore CS8604 // Possible null reference argument.
#else
            SecurityRuntime.Current.Log.Warning($"{GetType().Name}::()", "You CANNOT use this class as this platform is unsupported!");
#endif
        }



        public bool Exists(string credentialName) {
#if ANDROID
            try {
                return !string.IsNullOrEmpty(Get(credentialName));
            } catch {
                return false; // probably
            }
#else
            throw new PlatformNotSupportedException($"{GetType().Name} is only supported on Android.");
#endif
        }

        public string? Get(string credentialName) {
#if ANDROID
            string tag = $"{GetType().Name}::{nameof(Get)}";

            try {
                string? value;

                lock (_lock) {
                    value = _sharedPreferences.GetString(credentialName, null);
                }

                if (value == null)
                    return null;

                return CredentialHelpers.GetAndVerifyCredential(value);
            } catch (Exception ex) {
                SecurityRuntime.Current.Log.Error(tag, $"Failed to remove {credentialName}");
                SecurityRuntime.Current.Log.Error(tag, ex.Message);
                SecurityRuntime.Current.Log.Error(tag, ex.StackTrace ?? "No stack trace available.");

                throw;
            }
#else
            throw new PlatformNotSupportedException($"{GetType().Name} is only supported on Android.");
#endif
        }

        public void Remove(string credentialName) {
#if ANDROID
            string tag = $"{GetType().Name}::{nameof(Remove)}";
            try {
                lock (_lock) {
                    using (ISharedPreferencesEditor? editor = _sharedPreferences.Edit()) {
                        if (editor == null)
                            throw new NullReferenceException("Unable to get SharedPreferences editor!");

                        editor.Remove(credentialName);
                        editor.Apply();
                    }
                }
            } catch (Exception ex) {
                SecurityRuntime.Current.Log.Error(tag, $"Failed to remove {credentialName}");
                SecurityRuntime.Current.Log.Error(tag, ex.Message);
                SecurityRuntime.Current.Log.Error(tag, ex.StackTrace ?? "No stack trace available.");

                throw;
            }
#else
            throw new PlatformNotSupportedException($"{GetType().Name} is only supported on Android.");
#endif
        }

        public void Set(string credentialName, string value) {
#if ANDROID
            string tag = $"{GetType().Name}::{nameof(Set)}";
            try {
                value = CredentialHelpers.GetAndVerifyCredential(value);

                lock (_lock) {
                    using (ISharedPreferencesEditor? editor = _sharedPreferences.Edit()) {
                        if (editor == null)
                            throw new NullReferenceException("Unable to get SharedPreferences editor!");

                        editor.PutString(credentialName, value);
                        editor.Apply();
                    }
                }
            } catch (Exception ex) {
                SecurityRuntime.Current.Log.Error(tag, $"Failed to set {credentialName}");
                SecurityRuntime.Current.Log.Error(tag, ex.Message);
                SecurityRuntime.Current.Log.Error(tag, ex.StackTrace ?? "No stack trace available.");

                throw;
            }
#else
            throw new PlatformNotSupportedException($"{GetType().Name} is only supported on Android.");
#endif
        }
    }
}
