namespace CNCO.Unify.Events {
    /// <summary>
    /// Interface for an event emitter that allows firing and handling events.
    /// </summary>
    public interface IEventEmitter : IDisposable {
        /// <summary>
        /// Fires (activates) an event.
        /// </summary>
        /// <param name="eventName">Event to fire.</param>
        /// <param name="parameters">Any data to pass to the event listener.</param>
        void Emit(string eventName, params object?[]? parameters);

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
        /// <param name="eventName">Event to get all event listeners from.</param>
        /// <returns>List of event listeners for an event.</returns>
        IEventEmitterListener[] Listeners(string eventName);

        /// <summary>
        /// Number of unique events that are being listened for.
        /// </summary>
        /// <returns>Number of listened-for events.</returns>
        int EventsCount();

        /// <summary>
        /// Gets a list of all event listeners for a specific event.
        /// </summary>
        /// <param name="eventName">Event to get all event listeners from.</param>
        /// <returns>List of event listeners for an event.</returns>
        int ListenersCount(string eventName);

        /// <summary>
        /// Count of event listeners implementing a particular callback in an event.
        /// </summary>
        /// <param name="eventName">Event to count the number of event listeners with a particular callback.</param>
        /// <param name="callback">Callback to look for.</param>
        /// <returns>Number of event listeners with the given callback in an event.</returns>
        int ListenersCount(string eventName, ICallback callback);

        // Add

        /// <summary>
        /// Adds an event listener.
        /// </summary>
        /// <param name="eventName">
        /// Event to listen for.
        /// Name is case insensitive, cannot be longer than 128 characters.
        /// </param>
        /// <param name="callback">Callback method fired when event is received.</param>
        /// <exception cref="TooManyEventListenersException">Thrown when there are too many listeners for this event.</exception>
        /// <exception cref="TooManyEventsException">Thrown when there are too many events created.</exception>
        /// <exception cref="InvalidEventNameException">Thrown when the even name is invalid, such as being too long.</exception>
        void AddListener(string eventName, ICallback callback);

        /// <summary>
        /// Adds an event listener. No checks are made to see if `callback` is already added.
        /// Therefore, multiple calls passing the same `callback` will result in `callback` being added and activated multiple times.
        /// </summary>
        /// <param name="eventName">
        /// Event to listen for.
        /// Name is case insensitive, cannot be longer than 128 characters.
        /// </param>
        /// <param name="callback">Callback method fired when event is received.</param>
        /// <exception cref="TooManyEventListenersException">Thrown when there are too many listeners for this event.</exception>
        /// <exception cref="TooManyEventsException">Thrown when there are too many events created.</exception>
        /// <exception cref="InvalidEventNameException">Thrown when the even name is invalid, such as being too long.</exception>
        void On(string eventName, ICallback callback);

        /// <summary>
        /// Adds an event listener that is fired one time. No checks are made to see if `callback` is already added.
        /// Therefore, multiple calls passing the same `callback` will result in `callback` being added and activated multiple times.
        /// </summary>
        /// <param name="eventName">
        /// Event to listen for.
        /// Name is case insensitive, cannot be longer than 128 characters.
        /// </param>
        /// <param name="callback">Callback method fired when event is received.</param>
        /// <exception cref="TooManyEventListenersException">Thrown when there are too many listeners for this event.</exception>
        /// <exception cref="TooManyEventsException">Thrown when there are too many events created.</exception>
        /// <exception cref="InvalidEventNameException">Thrown when the even name is invalid, such as being too long.</exception>
        void Once(string eventName, ICallback callback);

        /// <summary>
        /// Adds an event listener to the front of the event listeners list for an event.
        /// This makes this event fire first.
        /// </summary>
        /// <param name="eventName">
        /// Event to listen for.
        /// Name is case insensitive, cannot be longer than 128 characters.
        /// </param>
        /// <param name="callback">Callback method fired when event is received.</param>
        /// <exception cref="TooManyEventListenersException">Thrown when there are too many listeners for this event.</exception>
        /// <exception cref="TooManyEventsException">Thrown when there are too many events created.</exception>
        /// <exception cref="InvalidEventNameException">Thrown when the even name is invalid, such as being too long.</exception>
        void PrependListener(string eventName, ICallback callback);

        /// <summary>
        /// Adds an event listener, which will be fired once, to the front of the event listeners list for an event.
        /// This makes this event fire first, but only once.
        /// </summary>
        /// <param name="eventName">
        /// Event to listen for.
        /// Name is case insensitive, cannot be longer than 128 characters.
        /// </param>
        /// <param name="callback">Callback method fired when event is received.</param>
        /// <exception cref="TooManyEventListenersException">Thrown when there are too many listeners for this event.</exception>
        /// <exception cref="TooManyEventsException">Thrown when there are too many events created.</exception>
        /// <exception cref="InvalidEventNameException">Thrown when the even name is invalid, such as being too long.</exception>
        void PrependOnceListener(string eventName, ICallback callback);

        // Remove

        /// <summary>
        /// Removes a listener.
        /// </summary>
        /// <param name="eventName">Event to remove the listener from.</param>
        /// <param name="callback">Listener to remove.</param>
        void Off(string eventName, ICallback callback);

        /// <summary>
        /// Removes a listener.
        /// </summary>
        /// <param name="eventName">Event to remove the listener from.</param>
        /// <param name="callback">Listener to remove.</param>
        void RemoveListener(string eventName, ICallback callback);

        /// <summary>
        /// Removes all listeners for a specific event.
        /// </summary>
        /// <param name="eventName">Event to remove all listeners from.</param>
        void RemoveAllListeners(string eventName);

        /// <summary>
        /// Removes all events and listeners.
        /// </summary>
        void RemoveAllEvents();
    }
}
