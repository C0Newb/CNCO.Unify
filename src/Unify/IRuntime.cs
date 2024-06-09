namespace CNCO.Unify {
    public interface IRuntime {
        /// <summary>
        /// Initializes this runtime, linking into necessary <see cref="IRuntime"/>s and calling registered hooks.
        /// </summary>
        public void Initialize();
    }
}
