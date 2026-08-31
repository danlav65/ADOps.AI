namespace ADOps.Core.Entities;

public sealed class ReplicationPartnerRelationship
{
    public required string SourceDomainController { get; init; }

    public required string PartnerDomainController { get; init; }

    public required string SourceSite { get; init; }

    public required string PartnerSite { get; init; }

    public DateTimeOffset DiscoveredUtc { get; init; }

    public bool IsActive { get; init; } = true;
}