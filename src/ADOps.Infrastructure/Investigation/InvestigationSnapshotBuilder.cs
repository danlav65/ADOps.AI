using ADOps.Core.Entities;
using ADOps.Core.Interfaces;
using ADOps.Infrastructure.Evidence;
using ADOps.Infrastructure.Investigation;

namespace ADOps.Infrastructure.Investigation;

public sealed class InvestigationSnapshotBuilder
    : IInvestigationSnapshotBuilder
{
    private readonly IReplicationCollector _replicationCollector;
    private readonly IPatchCollector _patchCollector;
    private readonly ISystemInfoCollector _systemInfoCollector;
    private readonly IRpcCollector _rpcCollector;
    private readonly IEvidenceNormalizer _evidenceNormalizer;

public InvestigationSnapshotBuilder(
        IReplicationCollector replicationCollector,
        IPatchCollector patchCollector,
        ISystemInfoCollector systemInfoCollector,
        IRpcCollector rpcCollector,
        IEvidenceNormalizer evidenceNormalizer)
{
        _replicationCollector = replicationCollector;
        _patchCollector = patchCollector;
        _systemInfoCollector = systemInfoCollector;
        _rpcCollector = rpcCollector;
        _evidenceNormalizer = evidenceNormalizer;
}

    public async Task<InvestigationSnapshot> BuildAsync(
        CollectorContext context,
        CancellationToken cancellationToken = default)
    {
        var snapshot = new InvestigationSnapshot
        {
            InvestigationId = context.InvestigationId,

            StartedUtc = DateTimeOffset.UtcNow,

            OperationalContext = new OperationalContext
            {
                Target = context.DomainName ?? "Unknown",
                Site = context.Site,
                Environment = "Unknown"
            }
        };

        var replication =
            await _replicationCollector.CollectAsync(
                context,
                cancellationToken);

        snapshot.Replication.AddRange(replication);

        snapshot.Topology =
            BuildTopology(snapshot.Replication);

        var patches =
            await _patchCollector.CollectAsync(
                context,
                cancellationToken);

        snapshot.Patches.AddRange(patches);

        var systemInfo =
            await _systemInfoCollector.CollectAsync(
                context,
                cancellationToken);

        var rpc =
            await _rpcCollector.CollectAsync(
                context,
                snapshot.Topology,
                cancellationToken);

        snapshot.Rpc.AddRange(rpc);

        snapshot.SystemInfo.AddRange(systemInfo);

        var evidence =
            _evidenceNormalizer.Normalize(snapshot);

        snapshot.Evidence.AddRange(evidence);

snapshot.CompletedUtc =
    DateTimeOffset.UtcNow;

return snapshot;
}

    private static TopologyContext BuildTopology(
    IReadOnlyCollection<ADOps.Core.Entities.Replication.ReplicationRecord> records)
{
    var relationships =
        records
            .Where(record =>
                !string.IsNullOrWhiteSpace(
                    record.SourceDomainController) &&
                !string.IsNullOrWhiteSpace(
                    record.PartnerDomainController))
            .GroupBy(
                record => new
                {
                    record.SourceDomainController,
                    record.PartnerDomainController
                })
            .Select(group =>
            {
                var latest =
                    group
                        .OrderByDescending(
                            record => record.CollectedUtc)
                        .First();

                return new ReplicationPartnerRelationship
                {
                    SourceDomainController =
                        latest.SourceDomainController,

                    PartnerDomainController =
                        latest.PartnerDomainController,

                    SourceSite =
                        latest.SourceSite ?? string.Empty,

                    PartnerSite =
                        latest.PartnerSite ?? string.Empty,

                    DiscoveredUtc =
                        latest.CollectedUtc,

                    IsActive = true
                };
            })
            .ToList();

    return new TopologyContext
    {
        ReplicationPartners = relationships
    };
    }
}
