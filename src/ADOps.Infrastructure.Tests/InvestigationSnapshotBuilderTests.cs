using ADOps.Core.Entities;
using ADOps.Core.Entities.Replication;
using ADOps.Core.Interfaces;
using ADOps.Infrastructure.Evidence;
using ADOps.Infrastructure.Investigation;

using EvidenceEntity = ADOps.Core.Entities.Evidence;

namespace ADOps.Infrastructure.Tests;

public sealed class InvestigationSnapshotBuilderTests
{
    [Fact]
public async Task BuildAsync_DerivesTopologyFromReplicationRecords()
{
    var firstCollectedUtc =
        new DateTimeOffset(
            2026,
            7,
            9,
            14,
            25,
            0,
            TimeSpan.Zero);

    var secondCollectedUtc =
        firstCollectedUtc.AddMinutes(5);

    var context =
        new CollectorContext
        {
            InvestigationId = "INC-SFO-20260709",
            Site = "SFO",
            DomainName = "apcflex.aero",
            DomainControllers =
                ["SFOFLEX-DC1"]
        };

    var replicationRecords =
        new[]
        {
            new ReplicationRecord
            {
                SourceDomainController = "SFOFLEX-DC1",
                PartnerDomainController = "ZUSW-DC1",
                SourceSite = "SFO",
                PartnerSite = "ZUSW",
                Success = false,
                ErrorCode = 1722,
                ErrorMessage = "RPC server unavailable",
                CollectedUtc = firstCollectedUtc
            },

            new ReplicationRecord
            {
                SourceDomainController = "SFOFLEX-DC1",
                PartnerDomainController = "ZUSW-DC1",
                SourceSite = "SFO",
                PartnerSite = "ZUSW",
                Success = true,
                CollectedUtc = secondCollectedUtc
            }
        };

    var builder =
        new InvestigationSnapshotBuilder(
            new FakeReplicationCollector(
                replicationRecords),
            new FakePatchCollector([]),
            new FakeSystemInfoCollector([]),
            new FakeRpcCollector([]),
            new FakeEvidenceNormalizer());

    var snapshot =
        await builder.BuildAsync(context);

    Assert.NotNull(snapshot.Topology);

    var relationship =
        Assert.Single(
            snapshot.Topology.ReplicationPartners);

    Assert.Equal(
        "SFOFLEX-DC1",
        relationship.SourceDomainController);

    Assert.Equal(
        "ZUSW-DC1",
        relationship.PartnerDomainController);

    Assert.Equal(
        "SFO",
        relationship.SourceSite);

    Assert.Equal(
        "ZUSW",
        relationship.PartnerSite);

    Assert.True(
        relationship.IsActive);

    Assert.Equal(
        secondCollectedUtc,
        relationship.DiscoveredUtc);
}

    private sealed class FakeReplicationCollector
        : IReplicationCollector
    {
        private readonly IReadOnlyCollection<ReplicationRecord> _records;

        public FakeReplicationCollector(
            IReadOnlyCollection<ReplicationRecord> records)
        {
            _records = records;
        }

        public Task<IReadOnlyCollection<ReplicationRecord>> CollectAsync(
            CollectorContext context,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_records);
        }
    }

    private sealed class FakePatchCollector
        : IPatchCollector
    {
        private readonly IReadOnlyCollection<PatchRecord> _records;

        public FakePatchCollector(
            IReadOnlyCollection<PatchRecord> records)
        {
            _records = records;
        }

        public Task<IReadOnlyCollection<PatchRecord>> CollectAsync(
            CollectorContext context,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_records);
        }
    }

    private sealed class FakeSystemInfoCollector
        : ISystemInfoCollector
    {
        private readonly IReadOnlyCollection<SystemInfoRecord> _records;

        public FakeSystemInfoCollector(
            IReadOnlyCollection<SystemInfoRecord> records)
        {
            _records = records;
        }

        public Task<IReadOnlyCollection<SystemInfoRecord>> CollectAsync(
            CollectorContext context,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_records);
        }
    }

    private sealed class FakeRpcCollector
    : IRpcCollector
    {
        private readonly IReadOnlyCollection<RpcRecord> _records;

        public FakeRpcCollector(
            IReadOnlyCollection<RpcRecord> records)
        {
        _records = records;
        }

        public Task<IReadOnlyCollection<RpcRecord>> CollectAsync(
            CollectorContext context,
            TopologyContext topology,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_records);
        }
    }
    
    private sealed class FakeEvidenceNormalizer
        : IEvidenceNormalizer
    {
        public bool WasCalled { get; private set; }

        public InvestigationSnapshot? SnapshotReceived { get; private set; }

        public IReadOnlyCollection<EvidenceEntity> Normalize(
            InvestigationSnapshot snapshot)
        {
            WasCalled = true;
            SnapshotReceived = snapshot;

            return [];
        }
    }
}