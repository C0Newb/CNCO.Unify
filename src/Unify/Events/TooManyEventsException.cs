namespace CNCO.Unify.Events {
    /// <summary>
    /// Too many events to listen for!
    /// </summary>
    public class TooManyEventsException : Exception {
        public string HelpMessage = "Try removing excess events with '.RemoveAllEvents()'.";

        public TooManyEventsException() : base($"Number of events exceeds the maximum of {EventEmitter.MAX_EVENTS}.") {
        }
    }
}
