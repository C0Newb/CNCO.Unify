namespace CNCO.Unify {
    /// <summary>
    /// Allows Unify libraries to link into the <see cref="IRuntime"/>s of other Unify libraries without depending on those classes.
    /// </summary>
    public class RuntimeLink {
        public readonly IRuntime Instance;
        public RuntimeLink(IRuntime instance) => Instance = instance;

        // override object.Equals
        public override bool Equals(object? obj) {
            return obj != null // Easy.
                && GetType() == obj.GetType() // Same type
                && ((RuntimeLink)obj).Instance.Equals(Instance); // Instances match
        }

        public override int GetHashCode() {
            return Instance.GetHashCode() & base.GetHashCode();
        }
    }
}
