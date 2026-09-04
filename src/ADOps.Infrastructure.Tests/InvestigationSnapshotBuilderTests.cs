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

    [Fact]
    public async Task BuildAsync_PassesDerivedTopologyToRpcCollector()
    {
        var collectedUtc =
            new DateTimeOffset(
                2026,
                7,
                9,
                14,
                25,
                0,
                TimeSpan.Zero);

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
                    CollectedUtc = collectedUtc
                }
            };

        var rpcCollector =
            new FakeRpcCollector([]);

        var builder =
            new InvestigationSnapshotBuilder(
            new FakeReplicationCollector(
                replicationRecords),
            new FakePatchCollector([]),
            new FakeSystemInfoCollector([]),
            rpcCollector,
            new FakeEvidenceNormalizer());

    await builder.BuildAsync(context);

    Assert.NotNull(rpcCollector.TopologyReceived);

    var relationship =
        Assert.Single(
            rpcCollector.TopologyReceived!.ReplicationPartners);

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
    }

    [Fact]
    public async Task BuildAsync_NormalizesEvidenceFromCompletedSnapshot()
    {
        var collectedUtc =
            new DateTimeOffset(
                2026,
                7,
                9,
                14,
                25,
                0,
                TimeSpan.Zero);

        var context =
            new CollectorContext
            {
                InvestigationId = "INC-SFO-20260709",
                Site = "SFO",
                DomainName = "apcflex.aero",
                DomainControllers =
                    ["SFOFLEX-DC1"]
            };

        var normalizer =
            new FakeEvidenceNormalizer();

        var builder =
            new InvestigationSnapshotBuilder(
            new FakeReplicationCollector([]),
            new FakePatchCollector([]),
            new FakeSystemInfoCollector([]),
            new FakeRpcCollector([]),
            normalizer);

        var snapshot =
            await builder.BuildAsync(context);

        Assert.True(
            normalizer.WasCalled);

        Assert.Same(
            snapshot,
            normalizer.SnapshotReceived);
    }

    [Fact]
    public async Task BuildAsync_AddsCollectorResultsToSnapshot()
    {
        var collectedUtc =
            new DateTimeOffset(
                2026,
                7,
                9,
                14,
                25,
                0,
                TimeSpan.Zero);

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
                    CollectedUtc = collectedUtc
                }
            };

        var patchRecords =
            new[]
            {
                new PatchRecord
                {
                    DomainController = "SFOFLEX-DC1",
                    Site = "SFO",
                    OperatingSystem = "Windows Server 2019",
                    OsBuild = "17763",
                    KnowledgeBaseArticle = "KB0000000",
                    PatchVersion = "June 2026 CU",
                    InstalledUtc = collectedUtc,
                    Installed = false,
                    CollectedUtc = collectedUtc
                }
            };

        var systemInfoRecords =
            new[]
            {
                new SystemInfoRecord
                {
                    DomainController = "SFOFLEX-DC1",
                    Site = "SFO",
                    ComputerName = "SFOFLEX-DC1",
                    OperatingSystem = "Windows Server 2019",
                    OsVersion = "10.0",
                    BuildNumber = "17763",
                    Edition = "Standard",
                    Architecture = "64-bit",
                    TimeZone = "UTC",
                    SystemDriveFreeSpaceGb = 3.2,
                    LogicalProcessors = 8,
                    PhysicalMemoryGb = 32,
                    VirtualMachine = true,
                    Hypervisor = "Microsoft Hyper-V",
                    PowerShellVersion = "5.1",
                    DotNetVersion = "4.8",
                    CollectedUtc = collectedUtc
                }
            };

        var rpcRecords =
            new[]
            {
                new RpcRecord
                {
                    DomainController = "SFOFLEX-DC1",
                    Target = "SFOFLEX-DC1",
                    Success = false,
                    ErrorCode = 1722,
                    ErrorMessage = "RPC server unavailable",
                    CollectedUtc = collectedUtc
                }
            };

        var builder =
            new InvestigationSnapshotBuilder(
            new FakeReplicationCollector(
                replicationRecords),
            new FakePatchCollector(
                patchRecords),
            new FakeSystemInfoCollector(
                systemInfoRecords),
            new FakeRpcCollector(
                rpcRecords),
            new FakeEvidenceNormalizer());

        var snapshot =
            await builder.BuildAsync(context);

        Assert.Single(snapshot.Replication);
        Assert.Equal(
            "SFOFLEX-DC1",
            snapshot.Replication.Single().SourceDomainController);

        Assert.Single(snapshot.Patches);
        Assert.Equal(
            "SFOFLEX-DC1",
            snapshot.Patches.Single().DomainController);

        Assert.Single(snapshot.SystemInfo);
        Assert.Equal(
            "SFOFLEX-DC1",
            snapshot.SystemInfo.Single().DomainController);

        Assert.Single(snapshot.Rpc);
        Assert.Equal(
            "SFOFLEX-DC1",
            snapshot.Rpc.Single().Target);

        var systemInfo =
            snapshot.SystemInfo.Single();

        Assert.Equal(
            "SFOFLEX-DC1",
            systemInfo.DomainController);

        Assert.Equal(
            3.2,
            systemInfo.SystemDriveFreeSpaceGb);

        Assert.Equal(
            "64-bit",
            systemInfo.Architecture);

        Assert.Equal(
            "4.8",
            systemInfo.DotNetVersion);

        Assert.True(
            systemInfo.VirtualMachine);

        Assert.Equal(
            "Microsoft Hyper-V",
            systemInfo.Hypervisor);
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

        public TopologyContext? TopologyReceived { get; private set; }

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
            TopologyReceived = topology;

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