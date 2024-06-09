namespace CNCO.Unify {
    /// <summary>
    /// Hook that can run when <see cref="UnifyRuntime.Initialize"/> is called.
    /// </summary>
    public class RuntimeHook {
        /// <summary>
        /// Name for the hook that is logged before being executed.
        /// Truncated to 75 characters.
        /// </summary>
        public string Name;

        /// <summary>
        /// Friendly description of the hook that is logged before being executed.
        /// Truncated to 250 characters.
        /// </summary>
        public string? Description;

        /// <summary>
        /// The hook code that will be executed.
        /// </summary>
        public Action<UnifyRuntime> Action;

        public RuntimeHook(string name, Action<UnifyRuntime> action) {
            Name = name;
            Action = action;
        }
    }
}
