using System.Text;

namespace CNCO.Unify.Storage {
    /// <summary>
    /// Temporary, volatile file storage saved to the application's memory.
    /// This should be used as something between the <see cref="NoopFileStorage"/> and <see cref="LocalFileStorage"/>.
    /// </summary>
    /// <remarks>
    /// The 
    /// </remarks>
    public class InMemoryFileStorage : IFileStorage, IDisposable {
        private readonly bool _throwErrors = !UnifyRuntime.Current.Configuration.SuppressFileStorageExceptions;
        private readonly string _directory = string.Empty;


        private readonly Dictionary<string, byte[]> Files = new Dictionary<string, byte[]>();


        /// <summary>
        /// Whether file names are case sensitive. Defaults to <see langword="true"/> to force compatibility with POSIX.
        /// </summary>
        public bool CaseSensitiveFileNames = true;

        /// <summary>
        /// Whether files names can include <code><![CDATA[< > : " / \ | ? *]]></code>.
        /// Defaults to <see langword="false"/> to force compatibility with Windows.
        /// </summary>
        public bool AllowInvalidNtfsFileNames = false;
        private readonly char[] InvalidNtfsFileNames = new char[8] { '<', '>', ':', '"', '/', '|', '?', '*' };


        public InMemoryFileStorage(string? directory = null) {
            if (string.IsNullOrEmpty(directory) || !Path.IsPathRooted(directory)) {
                // make it rooted
                string root;
                if (Platform.IsApple())
                    root = "./Library/";
                else if (Platform.IsAndroid())
                    root = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); // -> /data/users/*/<package>/files/
                else
                    root = "/";

                directory = Path.Combine(root, directory ?? string.Empty);
            }
            _directory = directory;
        }

        // Enforce case insensitivity or check for invalid file name characters.
        private string NormalizeName(string name) {
            // Use \
            name = name.Replace('/', '\\');

            char[] invalidFileNameCharacters;
            if (AllowInvalidNtfsFileNames) {
                invalidFileNameCharacters = new char[2] { '\0', '/' };
            } else {
                char[] specialCharacters = Enumerable.Range(0, 32).Select(i => (char)i).ToArray();
                invalidFileNameCharacters = InvalidNtfsFileNames.Concat(specialCharacters).ToArray();
            }

            foreach (char character in invalidFileNameCharacters) {
                if (name.Contains(character))
                    throw new IOException("Invalid file name.");
            }

            return CaseSensitiveFileNames ? name : name.ToLower();
        }

        public bool Append(string name, string contents) {
            try {
                return AppendBytes(name, Encoding.UTF8.GetBytes(contents));
            } catch {
                if (_throwErrors)
                    throw;
                return false;
            }
        }

        public bool AppendBytes(string name, byte[] contents) {
            try {
                name = NormalizeName(name);
                byte[] currentBytes = Files.GetValueOrDefault(name, Array.Empty<byte>());
                Files[name] = currentBytes.Concat(contents).ToArray();
                return true;
            } catch {
                if (_throwErrors)
                    throw;
                return false;
            }
        }

        public bool Delete(string name) {
            try {
                name = NormalizeName(name);
                Files.Remove(name);
                return true;
            } catch {
                if (_throwErrors)
                    throw;
                return false;
            }
        }

        public void Dispose() {
            Files.Clear();
            GC.SuppressFinalize(this);
        }

        public bool Exists(string name) {
            try {
                name = NormalizeName(name);
                return Files.ContainsKey(name);
            } catch {
                if (_throwErrors)
                    throw;
                return false;
            }
        }

        public string GetPath(string name) => Path.Combine(_directory, name);

        public string? Read(string name) {
            try {
                name = NormalizeName(name);
                if (!Files.TryGetValue(name, out byte[]? value))
                    throw new FileNotFoundException();
                return Encoding.UTF8.GetString(value);
            } catch {
                if (_throwErrors)
                    throw;
                return null;
            }
        }

        public byte[]? ReadBytes(string name) {
            try {
                name = NormalizeName(name);
                if (!Files.TryGetValue(name, out byte[]? value))
                    throw new FileNotFoundException();
                return value;
            } catch {
                if (_throwErrors)
                    throw;
                return null;
            }
        }

        public bool Rename(string name, string newName) {
            try {
                name = NormalizeName(name);
                newName = NormalizeName(newName);

                if (Files.ContainsKey(newName))
                    throw new IOException("File already exists.");

                if (!Files.TryGetValue(name, out byte[]? value))
                    throw new FileNotFoundException();

                Files.Add(newName, value);
                Files.Remove(name);

                return true;
            } catch {
                if (_throwErrors)
                    throw;
                return false;
            }
        }

        public bool Write(string name, string contents) {
            try {
                return WriteBytes(name, Encoding.UTF8.GetBytes(contents));
            } catch {
                if (_throwErrors)
                    throw;
                return false;
            }
        }

        public bool WriteBytes(string name, byte[] contents) {
            try {
                name = NormalizeName(name);
                Files[name] = contents;
                return true;
            } catch {
                if (_throwErrors)
                    throw;
                return false;
            }
        }
    }
}
