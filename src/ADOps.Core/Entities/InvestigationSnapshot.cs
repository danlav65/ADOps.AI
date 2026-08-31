using ADOps.Core.Entities.Replication;

namespace ADOps.Core.Entities;

/// <summary>
/// Represents a point-in-time snapshot of collected investigation data.
/// </summary>
public sealed class InvestigationSnapshot
{
    /// <summary>
    /// Unique identifier of the investigation.
    /// </summary>
    public required string InvestigationId { get; init; }

    /// <summary>
    /// UTC timestamp when the investigation started.
    /// </summary>
    public required DateTimeOffset StartedUtc { get; init; }

    /// <summary>
    /// UTC timestamp when the investigation completed.
    /// </summary>
    public DateTimeOffset? CompletedUtc { get; set; }

    /// <summary>
    /// Operational context collected for the investigation.
    /// </summary>
    public required OperationalContext OperationalContext
    {
        get;
        init;
    }

    /// <summary>
    /// Active Directory topology context associated with the investigation.
    /// </summary>
    public TopologyContext? Topology
    {
        get;
        set;
    }

    /// <summary>
    /// Replication records collected during the investigation.
    /// </summary>
    public List<ReplicationRecord> Replication
    {
        get;
    } = new();

    /// <summary>
    /// Patch information collected during the investigation.
    /// </summary>
    public List<PatchRecord> Patches
    {
        get;
    } = new();

    /// <summary>
    /// System information collected during the investigation.
    /// </summary>
    public List<SystemInfoRecord> SystemInfo
    {
        get;
    } = new();

    /// <summary>
    /// Evidence collected during the investigation.
    /// </summary>
    public List<Evidence> Evidence
    {
        get;
    } = new();

    /// <summary>
    /// Correlations identified during analysis.
    /// </summary>
    public List<CorrelatedFinding> Correlations
    {
        get;
    } = new();

    /// <summary>
    /// RPC connectivity records collected during the investigation.
    /// </summary>
    public List<RpcRecord> Rpc
    {
        get;
    } = new();

    /// <summary>
    /// AI-generated findings produced during analysis.
    /// </summary>
    public List<AIFinding> Findings
    {
        get;
    } = new();
}