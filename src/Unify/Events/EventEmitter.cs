using System.Text.RegularExpressions;

namespace CNCO.Unify.Events {
    /// <summary>
    /// Event emitter class, similar to the event emitter in JS.
    /// </summary>
    public partial class EventEmitter : IEventEmitter {
        /// <summary>
        /// Number of maximum events for this event emitter. This is to try and manage the max possible event listeners (MAX_EVENTS * MAX_EVENT_LISTENERS).
        /// </summary>
        public static readonly int MAX_EVENTS = 30;
        /// <summary>
        /// Number of event listeners for an event
        /// </summary>
        public static readonly int MAX_EVENT_LISTENERS = 5;

        /// <summary>
        /// Regex representing a valid event name.
        /// </summary>
        /// <returns>Regex for valid event names.</returns>
        [GeneratedRegex("^.{1,128}$")]
        private static partial Regex ValidEventNameRegex();

        /// <summary>
        /// Map of events -> list of callbacks.
        /// </summary>
        private readonly Dictionary<string, List<IEventEmitterListener>> _listeners = new Dictionary<string, List<IEventEmitterListener>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="EventEmitter"/> class.
        /// </summary>
        public EventEmitter() { }

        // Private stuff
        #region Private stuff
        private void AddEventListener(string eventName, ICallback callback, bool oneTimeListener = false, bool prependListener = false) {
            eventName = NormalizeEventName(eventName);
            if (!ValidEventNameRegex().IsMatch(eventName))
                throw new InvalidEventNameException(eventName);

            var eventEmitterListener = new EventEmitterListener(this, eventName, callback, oneTimeListener);
            if (!_listeners.TryGetValue(eventName, out List<IEventEmitterListener>? value)) {
                if (_listeners.Count >= MAX_EVENTS)
                    throw new TooManyEventsException();
                value = new List<IEventEmitterListener>();
                _listeners[eventName] = value;
            }

            var eventEmitterListeners = value;
            eventEmitterListeners ??= new List<IEventEmitterListener>();

            if (eventEmitterListeners.Count >= MAX_EVENT_LISTENERS)
                throw new TooManyEventListenersException(eventName);

            if (prependListener)
                eventEmitterListeners.Insert(0, eventEmitterListener);
            else
                eventEmitterListeners.Add(eventEmitterListener);

            _listeners[eventName] = eventEmitterListeners;
        }

        private static string NormalizeEventName(string eventName) => eventName.ToLower();
        #endregion


        // Interface

        public void Emit(string eventName, params object?[]? parameters) {
            eventName = NormalizeEventName(eventName);
            if (_listeners.TryGetValue(eventName, out List<IEventEmitterListener>? value)) {
                var eventEmitterList = value;
                if (eventEmitterList == null || eventEmitterList.Count == 0) {
                    _listeners.Remove(eventName);
                    return;
                }

                Runtime.Current.UnifyLog.Debug("EventEmitter", $"Event \"{eventName}\" has been fired.");

                foreach (var eventEmitterListener in eventEmitterList) {
                    eventEmitterListener.Activate(parameters);
                }
            }
        }

        // Numbers

        public int GetMaxListeners() => MAX_EVENT_LISTENERS;

        public int GetMaxEvents() => MAX_EVENTS;

        public string[] Events() => _listeners.Keys.ToArray();
        public int EventsCount() => _listeners.Count;

        public IEventEmitterListener[] Listeners(string eventName) {
            eventName = NormalizeEventName(eventName);
            if (_listeners.TryGetValue(eventName, out List<IEventEmitterListener>? value)) {
                var listeners = value;
                if (listeners == null || listeners.Count == 0) {
                    _listeners.Remove(eventName);
                    return Array.Empty<IEventEmitterListener>();
                }

                return listeners.ToArray();
            } else {
                return Array.Empty<IEventEmitterListener>();
            }
        }

        public int ListenersCount(string eventName) {
            eventName = NormalizeEventName(eventName);
            if (_listeners.TryGetValue(eventName, out List<IEventEmitterListener>? value)) {
                var listeners = value;
                if (listeners == null || listeners.Count == 0) {
                    _listeners.Remove(eventName);
                    return 0;
                }

                return listeners.Count;
            } else {
                return 0;
            }
        }

        public int ListenersCount(string eventName, ICallback callback) {
            eventName = NormalizeEventName(eventName);
            if (_listeners.TryGetValue(eventName, out List<IEventEmitterListener>? value)) {
                var listeners = value;
                if (listeners == null || listeners.Count == 0) {
                    _listeners.Remove(eventName);
                    return 0;
                }

                var count = 0;
                foreach (var eventEmitterListener in listeners) {
                    if (eventEmitterListener.GetCallback() == callback)
                        count++;
                }
                return count;
            } else {
                return 0;
            }
        }

        // Adding
        public void AddListener(string eventName, ICallback callback) => AddEventListener(eventName, callback);

        public void On(string eventName, ICallback callback) => AddEventListener(eventName, callback);

        public void Once(string eventName, ICallback callback) => AddEventListener(eventName, callback, true);

        public void PrependListener(string eventName, ICallback callback) => AddEventListener(eventName, callback, false, true);

        public void PrependOnceListener(string eventName, ICallback callback) => AddEventListener(eventName, callback, true, true);

        // Remove

        public void Off(string eventName, ICallback callback) => RemoveListener(eventName, callback);

        public void RemoveListener(string eventName, ICallback callback) {
            eventName = NormalizeEventName(eventName);
            if (_listeners.TryGetValue(eventName, out List<IEventEmitterListener>? value)) {
                var eventEmitterListeners = value;
                if (eventEmitterListeners == null || eventEmitterListeners.Count == 0) {
                    _listeners.Remove(eventName);
                    return;
                }

                var newEventEmitterListeners = new List<IEventEmitterListener>(eventEmitterListeners.Count - 1);

                foreach (var eventEmitterListener in eventEmitterListeners) {
                    if (eventEmitterListener.GetCallback() != callback) newEventEmitterListeners.Add(eventEmitterListener);
                }

                _listeners.Remove(eventName);
                if (eventEmitterListeners.Count >= 1)
                    _listeners[eventName] = newEventEmitterListeners;
            }
        }

        public void RemoveAllListeners(string eventName) => _listeners.Remove(NormalizeEventName(eventName));
        public void RemoveAllEvents() => _listeners.Clear();

        public void Dispose() {
            RemoveAllEvents();
            GC.SuppressFinalize(this);
        }
    }
}
