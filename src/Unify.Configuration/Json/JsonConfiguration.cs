using CNCO.Unify.Security.FileEncryption;
using CNCO.Unify.Storage;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace CNCO.Unify.Configuration.Json {
    /// <summary>
    /// Basic JSON configuration file.
    /// 
    /// Please note, your properties must have <code>{ get; set; }</code> otherwise they will not be saved.
    /// Also, make sure you include the base constructor (no parameters)!
    /// </summary>
    public class JsonConfiguration {
        /// <summary>
        /// Full path, including the name, to the configuration file.
        /// </summary>
        private readonly string? _filePath;

        /// <summary>
        /// File storage strategy to use to write/read this configuration.
        /// </summary>
        private readonly IFileStorage _fileStorage;

        /// <summary>
        /// Encryption strategy to use when reading/writing this configuration.
        /// </summary>
        private readonly IFileEncryption _fileEncryption;


        public JsonConfiguration() {
            _fileStorage = new NoopFileStorage();
            _fileEncryption = new NoopEncryption();
        }

        /// <summary>
        /// Initializes a new <see cref="JsonConfiguration"/> class given a storage and (optional) encryption strategy.
        /// </summary>
        /// <remarks>
        /// If the encryption strategy <paramref name="fileEncryption"/> is null or empty,
        /// the <see cref="NoopEncryption"/> will be used (no encryption).
        /// </remarks>
        /// <param name="name">Name of the configuration file.</param>
        /// <param name="fileStorage">Storage strategy to use when reading/writing the configuration.</param>
        /// <param name="fileEncryption">
        /// File encryption strategy to use when reading/writing the configuration.
        /// If <see langword="null"/>, the <see cref="NoopEncryption"/> (no encryption) will be used.
        /// </param>
        public JsonConfiguration(string name, IFileStorage fileStorage, IFileEncryption? fileEncryption = null) {
            _filePath = name;
            if (!_filePath.ToLower().EndsWith(".json"))
                _filePath += ".json";

            _fileStorage = fileStorage;
            _fileEncryption = fileEncryption ?? new NoopEncryption();

            if (fileStorage.Exists(_filePath)) {
                Load();
            }
        }

        /// <summary>
        /// Serializes this class into a <see cref="JsonNode"/>, such as a <see cref="JsonObject"/> or <see cref="JsonArray"/>.
        /// </summary>
        /// <returns>This class's property values in the Json form.</returns>
        public JsonNode Serialize<T>() where T : JsonConfiguration {
            return JsonSerializer.SerializeToNode(this as T) ?? new JsonObject();
        }

        /// <summary>
        /// Saves the contents of a <see cref="JsonNode"/> to disk
        /// </summary>
        public void Save() {
            if (_filePath == null) return;

            string? directory = Path.GetDirectoryName(_filePath);
            if (string.IsNullOrEmpty(_filePath))
                throw new NullReferenceException("Configuration file path (" + nameof(_filePath) + ") is not set.");
            if (!string.IsNullOrEmpty(directory) && (directory.EndsWith('/') || directory.EndsWith('\\')))
                throw new InvalidOperationException("Invalid path to configuration file, path cannot be a directory.");

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);


            JsonSerializerOptions options = JsonHelpers.Options;
            string jsonString = JsonSerializer.Serialize(this, GetType(), options);

            jsonString = ModifyBeforeWrite(jsonString);

            jsonString = _fileEncryption.EncryptString(jsonString);
            _fileStorage.Write(jsonString, _filePath);
        }


        /// <summary>
        /// Sets the class properties to the values stored in the <paramref name="jsonNode"/>.
        /// </summary>
        public void Deserialize(JsonNode jsonNode) {
            var newObj = jsonNode.Deserialize(GetType(), JsonHelpers.Options);

            foreach (var property in GetType().GetProperties()) {
                if (property.GetCustomAttribute<JsonIgnoreAttribute>() != null)
                    continue;

                if (property.CanRead && property.CanWrite)
                    property.SetValue(this, property.GetValue(newObj));
            }
        }

        /// <summary>
        /// Loads the configuration from disk and calls <see cref="Deserialize(JsonNode)"/> to set this class's properties.
        /// </summary>
        /// <param name="allowEmpty">Allows load to continue regardless if the configuration file exists or not.</param>
        /// <exception cref="ConfigurationNotFoundException"></exception>
        public void Load(bool allowEmpty = false) {
            if (string.IsNullOrEmpty(_filePath)) return;

            if (!_fileStorage.Exists(_filePath) && !allowEmpty)
                throw new ConfigurationNotFoundException($"Configuration {_filePath} was not found.");


            string? jsonString = _fileStorage.Read(_filePath);
            if (jsonString == null) {
                if (allowEmpty)
                    jsonString = "{}";
                else
                    throw new ConfigurationReadException($"Unable to read configuration {_filePath}");
            }
            jsonString = _fileEncryption.DecryptString(jsonString);

            jsonString = ModifyAfterRead(jsonString);

            JsonNodeOptions nodeOptions = new JsonNodeOptions {
                PropertyNameCaseInsensitive = true,
            };
            JsonDocumentOptions documentOptions = new JsonDocumentOptions { };
            JsonNode jsonNode = JsonNode.Parse(jsonString, nodeOptions, documentOptions) ?? new JsonObject();
            Deserialize(jsonNode);
        }

        public string GetFilePath() {
            return _filePath ?? string.Empty;
        }


        /// <summary>
        /// Called right before saving the serialized Json file the disk.
        /// </summary>
        /// <param name="jsonString">Serialized Json string.</param>
        /// <returns>Modified serialized Json string.</returns>
        public virtual string ModifyBeforeWrite(string jsonString) {
            return jsonString;
        }


        /// <summary>
        /// Called right after loading the serialized Json file from the disk and before being deserialize.
        /// </summary>
        /// <param name="jsonString">Serialized Json string.</param>
        /// <returns>Modified serialized Json string.</returns>
        public virtual string ModifyAfterRead(string jsonString) {
            return jsonString;
        }
    }
}
