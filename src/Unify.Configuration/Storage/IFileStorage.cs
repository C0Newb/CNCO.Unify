﻿namespace CNCO.Unify.Configuration.Storage {
    /// <summary>
    /// Used to write a configuration to a storage medium, such as the filesystem, AWS, SQL, etc.
    /// </summary>
    public interface IFileStorage {
        /// <summary>
        /// Writes the contents of a configuration file to storage.
        /// </summary>
        /// <param name="contents">Contents to be written.</param>
        /// <param name="name">Name of the file to write to.</param>
        /// <returns>Whether the file was saved or not.</returns>
        public bool Write(string contents, string name);
        /// <inheritdoc cref="Write"/>
        public bool WriteBytes(byte[] contents, string name);


        /// <summary>
        /// Reads the contents of a configuration file from storage.
        /// </summary>
        /// <param name="name">Name of the file to read from.</param>
        /// <returns>Contents of the file. Null represents a read error occurred.</returns>
        public string? Read(string name);

        /// <inheritdoc cref="Read(string)"/>
        /// <returns>Contents of the file in bytes. Null represents a read error occurred.</returns>
        public byte[]? ReadBytes(string name);


        /// <summary>
        /// Checks whether a configuration with <paramref name="name"/> exists on the storage.
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
    }
}
