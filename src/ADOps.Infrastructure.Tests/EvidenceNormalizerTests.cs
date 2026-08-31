using ADOps.Core.Entities;
using ADOps.Core.Entities.Replication;
using ADOps.Infrastructure.Evidence;

namespace ADOps.Infrastructure.Tests;

public sealed class EvidenceNormalizerTests
{
    [Fact]
    public void Normalize_ConvertsReplicationFailureToEvidence()
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

        var snapshot =
            CreateSnapshot();

        snapshot.Replication.Add(
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
            });

        var normalizer =
            new EvidenceNormalizer(
                new FakeEvidenceIdGenerator());

        var evidence =
            normalizer.Normalize(snapshot);

        var item =
            Assert.Single(evidence);

        Assert.Equal(
            "EV-TEST-001",
            item.EvidenceId);

        Assert.Equal(
            snapshot.InvestigationId,
            item.InvestigationId);

        Assert.Equal(
            EvidenceType.ReplicationFailure,
            item.Type);

        Assert.Equal(
            "SFOFLEX-DC1",
            item.Source);

        Assert.Equal(
            "ZUSW-DC1",
            item.Target);

        Assert.Equal(
            collectedUtc,
            item.CollectedUtc);

        Assert.Equal(
            "1722",
            item.ErrorCode);

        Assert.Equal(
            "RPC server unavailable",
            item.Details);

        Assert.True(item.IsValid);
    }

    [Fact]
    public void Normalize_ConvertsRpcFailureToEvidence()
    {
        var collectedUtc =
            new DateTimeOffset(
                2026,
                7,
                9,
                14,
                27,
                0,
                TimeSpan.Zero);

        var snapshot =
            CreateSnapshot();

        snapshot.Rpc.Add(
            new RpcRecord
            {
                DomainController = "SFOFLEX-DC1",
                Target = "ZUSW-DC1",
                SourceAddress = "10.10.1.10",
                RemoteAddress = "10.20.1.10",
                RemotePort = 135,
                InterfaceAlias = "Ethernet",
                Success = false,
                ErrorCode = 1722,
                ErrorMessage = "RPC server unavailable",
                CollectedUtc = collectedUtc,
                SourceCommand = "Test-NetConnection"
            });

        var normalizer =
            new EvidenceNormalizer(
                new FakeEvidenceIdGenerator());

        var evidence =
            normalizer.Normalize(snapshot);

        var item =
            Assert.Single(evidence);

        Assert.Equal(
            "EV-TEST-001",
            item.EvidenceId);

        Assert.Equal(
            snapshot.InvestigationId,
            item.InvestigationId);

        Assert.Equal(
            EvidenceType.RpcFailure,
            item.Type);

        Assert.Equal(
            "SFOFLEX-DC1",
            item.Source);

        Assert.Equal(
            "ZUSW-DC1",
            item.Target);

        Assert.Equal(
            collectedUtc,
            item.CollectedUtc);

        Assert.Equal(
            "1722",
            item.ErrorCode);

        Assert.Equal(
            "RPC server unavailable",
            item.Details);

        Assert.True(item.IsValid);
    }

    [Fact]
    public void Normalize_ConvertsPatchToEvidence()
    {
        var collectedUtc =
            new DateTimeOffset(
                2026,
                7,
                9,
                14,
                30,
                0,
                TimeSpan.Zero);

        var snapshot =
            CreateSnapshot();

        snapshot.Patches.Add(
            new PatchRecord
            {
                DomainController = "SFOFLEX-DC1",
                Site = "SFO",
                KnowledgeBaseArticle = "KB5060531",
                Installed = true,
                OperatingSystem = "Windows Server 2022",
                OsBuild = "20348",
                PatchVersion = "2026.07",
                InstalledUtc = collectedUtc,
                CollectedUtc = collectedUtc
            });

        var normalizer =
            new EvidenceNormalizer(
                new FakeEvidenceIdGenerator());

        var evidence =
            normalizer.Normalize(snapshot);

        var item =
            Assert.Single(evidence);

        Assert.Equal(
            EvidenceType.Patch,
            item.Type);

        Assert.Equal(
            "PatchCollector",
            item.Source);

        Assert.Equal(
            "SFOFLEX-DC1",
            item.Target);

        Assert.Equal(
            collectedUtc,
            item.CollectedUtc);

        Assert.Contains(
            "KB5060531",
            item.Summary);

        Assert.Contains(
            "Windows Server 2022",
            item.Details);

        Assert.Contains(
            "20348",
            item.Details);

        Assert.True(item.IsValid);
    }

    [Fact]
    public void Normalize_ConvertsSystemInfoToInfrastructureEvidence()
    {
        var collectedUtc =
            new DateTimeOffset(
                2026,
                7,
                9,
                14,
                35,
                0,
                TimeSpan.Zero);

        var snapshot =
            CreateSnapshot();

        snapshot.SystemInfo.Add(
            new SystemInfoRecord
            {
                DomainController = "SFOFLEX-DC1",
                Site = "SFO",
                ComputerName = "SFOFLEX-DC1",
                OperatingSystem = "Windows Server 2022",
                OsVersion = "10.0",
                BuildNumber = "20348",
                Edition = "Standard",
                Architecture = "x64",
                LogicalProcessors = 8,
                PhysicalMemoryGb = 32,
                SystemDriveFreeSpaceGb = 100,
                VirtualMachine = true,
                Hypervisor = "Azure",
                TimeZone = "UTC",
                PowerShellVersion = "7.5",
                DotNetVersion = "10.0",
                CollectedUtc = collectedUtc
            });

        var normalizer =
            new EvidenceNormalizer(
                new FakeEvidenceIdGenerator());

        var evidence =
            normalizer.Normalize(snapshot);

        var item =
            Assert.Single(evidence);

        Assert.Equal(
            EvidenceType.InfrastructureHealth,
            item.Type);

        Assert.Equal(
            "SystemInfoCollector",
            item.Source);

        Assert.Equal(
            "SFOFLEX-DC1",
            item.Target);

        Assert.Equal(
            collectedUtc,
            item.CollectedUtc);

        Assert.Contains(
            "Windows Server 2022",
            item.Details);

        Assert.Contains(
            "100",
            item.Details);

        Assert.True(item.IsValid);
    }

    private static InvestigationSnapshot CreateSnapshot()
    {
        return new InvestigationSnapshot
        {
            InvestigationId = "INC-SFO-20260709",
            StartedUtc = DateTimeOffset.UtcNow,
            OperationalContext =
                new OperationalContext
                {
                    Target = "apcflex.aero",
                    Site = "SFO",
                    Environment = "Production"
                }
        };
    }

    private sealed class FakeEvidenceIdGenerator
        : IEvidenceIdGenerator
    {
        public string Generate()
        {
            return "EV-TEST-001";
        }
    }
}