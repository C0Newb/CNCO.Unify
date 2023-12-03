namespace CNCO.Unify {
    /// <summary>
    /// Allows Unify libraries to link into the Unify Runtime without Unify depending on those classes.
    /// </summary>
    public class RuntimeLink {
        public readonly IRuntime Instance;
        public RuntimeLink(IRuntime instance) => Instance = instance;
    }
}
