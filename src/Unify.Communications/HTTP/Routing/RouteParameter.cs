namespace CNCO.Unify.Communications.Http.Routing {
    /// <summary>
    /// A parameter in a <see cref="RouteTemplate"/>.
    /// </summary>
    public sealed class RouteParameter {
        /// <summary>
        /// Original value (as a string).
        /// </summary>
        private readonly string _originalStringValue;


        /// <summary>
        /// Type of <see cref="Value"/>.
        /// </summary>
        public Type Type { get; private set; }

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

        /// <summary>
        /// Attempts to guess and parse <see cref="Value"/> to the correct type.
        /// </summary>
        private void GuessType() {
            if (int.TryParse((string)Value, out int @int)) {
                Type = typeof(int);
                Value = @int;
                return;
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
    }
}
