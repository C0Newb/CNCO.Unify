namespace CNCO.Unify.Events {
    public interface IEventEmitterListener {
        /// <summary>
        /// Calls the callback method. This is called when the event is emitted.
        /// Parameters passed depend on the event you're listening to.
        /// </summary>
        /// <param name="parameters">Parameters passed by the event.</param>
        void Activate(params object?[]? parameters);

        /// <summary>
        /// Gets the event name this event emitter listener is listening to.
        /// </summary>
        /// <returns>Event name the event emitter listener is listening for.</returns>
        string GetEvent();

        /// <summary>
        /// Gets the callback method that will be called when activated.
        /// </summary>
        /// <returns>Method called when the event is emitted.</returns>
        ICallback GetCallback();

        /// <summary>
        /// Whether this event emitter listener will remove itself after the first activation,
        /// making this listener a one-time listener.
        /// </summary>
        /// <returns>Whether this event listener is called one time after an event is emitted or until removed.</returns>
        bool GetOneTimeListener();
    }
}
