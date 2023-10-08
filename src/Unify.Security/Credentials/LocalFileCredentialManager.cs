using CNCO.Unify.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNCO.Unify.Security.Credentials {
    /// <summary>
    /// <see cref="ICredentialManager"/> using a JSON file.
    /// This is not secure and should not be used unless the file is guaranteed to be protected.
    /// </summary>
    public class FileBasedCredentialManager : ICredentialManager {
        private IFileStorage fileStorage;
        private string fileName = "Unify.Credentials.json";

        public FileBasedCredentialManager() {
            fileStorage = new LocalFileStorage("credentials");
        }

        public bool Exists(string credentialName) => throw new NotImplementedException();
        public string Get(string credentialName) => throw new NotImplementedException();
        public bool Remove(string credentialName) => throw new NotImplementedException();
        public bool Set(string credentialName, string value) => throw new NotImplementedException();
    }
}
