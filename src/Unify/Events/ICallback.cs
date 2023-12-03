namespace CNCO.Unify.Events {
    /// <summary>
    /// Represents a callback (method).
    /// </summary>
    public interface ICallback {
        /// <summary>
        /// The main method for the callback.
        /// </summary>
        /// <param name="parameters">Parameters to be passed to the callback.</param>
        void Main(params object?[]? parameters);
    }
}