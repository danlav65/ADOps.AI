namespace ADOps.Core.Entities.Replication;

public sealed class ReplicationRecord
{
    /// <summary>
    /// Domain controller where the replication check originated.
    /// Example: SFOFLEX-DC1
    /// </summary>
    public required string SourceDomainController { get; init; }

    /// <summary>
    /// Partner domain controller involved in replication.
    /// Example: ZUSW-DC1
    /// </summary>
    public required string PartnerDomainController { get; init; }

    /// <summary>
    /// Active Directory site of the source DC.
    /// </summary>
    public string? SourceSite { get; init; }

    /// <summary>
    /// Active Directory site of the partner DC.
    /// </summary>
    public string? PartnerSite { get; init; }

    /// <summary>
    /// Whether replication succeeded.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Replication error code if failure occurred.
    /// Example: 1722 (RPC Server Unavailable)
    /// </summary>
    public int? ErrorCode { get; init; }

    /// <summary>
    /// Human-readable replication error.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Last known successful replication timestamp.
    /// </summary>
    public DateTimeOffset? LastSuccessfulReplicationUtc { get; init; }

    /// <summary>
    /// Time this evidence was collected.
    /// </summary>
    public DateTimeOffset CollectedUtc { get; init; }

    /// <summary>
    /// Command used to collect the replication evidence.
    /// </summary>
    public string? SourceCommand { get; init; }
}