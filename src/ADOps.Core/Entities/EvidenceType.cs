namespace ADOps.Core.Entities;

public enum EvidenceType
{
    Unknown = 0,

    Replication,

    ReplicationFailure,

    ReplicationSuccess,

    RpcFailure,

    InfrastructureHealth,

    Kerberos,

    Patch,

    Topology
}