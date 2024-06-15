namespace CNCO.Unify.Storage {
    /// <summary>
    /// Used to write a configuration to a storage medium, such as the filesystem, AWS, SQL, etc.
    /// </summary>
    public interface IFileStorage {
        /// <summary>
        /// Returns the path to a particular file.
        /// </summary>
        /// <param name="name">Filename or relative path to file.</param>
        /// <returns>Full path to the given filename.</returns>
        public string GetPath(string name);


        /// <summary>
        /// Writes <paramref name="contents"/> to a file in storage.
        /// </summary>
        /// <param name="contents">Contents to be written.</param>
        /// <param name="name">Name of the file to write to.</param>
        /// <returns>Whether the file was saved or not.</returns>
        public bool Write(string name, string contents);
        /// <inheritdoc cref="Write"/>
        public bool WriteBytes(string name, byte[] contents);


        /// <summary>
        /// Appends <paramref name="contents"/> to a file in storage.
        /// </summary>
        /// <param name="contents">Contents to be written (appended).</param>
        /// <param name="name">Name of the file to append to.</param>
        /// <returns>Whether the file was appended to or not.</returns>
        public bool Append(string name, string contents);

        /// <inheritdoc cref="Append"/>
        public bool AppendBytes(string name, byte[] contents);


        /// <summary>
        /// Reads the contents of a file from storage.
        /// </summary>
        /// <param name="name">Name of the file to read from.</param>
        /// <returns>Contents of the file. Null represents a read error occurred.</returns>
        public string? Read(string name);

        /// <inheritdoc cref="Read(string)"/>
        /// <returns>Contents of the file in bytes. Null represents a read error occurred.</returns>
        public byte[]? ReadBytes(string name);


        /// <summary>
        /// Opens the file and returns the <see cref="FileStream"/>.
        /// </summary>
        /// <param name="name">Name of the file to open.</param>
        /// <returns>Stream of the file.</returns>
        public Stream? Open(string name, FileStreamOptions? streamOptions);


        /// <summary>
        /// Checks whether a file with <paramref name="name"/> exists on the storage.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Whether the file exists on the storage.</returns>
        public bool Exists(string name);


        /// <summary>
        /// Removes a file from storage.
        /// </summary>
        /// <param name="name">Name of the file to delete.</param>
        /// <returns>Successfully deleted.</returns>
        public bool Delete(string name);


        /// <summary>
        /// Renames (moves) a file.
        /// </summary>
        /// <param name="name">Current file name.</param>
        /// <param name="newName">New file name.</param>
        /// <returns>Successfully renamed.</returns>
        public bool Rename(string name, string newName);
    }
}
