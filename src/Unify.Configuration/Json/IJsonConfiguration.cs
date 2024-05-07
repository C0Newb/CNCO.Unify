using System.Text.Json.Nodes;

namespace CNCO.Unify.Configuration.Json {
    public interface IJsonConfiguration {
        /// <summary>
        /// Deletes the configuration file.
        /// </summary>
        void Delete();

        /// <summary>
        /// Sets the class properties to the values stored in the <paramref name="jsonNode"/>.
        /// </summary>
        void Deserialize(JsonNode jsonNode);

        /// <summary>
        /// Returns the configuration's file path.
        /// </summary>
        /// <returns>Configuration's file path.</returns>
        string GetFilePath();

        /// <summary>
        /// Loads the configuration from disk and calls <see cref="Deserialize(JsonNode)"/> to set this class's properties.
        /// </summary>
        /// <param name="allowEmpty">Allows load to continue regardless if the configuration file exists or not.</param>
        /// <exception cref="ConfigurationNotFoundException"></exception>
        /// <exception cref="ConfigurationReadException"></exception>
        void Load(bool allowEmpty = false);

        /// <summary>
        /// Called right after loading the serialized Json file from the disk and before being deserialize.
        /// </summary>
        /// <param name="jsonString">Serialized Json string.</param>
        /// <returns>Modified serialized Json string.</returns>
        string ModifyAfterRead(string jsonString);

        /// <summary>
        /// Called right before saving the serialized Json file the disk.
        /// </summary>
        /// <param name="jsonString">Serialized Json string.</param>
        /// <returns>Modified serialized Json string.</returns>
        string ModifyBeforeWrite(string jsonString);

        /// <summary>
        /// Saves the contents of a <see cref="JsonNode"/> to disk.
        /// </summary>
        void Save();

        /// <summary>
        /// Serializes this class into a <see cref="JsonNode"/>, such as a <see cref="JsonObject"/> or <see cref="JsonArray"/>.
        /// </summary>
        /// <returns>This class's property values in the Json form.</returns>
        JsonNode Serialize<T>() where T : JsonConfiguration;
    }
}