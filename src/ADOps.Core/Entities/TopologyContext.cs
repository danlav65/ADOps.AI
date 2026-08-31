namespace ADOps.Core.Entities;

public sealed class TopologyContext
{
    public required IReadOnlyCollection<
        ReplicationPartnerRelationship>
        ReplicationPartners
    {
        get;
        init;
    }
}