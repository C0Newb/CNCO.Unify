namespace CNCO.Unify.Configuration.Storage {
    /// <summary>
    /// Used to provide no storage capabilities when a <see cref="IFileStorage"/> is required.
    /// 
    /// Will pretend the file was written but cannot be read and does not exist.
    /// </summary>
    internal class NoopFileStorage : IFileStorage {
        public bool Delete(string name) => true;

        public bool Exists(string name) => false;

        public string? Read(string name) => null;

        public byte[]? ReadBytes(string name) => null;

        public bool Write(string contents, string name) => true;

        public bool WriteBytes(byte[] contents, string name) => true;
    }
}