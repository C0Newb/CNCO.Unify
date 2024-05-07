namespace CNCO.Unify.Security.Credentials {

    [Serializable]
    public class CredentialTamperException : Exception {
        public CredentialTamperException() { }
        public CredentialTamperException(string message) : base(message) { }
        public CredentialTamperException(string message, Exception inner) : base(message, inner) { }
        protected CredentialTamperException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
