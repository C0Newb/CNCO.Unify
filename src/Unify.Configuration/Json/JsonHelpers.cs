using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace CNCO.Unify.Configuration.Json {
    /// <summary>
    /// Helper classes to cover some short-fallings of <see cref="System.Text.Json"/>. (Provides a "better" (doesn't explode) number converter)
    /// </summary>
    public class JsonHelpers {
        public static JsonSerializerOptions Options = new JsonSerializerOptions {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = {
                new NumericConverterFactory(),
            }
        };

        #region SecureAttributeConverter
        public class SecureJsonTypeInfoModifier {
            private readonly SecureAttributeConverterFactory? _attributeConverterFactory;

            /// <summary>
            /// Creates a new instance of the <see cref="SecureJsonTypeInfoModifier"/> class.
            /// This is used to update the getter/setter for properties with the <see cref="SecureAttribute"/> in order to encrypt/decrypt their values.
            /// </summary>
            /// <param name="encryptionFunction">Function that takes in the plain text and encrypts the value.</param>
            /// <param name="decryptionFunction">Function that decrypts the JSON value.</param>
            public SecureJsonTypeInfoModifier(Func<string, string?> encryptionFunction, Func<string, string?> decryptionFunction) {
                _attributeConverterFactory = new SecureAttributeConverterFactory(encryptionFunction, decryptionFunction);
            }

            /// <summary>
            /// Creates a new instance of the <see cref="SecureJsonTypeInfoModifier"/> class.
            /// This is used to update the getter/setter for properties with the <see cref="SecureAttribute"/> in order to encrypt/decrypt their values.
            /// </summary>
            /// <param name="secureAttributeConverterFactory">
            /// A <see cref="SecureAttributeConverterFactory"/> instance with the appropriate encryption and decryption functions.
            /// </param>
            public SecureJsonTypeInfoModifier(SecureAttributeConverterFactory secureAttributeConverterFactory) {
                _attributeConverterFactory = secureAttributeConverterFactory;
            }

            private static bool IsTypeSupported(Type type) => type == typeof(string);

            public void Modify(JsonTypeInfo typeInfo) {
                if (_attributeConverterFactory == null)
                    return; // nothing to do...

                foreach (JsonPropertyInfo propertyInfo in typeInfo.Properties) {
                    if (!IsTypeSupported(propertyInfo.PropertyType)) // Not interested
                        continue;

                    var secureAttributes = propertyInfo.AttributeProvider?.GetCustomAttributes(typeof(SecureAttribute), true) ?? Array.Empty<object>();
                    var attribute = secureAttributes.Length >= 1 ? (SecureAttribute)secureAttributes[0] : null;

                    if (attribute == null) // We don't care
                        continue;

                    // I'm interested... add the required converter!
                    propertyInfo.CustomConverter = _attributeConverterFactory;
                }
            }
        }


        /// <summary>
        /// Used to convert JSON properties with the <see cref="SecureAttribute"/>.
        /// </summary>
        public class SecureAttributeConverterFactory : JsonConverterFactory {
            private readonly Func<string, string?> _encryptionFunction;
            private readonly Func<string, string?> _decryptionFunction;

            /// <summary>
            /// Creates a new instance of the <see cref="SecureAttributeConverterFactory"/> class.
            /// This is used to convert JSON values secured with the <see cref="SecureAttribute"/>.
            /// </summary>
            /// <param name="encryptionFunction">Function that takes in the plain text and encrypts the value.</param>
            /// <param name="decryptionFunction">Function that decrypts the JSON value.</param>
            public SecureAttributeConverterFactory(Func<string, string?> encryptionFunction, Func<string, string?> decryptionFunction) {
                _encryptionFunction = encryptionFunction;
                _decryptionFunction = decryptionFunction;
            }

            public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(string);

            public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) {
                var converterType = typeof(SecureAttributeConverter<>).MakeGenericType(typeToConvert);
                var converterInstance = Activator.CreateInstance(converterType, _encryptionFunction, _decryptionFunction);
                return converterInstance as JsonConverter ?? throw new JsonException($"Converter creation failed for type '{typeToConvert.FullName}'.");
            }
        }

        private class SecureAttributeConverter<T> : JsonConverter<T> {
            private readonly Func<string, string?> _encryptionFunction;
            private readonly Func<string, string?> _decryptionFunction;

            public SecureAttributeConverter(Func<string, string?> encryptionFunction, Func<string, string?> decryptionFunction) {
                _encryptionFunction = encryptionFunction;
                _decryptionFunction = decryptionFunction;
            }

            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
                string decryptedValue = _decryptionFunction(reader.GetString() ?? throw new NullReferenceException()) ?? string.Empty;

                if (reader.TokenType == JsonTokenType.String) {
                    // number
                    return (T)Convert.ChangeType(decryptedValue, typeof(T));
                }

                throw new JsonException($"Unable to convert value to {typeof(T).Name}.");
            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) {
                string encryptedValue = _encryptionFunction(value?.ToString() ?? string.Empty) ?? string.Empty;
                writer.WriteStringValue(encryptedValue);
            }
        }
        #endregion

        #region NumericConverter
        /// <summary>
        /// Used to better convert numeric JSON values.
        /// </summary>
        public class NumericConverterFactory : JsonConverterFactory {
            public override bool CanConvert(Type type) => type == typeof(int)
                || type == typeof(long)
                || type == typeof(short)
                || type == typeof(byte)
                || type == typeof(uint)
                || type == typeof(ulong)
                || type == typeof(ushort)
                || type == typeof(float)
                || type == typeof(double)
                || type == typeof(decimal);

            public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) {
                var converterType = typeof(NumericConverter<>).MakeGenericType(typeToConvert);
                var converterInstance = Activator.CreateInstance(converterType);
                return converterInstance as JsonConverter ?? throw new JsonException($"Converter creation failed for type '{typeToConvert.FullName}'.");
            }
        }

        private class NumericConverter<T> : JsonConverter<T> {
            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
                if (reader.TokenType == JsonTokenType.Number) {
                    return (T)Convert.ChangeType(reader.GetDouble(), typeof(T));
                }

                if (reader.TokenType == JsonTokenType.String && double.TryParse(reader.GetString(), out var doubleValue)) {
                    return (T)Convert.ChangeType(doubleValue, typeof(T));
                }

                return (T)Convert.ChangeType(0, typeof(T));
                //throw new JsonException($"Unable to convert value to {typeof(T).Name}.");
            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) {
                writer.WriteNumberValue(Convert.ToDouble(value));
            }
        }
        #endregion
    }
}
