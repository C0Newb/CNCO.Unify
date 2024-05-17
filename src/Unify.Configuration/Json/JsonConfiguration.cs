using CNCO.Unify.Security;
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
    public class JsonConfiguration : IJsonConfiguration {
        private readonly object _lock = new object();


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
        private readonly IEncryptionProvider _fileEncryption;


        protected JsonSerializerOptions JsonSerializerOptions { get; set; } = JsonHelpers.Options;


        public JsonConfiguration() {
            _fileStorage = new NoopFileStorage();
            _fileEncryption = new NoopFileEncryption();
        }

        /// <summary>
        /// Initializes a new <see cref="JsonConfiguration"/> class given a storage and (optional) encryption strategy.
        /// </summary>
        /// <remarks>
        /// If the encryption strategy <paramref name="fileEncryption"/> is null or empty,
        /// the <see cref="NoopFileEncryption"/> will be used (no encryption).
        /// </remarks>
        /// <param name="name">Name of the configuration file.</param>
        /// <param name="fileStorage">Storage strategy to use when reading/writing the configuration.</param>
        /// <param name="fileEncryption">
        /// File encryption strategy to use when reading/writing the configuration.
        /// If <see langword="null"/>, the <see cref="NoopFileEncryption"/> (no encryption) will be used.
        /// </param>
        public JsonConfiguration(string name, IFileStorage fileStorage, IEncryptionProvider? fileEncryption = null) {
            _filePath = name;
            if (!_filePath.ToLower().EndsWith(".json"))
                _filePath += ".json";

            _fileStorage = fileStorage;
            _fileEncryption = fileEncryption ?? new NoopFileEncryption();

            if (fileStorage.Exists(_filePath)) {
                Load();
            }
        }

        public JsonNode Serialize<T>() where T : JsonConfiguration {
            return JsonSerializer.SerializeToNode(this as T, typeof(T), JsonSerializerOptions) ?? new JsonObject();
        }

        public void Save() {
            if (_filePath == null) return;

            string? directory = Path.GetDirectoryName(_filePath);
            if (string.IsNullOrEmpty(_filePath))
                throw new NullReferenceException("Configuration file path (" + nameof(_filePath) + ") is not set.");
            if (!string.IsNullOrEmpty(directory) && (directory.EndsWith('/') || directory.EndsWith('\\')))
                throw new InvalidOperationException("Invalid path to configuration file, path cannot be a directory.");

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);


            string jsonString = ToString();

            jsonString = ModifyBeforeWrite(jsonString);

            jsonString = _fileEncryption.EncryptString(jsonString);

            lock (_lock) {
                _fileStorage.Write(_filePath, jsonString);
            }
        }

        public void Deserialize(JsonNode jsonNode) {
            var newObj = jsonNode.Deserialize(GetType(), JsonSerializerOptions);

            foreach (var property in GetType().GetProperties()) {
                if (property.GetCustomAttribute<JsonIgnoreAttribute>() != null)
                    continue;

                if (property.CanRead && property.CanWrite)
                    property.SetValue(this, property.GetValue(newObj));
            }
        }

        public void Load(bool allowEmpty = false) {
            if (string.IsNullOrEmpty(_filePath)) return;

            if (!_fileStorage.Exists(_filePath) && !allowEmpty)
                throw new ConfigurationNotFoundException($"Configuration {_filePath} was not found.");

            string? jsonString;
            lock (_lock) {
                jsonString = _fileStorage.Read(_filePath);
            }
            if (jsonString == null) {
                if (allowEmpty)
                    jsonString = "{}";
                else
                    throw new ConfigurationReadException($"Unable to read configuration {_filePath}");
            }

            JsonNode jsonNode;
            JsonNodeOptions nodeOptions = new JsonNodeOptions {
                PropertyNameCaseInsensitive = true,
            };
            JsonDocumentOptions documentOptions = new JsonDocumentOptions { };

            try {
                jsonString = _fileEncryption.DecryptString(jsonString);
                jsonString = ModifyAfterRead(jsonString);
                jsonNode = JsonNode.Parse(jsonString, nodeOptions, documentOptions) ?? new JsonObject();
            } catch (Exception ex) {
                Runtime.ApplicationLog.Error($"Failed to parse {_filePath} failed, renaming file to *.broken and regenerating.");
                Runtime.ApplicationLog.Error(ex.Message);
                Runtime.ApplicationLog.Error(ex.StackTrace ?? "No stack trace.");
                _fileStorage.Rename(_filePath, _filePath + ".broken");
                jsonString = ModifyAfterRead("{}");
                jsonNode = JsonNode.Parse(jsonString, nodeOptions, documentOptions) ?? new JsonObject();
            }

            Deserialize(jsonNode);
        }

        public string GetFilePath() {
            return _filePath ?? string.Empty;
        }

        public void Delete() {
            if (!string.IsNullOrEmpty(_filePath) && _fileStorage.Exists(_filePath)) {
                Runtime.ApplicationLog.Debug($"Deleting {_filePath}");
                _fileStorage.Delete(_filePath);
            }
        }

        public virtual string ModifyBeforeWrite(string jsonString) {
            return jsonString;
        }

        public virtual string ModifyAfterRead(string jsonString) {
            return jsonString;
        }

        public override string ToString() {
            return JsonSerializer.Serialize(this, GetType(), JsonSerializerOptions);
        }
    }
}
