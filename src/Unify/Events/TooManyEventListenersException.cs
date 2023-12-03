namespace CNCO.Unify.Events {
    // Too many event listeners (callbacks) subscribed to an event
    public class TooManyEventListenersException : Exception {
        public string HelpMessage = "Try removing excess listeners with .RemoveListener(Event, Callback) or .RemoveAllListeners(Event).";

        public TooManyEventListenersException() : base($"Number of event listeners for this event exceeds the maximum of {EventEmitter.MAX_EVENT_LISTENERS}.") {
        }

        public TooManyEventListenersException(string eventName) : base($"Number of event listeners for {eventName} exceeds the maximum of {EventEmitter.MAX_EVENT_LISTENERS}.") {
        }
    }
}