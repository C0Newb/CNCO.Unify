namespace CNCO.Unify.Storage {
    /// <summary>
    /// Used to provide no storage capabilities when a <see cref="IFileStorage"/> is required.
    /// 
    /// Will pretend the file was written but cannot be read and does not exist.
    /// </summary>
    public class NoopFileStorage : IFileStorage {
        public bool Append(string contents, string name) => true;

        public bool AppendBytes(byte[] contents, string name) => true;

        public bool Delete(string name) => true;

        public bool Exists(string name) => false;
        public string GetPath(string filename) => filename;
        public string? Read(string name) => null;

        public byte[]? ReadBytes(string name) => null;

        public bool Write(string contents, string name) => true;

        public bool WriteBytes(byte[] contents, string name) => true;
    }
}