namespace CNCO.Unify.Communications.Mdns {
    public interface IServiceProfile {
        string Domain { get; }
        string ServiceName { get; set; }
    }
}