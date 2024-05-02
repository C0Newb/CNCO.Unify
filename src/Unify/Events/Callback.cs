namespace CNCO.Unify.Events {
    /// <summary>
    /// Easy class to initialize a basic <see cref="ICallback"/> using a lambda function.
    /// </summary>
    public class Callback : ICallback {
        private readonly Action<object?[]?>? callback;

        public Callback() => callback = (_) => { };
        public Callback(Action<object?[]?> callback) => this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
        public void Main(params object?[]? parameters) => callback?.Invoke(parameters);
    }
}
