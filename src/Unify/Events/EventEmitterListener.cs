namespace CNCO.Unify.Events {
    /// <summary>
    /// Event emitter listener, this is the object that holds the callback class and allows for one-time listening.
    /// (This is what listens for an event).
    /// </summary>
    public class EventEmitterListener : IEventEmitterListener {
        private readonly IEventEmitter _eventEmitter;
        private readonly string _event;
        private readonly ICallback _callback;
        private readonly bool _oneTimeListener;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventEmitterListener"/> class.
        /// </summary>
        /// <param name="eventEmitter">The parent event emitter.</param>
        /// <param name="eventName">The event name.</param>
        /// <param name="callback">The callback method.</param>
        public EventEmitterListener(IEventEmitter eventEmitter, string eventName, ICallback callback) {
            _eventEmitter = eventEmitter;
            _event = eventName;
            _callback = callback;
            _oneTimeListener = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventEmitterListener"/> class with the option for one-time listening.
        /// </summary>
        /// <param name="eventEmitter">The parent event emitter.</param>
        /// <param name="eventName">The event name.</param>
        /// <param name="callback">The callback method.</param>
        /// <param name="oneTimeListener">If set to <c>true</c>, the listener will be removed after the first activation.</param>
        public EventEmitterListener(IEventEmitter eventEmitter, string eventName, ICallback callback, bool oneTimeListener) : this(eventEmitter, eventName, callback) {
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
}
