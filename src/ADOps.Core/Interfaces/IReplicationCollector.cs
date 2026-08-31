using ADOps.Core.Entities;
using ADOps.Core.Entities.Replication;

namespace ADOps.Core.Interfaces;

/// <summary>
/// Defines the contract for collecting Active Directory replication evidence.
/// </summary>
public interface IReplicationCollector
{
    /// <summary>
    /// Collects replication records for the supplied investigation context.
    /// </summary>
    Task<IReadOnlyCollection<ReplicationRecord>> CollectAsync(
        CollectorContext context,
        CancellationToken cancellationToken = default);
}
