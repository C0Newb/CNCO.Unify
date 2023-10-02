namespace CNCO.Unify.Configuration.Storage {
    /// <summary>
    /// Saves/loads file from the local filesystem.
    /// </summary>
    public class LocalFileStorage : IFileStorage {
        private readonly string _directory = string.Empty;

        /// <summary>
        /// Directory all files are stored in.
        /// </summary>
        public string Directory {
            get => _directory;
        }

        /// <summary>
        /// Initializes an instance of <see cref="LocalFileStorage"/>.
        /// </summary>
        /// <param name="directory">
        /// Parent directory files are written to.
        /// Defaults to current working directory.
        /// </param>
        public LocalFileStorage(string? directory = null) {
            if (string.IsNullOrEmpty(directory))
                _directory = System.IO.Directory.GetCurrentDirectory();
            else
                _directory = directory;
        }

        private string GetPath(string name) => Path.Combine(_directory, name);


        public bool Delete(string name) {
            try {
                File.Delete(GetPath(name));
                return false;
            } catch {
                return false;
            }
        }

        public bool Exists(string name) => File.Exists(GetPath(name));

        public string? Read(string name) {
            try {
                return File.ReadAllText(GetPath(name));
            } catch {
                return null;
            }

        }

        public byte[]? ReadBytes(string name) {
            try {
                return File.ReadAllBytes(GetPath(name));
            } catch {
                return null;
            }
        }

        public bool Write(string contents, string name) {
            try {
                File.WriteAllText(GetPath(name), contents);
                return false;
            } catch {
                return false;
            }
        }

        public bool WriteBytes(byte[] contents, string name) {
            try {
                File.WriteAllBytes(GetPath(name), contents);
                return true;
            } catch {
                return false;
            }
        }
    }
}
