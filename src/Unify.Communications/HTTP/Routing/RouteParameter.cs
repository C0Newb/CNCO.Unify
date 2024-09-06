using System.Numerics;
using System.Text.RegularExpressions;

namespace CNCO.Unify.Communications.Http.Routing {
    /// <summary>
    /// A parameter in a <see cref="RouteTemplate"/>.
    /// </summary>
    public sealed partial class RouteParameter {
        /// <summary>
        /// Original value (as a string).
        /// </summary>
        private readonly string _originalStringValue;


        /// <summary>
        /// Type of <see cref="Value"/>.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Whether the type is a number and can be molded to a different type which is also numeric.
        /// </summary>
        public bool IsNumeric { get; private set; }

        /// <summary>
        /// Name of the parameter in the request path.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Value of the parameter.
        /// </summary>
        public object Value { get; private set; }


        /// <summary>
        /// Initializes an instance of the <see cref="RouteParameter"/> class.
        /// </summary>
        /// <param name="type">Type of <paramref name="value"/>.</param>
        /// <param name="name">Name of the parameter in the request path.</param>
        /// <param name="value">Value of the parameter.</param>
        public RouteParameter(Type type, string name, object value) {
            Type = type;
            Name = name;
            Value = value;
            _originalStringValue = value.ToString() ?? string.Empty;
        }

        /// <inheritdoc cref="RouteParameter(Type, string, object)"/>
        public RouteParameter(string name, object value) {
            Type = typeof(string);
            Name = name;
            Value = value;
            _originalStringValue = value.ToString() ?? string.Empty;

            GuessType();
        }

        /// <summary>
        /// Gets the original string value.
        /// </summary>
        /// <returns>Original string representation of the value.</returns>
        internal object? ToStringValue() => _originalStringValue;


        private bool TryParsingNumeric() {
            if (!IsNumericRegex().IsMatch((string)Value)) {
                return false;
            }

            // Remove the trailing '.'
            string value = ((string)Value).TrimEnd('.');

            if (IsDecimalRegex().IsMatch(value)) {
                // Try parsing as a decimal
                // (has higher precision than double or float, we can always cast to those two later)
                if (decimal.TryParse(value, out decimal @decimal)) {
                    Type = typeof(decimal);
                    Value = @decimal;
                    return true;
                } else if (double.TryParse(value, out double @double)) {
                    // .. or double ?
                    Type = Type = typeof(double);
                    Value = @double;
                    return true;
                } else if (float.TryParse(value, out float @float)) {
                    Type = typeof(float);
                    Value = @float;
                    return true;
                } else if (value.Length > decimal.MaxValue.ToString().Length) {
                    /*CommunicationsRuntime.Current.RuntimeLog.Verbose(
                        $"{nameof(RouteParameter)}::{nameof(TryParsingNumeric)}()",
                        $"Attempt to parse a VERY large number to a decimal/double/float. Value is {value.Length} characters long."
                    );*/
                    // it'll be treated as a big int :)
                } else {
                    // if it's none of those, treat the thing as a string. Something is off...
                    // Go ahead and log it so someone can fix it later :)
                    CommunicationsRuntime.Current.RuntimeLog.Verbose(
                        $"{nameof(RouteParameter)}::{nameof(TryParsingNumeric)}()",
                        $"Failed to parse decimal/double/float value: {value}"
                    );
                }
            } else {
                // Try int then long (int is more common)
                if (int.TryParse((string)Value, out int @int)) {
                    Type = typeof(int);
                    Value = @int;
                    return true;
                } else if (long.TryParse((string)Value, out long @long)) {
                    Type = typeof(long);
                    Value = @long;
                    return true;
                }
                // otherwise, treat it as a really big number
            }

            if (BigInteger.TryParse((string)Value, out BigInteger bigInteger)) {
                Type = typeof(BigInteger);
                Value = bigInteger;
                return true;
            }

            // Nope, treat it as a string
            return false;
        }

        /// <summary>
        /// Attempts to guess and parse <see cref="Value"/> to the correct type.
        /// </summary>
        private void GuessType() {
            if (TryParsingNumeric()) {
                // You may think to yourself: "what, this is clearly a number?"
                // We set it to not numeric since we DO NOT want to try casting BigInteger to int.
                if (Type != typeof(BigInteger)) {
                    IsNumeric = true;
                }
            } else if (Guid.TryParse((string)Value, out Guid guid)) {
                Type = typeof(Guid);
                Value = guid;
                return;
            } else if (DateTime.TryParse((string)Value, out DateTime dateTime)) {
                Type = typeof(DateTime);
                Value = dateTime;
                return;
            } else {
                Type = typeof(string);
                return;
            }
        }

        [GeneratedRegex(@"^\d+(\.)\d+$")]
        private static partial Regex IsDecimalRegex();

        [GeneratedRegex(@"^\d+(|\.)\d+$")]
        private static partial Regex IsNumericRegex();
    }
}
