namespace CNCO.Unify.Storage {
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
            if (string.IsNullOrEmpty(directory) || !Path.IsPathRooted(directory)) {
                // make it rooted
                string root;
                if (Platform.IsApple())
                    root = "./Library/";
                else if (Platform.IsAndroid())
                    root = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); // -> /data/users/*/<package>/files/
                else
                    root = System.IO.Directory.GetCurrentDirectory();

                directory = Path.Combine(root, directory ?? string.Empty);
            }
            _directory = directory;
        }

        public string GetPath(string name) {
            try {
                if (!System.IO.Directory.Exists(Directory))
                    System.IO.Directory.CreateDirectory(Directory);
            } catch { }
            return Path.Combine(_directory, name);
        }


        public bool Delete(string name) {
            try {
                File.Delete(GetPath(name));
                return true;
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

        public bool Write(string name, string contents) {
            try {
                File.WriteAllText(GetPath(name), contents);
                return true;
            } catch {
                return false;
            }
        }

        public bool WriteBytes(string name, byte[] contents) {
            try {
                File.WriteAllBytes(GetPath(name), contents);
                return true;
            } catch {
                return false;
            }
        }

        public bool Append(string name, string contents) {
            try {
                File.AppendAllText(GetPath(name), contents);
                return true;
            } catch {
                return false;
            }
        }

        public bool AppendBytes(string name, byte[] contents) {
            try {
                byte[] current = ReadBytes(name) ?? Array.Empty<byte>();
                byte[] newBytes = new byte[current.Length + contents.Length];

                Buffer.BlockCopy(current, 0, newBytes, 0, current.Length);
                Buffer.BlockCopy(contents, 0, newBytes, current.Length, contents.Length);

                WriteBytes(name, newBytes);
                return true;
            } catch {
                return false;
            }
        }

        public bool Rename(string name, string newName) {
            try {
                File.Move(GetPath(name), GetPath(newName));
                return true;
            } catch {
                return false;
            }
        }
    }
}
