namespace CNCO.Unify.Events {
    /// <summary>
    /// Event emitter class, similar to the event emitter in JS.
    /// </summary>
    public class EventEmitter : IEventEmitter {
        /// <summary>
        /// Number of maximum events for this event emitter. This is to try and manage the max possible event listeners (MAX_EVENTS * MAX_EVENT_LISTENERS).
        /// </summary>
        public static readonly int MAX_EVENTS = 30;
        /// <summary>
        /// Number of event listeners for an event
        /// </summary>
        public static readonly int MAX_EVENT_LISTENERS = 5;


        /// <summary>
        /// Event emitter listener, this is the object that holds the callback class and allows for one-time listening.
        /// (This is what listens for an event).
        /// </summary>
        public class EventEmitterListener : IEventEmitterListener {
            private readonly EventEmitter _eventEmitter;
            private readonly string _event;
            private readonly ICallback _callback;
            private readonly bool _oneTimeListener;

            /// <summary>
            /// Initializes a new instance of the <see cref="EventEmitterListener"/> class.
            /// </summary>
            /// <param name="eventEmitter">The parent event emitter.</param>
            /// <param name="event">The event name.</param>
            /// <param name="callback">The callback method.</param>
            public EventEmitterListener(EventEmitter eventEmitter, string @event, ICallback callback) {
                _eventEmitter = eventEmitter;
                _event = @event;
                _callback = callback;
                _oneTimeListener = false;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="EventEmitterListener"/> class with the option for one-time listening.
            /// </summary>
            /// <param name="eventEmitter">The parent event emitter.</param>
            /// <param name="event">The event name.</param>
            /// <param name="callback">The callback method.</param>
            /// <param name="oneTimeListener">If set to <c>true</c>, the listener will be removed after the first activation.</param>
            public EventEmitterListener(EventEmitter eventEmitter, string @event, ICallback callback, bool oneTimeListener) {
                _eventEmitter = eventEmitter;
                _event = @event;
                _callback = callback;
                _oneTimeListener = oneTimeListener;
            }


            public void Activate(params object?[]? parameters) {
                if (_oneTimeListener)
                    _eventEmitter.RemoveListener(_event, _callback);
                _callback.Main(parameters);
            }
            public string GetEvent() => _event;
            public ICallback GetCallback() => _callback;
            public bool GetOneTimeListener() => _oneTimeListener;
        }

        /// <summary>
        /// Map of events -> list of callbacks.
        /// </summary>
        private readonly Dictionary<string, List<IEventEmitterListener>> _listeners = new Dictionary<string, List<IEventEmitterListener>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="EventEmitter"/> class.
        /// </summary>
        public EventEmitter() { }

        // Private stuff

        private void AddEventListener(string @event, EventEmitterListener eventEmitterListener, bool prependListener) {
            if (!_listeners.TryGetValue(@event, out List<IEventEmitterListener>? value)) {
                if (_listeners.Count >= MAX_EVENTS)
                    throw new TooManyEventsException();
                value = new List<IEventEmitterListener>();
                _listeners[@event] = value;
            }

            var eventEmitterListeners = value;
            eventEmitterListeners ??= new List<IEventEmitterListener>();

            if (eventEmitterListeners.Count >= MAX_EVENT_LISTENERS)
                throw new TooManyEventListenersException(@event);

            if (prependListener)
                eventEmitterListeners.Insert(0, eventEmitterListener);
            else
                eventEmitterListeners.Add(eventEmitterListener);

            _listeners[@event] = eventEmitterListeners;
        }

        private void AddEventListener(string @event, EventEmitterListener eventEmitterListener) {
            AddEventListener(@event, eventEmitterListener, false);
        }

        // Interface


        public void Emit(string @event, params object?[]? parameters) {
            if (_listeners.TryGetValue(@event, out List<IEventEmitterListener>? value)) {
                var eventEmitterList = value;
                if (eventEmitterList == null || eventEmitterList.Count == 0) {
                    _listeners.Remove(@event);
                    return;
                }

                Runtime.Current.UnifyLog.Debug("EventEmitter", $"Event \"{@event}\" has been fired.");

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

        public IEventEmitterListener[] Listeners(string @event) {
            if (_listeners.TryGetValue(@event, out List<IEventEmitterListener>? value)) {
                var listeners = value;
                if (listeners == null || listeners.Count == 0) {
                    _listeners.Remove(@event);
                    return Array.Empty<IEventEmitterListener>();
                }

                return listeners.ToArray();
            } else {
                return Array.Empty<IEventEmitterListener>();
            }
        }

        public int ListenersCount(string @event) {
            if (_listeners.TryGetValue(@event, out List<IEventEmitterListener>? value)) {
                var listeners = value;
                if (listeners == null || listeners.Count == 0) {
                    _listeners.Remove(@event);
                    return 0;
                }

                return listeners.Count;
            } else {
                return 0;
            }
        }

        public int ListenersCount(string @event, ICallback callback) {
            if (_listeners.TryGetValue(@event, out List<IEventEmitterListener>? value)) {
                var listeners = value;
                if (listeners == null || listeners.Count == 0) {
                    _listeners.Remove(@event);
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

        public void AddListener(string @event, ICallback callback) {
            var eventEmitterListener = new EventEmitterListener(this, @event, callback);
            AddEventListener(@event, eventEmitterListener);
        }

        public void On(string @event, ICallback callback) {
            AddListener(@event, callback);
        }

        public void Once(string @event, ICallback callback) {
            var eventEmitterListener = new EventEmitterListener(this, @event, callback, true);
            AddEventListener(@event, eventEmitterListener);
        }

        public void PrependListener(string @event, ICallback callback) {
            var eventEmitterListener = new EventEmitterListener(this, @event, callback);
            AddEventListener(@event, eventEmitterListener, true);
        }

        public void PrependOnceListener(string @event, ICallback callback) {
            var eventEmitterListener = new EventEmitterListener(this, @event, callback, true);
            AddEventListener(@event, eventEmitterListener, true);
        }

        // Remove

        public void Off(string @event, ICallback callback) => RemoveListener(@event, callback);

        public void RemoveListener(string @event, ICallback callback) {
            if (_listeners.TryGetValue(@event, out List<IEventEmitterListener>? value)) {
                var eventEmitterListeners = value;
                if (eventEmitterListeners == null || eventEmitterListeners.Count == 0) {
                    _listeners.Remove(@event);
                    return;
                }

                var newEventEmitterListeners = new List<IEventEmitterListener>(eventEmitterListeners.Count - 1);

                foreach (var eventEmitterListener in eventEmitterListeners) {
                    if (eventEmitterListener.GetCallback() != callback) {
                        newEventEmitterListeners.Add(eventEmitterListener);
                    }
                }

                _listeners.Remove(@event);
                if (eventEmitterListeners.Count >= 1)
                    _listeners[@event] = newEventEmitterListeners;
            }
        }

        public void RemoveAllListeners(string @event) => _listeners.Remove(@event);
        public void RemoveAllEvents() => _listeners.Clear();
    }
}
