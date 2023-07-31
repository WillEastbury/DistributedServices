namespace AppFrame.Interfaces;
public interface ICluster
{
    ITable<Partition> tablePartitions { get; }
    ITable<Replica> tableReplicas { get; }
    string GetClusterName();
    List<string> GetServices();
    List<Partition> GetPartitions(string service);
    List<Replica> GetReplicas(Partition partition);

    Replica GetLeaderReplica(Partition partition);
    List<Replica> GetLocalFollowers(Partition partition);
    List<Replica> GetRemoteFollowers(Partition partition);

    void RegisterNodeServices(List<string> services);
    void ElectLeaders(List<string> services, List<Partition> partitions);
    bool TryCoup(List<string> services, List<Partition> partitions);
    void ForceLeader(string service, int partitionId, string leaderNodeId);

    Partition GetPhysicalPartition(string service, string logicalPartitionId);

    // Exposed as private variables
    List<Uri> SeedNodes { get; }
    string SecurityAPIKey { get; }
}
public class Partition
{
    public int PartitionId { get; set; }
    public int PhysicalPartitionId { get; set; }
    public string HashedRangeKeyFrom { get; set; }
    public string HashedRangeKeyTo { get; set; }
    // Other properties as needed
}

public class Replica
{
    public int ReplicaId { get; set; }
    public string Endpoint { get; set; }
    public ReplicaStatus Status { get; set; }
    // Add any other relevant properties
}

public enum ReplicaStatus
{
    Active,
    Inactive,
    Failed,
    // Add more status values as needed
}