namespace CNCO.Unify {

    /// <summary>
    /// Signals to a <see cref="IRuntime"/> that this class should be linked and initialized with during that <see cref="IRuntime"/>'s initialization.
    /// By default, the <see cref="IRuntime"/> is the <see cref="UnifyRuntime"/>.
    /// </summary>
    /// <remarks>
    /// Allows Unify libraries to link into the <see cref="IRuntime"/>s of other Unify libraries without depending on those classes.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class LinkRuntimeAttribute : Attribute {
        readonly Type _runtime;

        /// <summary>
        /// Link to a specific runtime.
        /// </summary>
        /// <param name="runtime">Runtime to link to.</param>
        public LinkRuntimeAttribute(Type runtime) {
            _runtime = runtime;
        }

        public LinkRuntimeAttribute() : this(typeof(UnifyRuntime)) { }

        /// <summary>
        /// Runtime to link to.
        /// </summary>
        public Type RuntimeType {
            get => _runtime;
        }
    }
}
