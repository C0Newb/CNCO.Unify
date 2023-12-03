namespace CNCO.Unify.Events {
    /// <summary>
    /// Interface for an event emitter that allows firing and handling events.
    /// </summary>
    public interface IEventEmitter {
        /// <summary>
        /// Fires (activates) an event.
        /// </summary>
        /// <param name="event">Event to fire.</param>
        /// <param name="parameters">Any data to pass to the event listener.</param>
        void Emit(string @event, params object?[]? parameters);

        /// <summary>
        /// Returns the maximum number of allowed event listeners for any event (<see cref="EventEmitter.MAX_EVENT_LISTENERS"/>).
        /// </summary>
        /// <returns>Number of event listeners allowed to be added to an event.</returns>
        int GetMaxListeners();

        /// <summary>
        /// Gets the maximum number of unique events (that can be emitted/listened for, see <see cref="EventEmitter.MAX_EVENTS"/>).
        /// </summary>
        /// <returns>Number of events allowed to exist.</returns>
        int GetMaxEvents();

        /// <summary>
        /// Gets a list of all event names that have been created.
        /// </summary>
        /// <returns>List of event names.</returns>
        string[] Events();

        /// <summary>
        /// Gets a list of all event listeners for a specific event.
        /// </summary>
        /// <param name="event">Event to get all event listeners from.</param>
        /// <returns>List of event listeners for an event.</returns>
        IEventEmitterListener[] Listeners(string @event);

        /// <summary>
        /// Number of unique events that are being listened for.
        /// </summary>
        /// <returns>Number of listened-for events.</returns>
        int EventsCount();

        /// <summary>
        /// Gets a list of all event listeners for a specific event.
        /// </summary>
        /// <param name="event">Event to get all event listeners from.</param>
        /// <returns>List of event listeners for an event.</returns>
        int ListenersCount(string @event);

        /// <summary>
        /// Count of event listeners implementing a particular callback in an event.
        /// </summary>
        /// <param name="event">Event to count the number of event listeners with a particular callback.</param>
        /// <param name="callback">Callback to look for.</param>
        /// <returns>Number of event listeners with the given callback in an event.</returns>
        int ListenersCount(string @event, ICallback callback);

        // Add

        /// <summary>
        /// Adds an event listener.
        /// </summary>
        /// <param name="event">Event to listen for.</param>
        /// <param name="callback">Callback method fired when event is received.</param>
        /// <exception cref="TooManyEventListenersException">Thrown when there are too many listeners for this event.</exception>
        /// <exception cref="TooManyEventsException">Thrown when there are too many events created.</exception>
        void AddListener(string @event, ICallback callback);

        /// <summary>
        /// Adds an event listener. No checks are made to see if `callback` is already added.
        /// Therefore, multiple calls passing the same `callback` will result in `callback` being added and activated multiple times.
        /// </summary>
        /// <param name="event">Event to listen for.</param>
        /// <param name="callback">Callback method fired when event is received.</param>
        /// <exception cref="TooManyEventListenersException">Thrown when there are too many listeners for this event.</exception>
        /// <exception cref="TooManyEventsException">Thrown when there are too many events created.</exception>
        void On(string @event, ICallback callback);

        /// <summary>
        /// Adds an event listener that is fired one time. No checks are made to see if `callback` is already added.
        /// Therefore, multiple calls passing the same `callback` will result in `callback` being added and activated multiple times.
        /// </summary>
        /// <param name="event">Event to listen for.</param>
        /// <param name="callback">Callback method fired when event is received.</param>
        /// <exception cref="TooManyEventListenersException">Thrown when there are too many listeners for this event.</exception>
        /// <exception cref="TooManyEventsException">Thrown when there are too many events created.</exception>
        void Once(string @event, ICallback callback);

        /// <summary>
        /// Adds an event listener to the front of the event listeners list for an event.
        /// This makes this event fire first.
        /// </summary>
        /// <param name="event">Event to listen for.</param>
        /// <param name="callback">Callback method fired when event is received.</param>
        /// <exception cref="TooManyEventListenersException">Thrown when there are too many listeners for this event.</exception>
        /// <exception cref="TooManyEventsException">Thrown when there are too many events created.</exception>
        void PrependListener(string @event, ICallback callback);

        /// <summary>
        /// Adds an event listener, which will be fired once, to the front of the event listeners list for an event.
        /// This makes this event fire first, but only once.
        /// </summary>
        /// <param name="event">Event to listen for.</param>
        /// <param name="callback">Callback method fired when event is received.</param>
        /// <exception cref="TooManyEventListenersException">Thrown when there are too many listeners for this event.</exception>
        /// <exception cref="TooManyEventsException">Thrown when there are too many events created.</exception>
        void PrependOnceListener(string @event, ICallback callback);

        // Remove

        /// <summary>
        /// Removes a listener.
        /// </summary>
        /// <param name="event">Event to remove the listener from.</param>
        /// <param name="callback">Listener to remove.</param>
        void Off(string @event, ICallback callback);

        /// <summary>
        /// Removes a listener.
        /// </summary>
        /// <param name="event">Event to remove the listener from.</param>
        /// <param name="callback">Listener to remove.</param>
        void RemoveListener(string @event, ICallback callback);

        /// <summary>
        /// Removes all listeners for a specific event.
        /// </summary>
        /// <param name="event">Event to remove all listeners from.</param>
        void RemoveAllListeners(string @event);

        /// <summary>
        /// Removes all events and listeners.
        /// </summary>
        void RemoveAllEvents();
    }
}
