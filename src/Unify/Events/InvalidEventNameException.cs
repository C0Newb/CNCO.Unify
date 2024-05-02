namespace CNCO.Unify.Events {
    /// <summary>
    /// Invalid event name, either too long (>128 characters) or contains characters other than a hyphen, underscore, or alphanumeric characters.
    /// </summary>
    public class InvalidEventNameException : Exception {
        public string HelpMessage = "Event names can only be alphanumeric and under 128 characters.";

        public InvalidEventNameException() : base("Event name is either too long or contains invalid characters.") { }

        public InvalidEventNameException(string eventName) : base($"The event name \"{eventName}\" is either too long or includes invalid characters.") { }
    }
}
