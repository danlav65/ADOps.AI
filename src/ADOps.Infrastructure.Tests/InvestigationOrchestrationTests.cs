using ADOps.Core.Entities;
using ADOps.Core.Entities.Replication;
using ADOps.Core.Enums;
using ADOps.Core.Interfaces;
using ADOps.Infrastructure.Analysis;
using ADOps.Infrastructure.Collectors;
using ADOps.Infrastructure.Evidence;
using ADOps.Infrastructure.Investigation;
using ADOps.Infrastructure.Correlation;
using ADOps.Infrastructure.Recommendations;

namespace ADOps.Infrastructure.Tests;

public sealed class InvestigationOrchestrationTests
{
    [Fact]
    public async Task InvestigationPipeline_ProducesRcaAndRecommendations()
    {
        // Arrange

        var collectedUtc =
            new DateTimeOffset(
                2026,
                7,
                9,
                12,
                0,
                0,
                TimeSpan.Zero);

        var context = new CollectorContext
        {
            InvestigationId = "INC-SFO-20260709",
            Site = "SFO",
            DomainName = "apcflex.aero",
            DomainControllers =
            [
                "SFOFLEX-DC1",
                "ZUSW-DC1"
            ]
        };

        var snapshotBuilder =
            new InvestigationSnapshotBuilder(
                new FakeReplicationCollector(collectedUtc),
                new FakePatchCollector(collectedUtc),
                new FakeSystemInfoCollector(collectedUtc),
                new FakeRpcCollector(collectedUtc),
                new EvidenceNormalizer(
                    new EvidenceIdGenerator()));

        var correlationEngine =
            new CorrelationEngine();

        var rcaAnalyzer =
            new RootCauseAnalyzer();

        var recommendationEngine =
            new RecommendationEngine();

        // Act

        var snapshot =
            await snapshotBuilder.BuildAsync(context);

        var findings =
            correlationEngine.Correlate(
                snapshot.Evidence,
                snapshot.Topology!);

        var rca =
            rcaAnalyzer.Analyze(findings);

        var recommendations =
            recommendationEngine.Generate(
                rca,
                findings);

        // Assert - snapshot

        Assert.Equal(
            "INC-SFO-20260709",
            snapshot.InvestigationId);

        Assert.Equal(
            "apcflex.aero",
            snapshot.OperationalContext.Target);

        Assert.Equal(
            "SFO",
            snapshot.OperationalContext.Site);

        Assert.NotNull(snapshot.Topology);

        Assert.Equal(
            2,
            snapshot.Topology!.ReplicationPartners.Count);

        Assert.Contains(
            snapshot.Topology.ReplicationPartners,
            relationship =>
                relationship.SourceDomainController == "SFOFLEX-DC1" &&
                relationship.PartnerDomainController == "ZUSW-DC1");

        Assert.Contains(
            snapshot.Topology.ReplicationPartners,
            relationship =>
                relationship.SourceDomainController == "ZUSW-DC1" &&
                relationship.PartnerDomainController == "SFOFLEX-DC1");

        Assert.Equal(
            7,
            snapshot.Evidence.Count);

        Assert.NotNull(snapshot.CompletedUtc);

        // Assert - topology

        var relationship =
            Assert.Single(
                snapshot.Topology.ReplicationPartners,
                relationship =>
                    relationship.SourceDomainController == "SFOFLEX-DC1" &&
                    relationship.PartnerDomainController == "ZUSW-DC1");

Assert.Equal(
    "SFO",
    relationship.SourceSite);

Assert.Equal(
    "ZUSW",
    relationship.PartnerSite);

        Assert.Equal(
            "SFO",
            relationship.SourceSite);

        Assert.Equal(
            "ZUSW",
            relationship.PartnerSite);

        // Assert - correlation

        Console.WriteLine("=== NORMALIZED EVIDENCE ===");

        foreach (var item in snapshot.Evidence)
        {
            Console.WriteLine(
                $"{item.EvidenceId} | " +
                $"{item.Type} | " +
                $"Source={item.Source} | " +
                $"Target={item.Target} | " +
                $"CollectedUtc={item.CollectedUtc} | " +
                $"Summary={item.Summary}");
        }

        Console.WriteLine("=== END NORMALIZED EVIDENCE ===");
        
        Console.WriteLine("=== CORRELATION FINDINGS ===");

        foreach (var finding in findings)
        {
            Console.WriteLine(
                $"{finding.CorrelationType} | " +
                $"Confidence={finding.Confidence} | " +
                $"Evidence=[{string.Join(", ", finding.EvidenceIds)}]");

            Console.WriteLine($"Summary: {finding.Summary}");
        }

        Console.WriteLine("=== END CORRELATION FINDINGS ===");

        Assert.Equal(
            4,
            findings.Count);

        Assert.Contains(
            findings,
            finding =>
                finding.CorrelationType ==
                "Replication + RPC");

        Assert.Contains(
            findings,
            finding =>
                finding.CorrelationType ==
                "Replication + Infrastructure");

        Assert.Contains(
            findings,
            finding =>
                finding.CorrelationType ==
                "Replication + Patch Baseline");

        var partnerPatchFinding =
            Assert.Single(
                findings,
                finding =>
                    finding.CorrelationType ==
                    "Replication Partner + Patch Baseline");

        Assert.Contains(
            "EV-000001",
            partnerPatchFinding.EvidenceIds);

        Assert.Contains(
            "EV-000003",
            partnerPatchFinding.EvidenceIds);

        Assert.Contains(
            "EV-000002",
            partnerPatchFinding.EvidenceIds);

        Assert.Contains(
            "EV-000004",
            partnerPatchFinding.EvidenceIds);

        Assert.Equal(
            0.75,
            partnerPatchFinding.Confidence);

        // Assert - RCA

        Assert.Contains(
            "Patch baseline drift",
            rca.RootCause,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "probable contributing factor",
            rca.RootCause,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "0.75",
            rca.RootCause);

        Assert.Contains(
            "EV-000001",
            rca.RootCause);

        Assert.Contains(
            "EV-000002",
            rca.RootCause);

        Assert.Contains(
            "EV-000003",
            rca.RootCause);

        Assert.Contains(
            "EV-000004",
            rca.RootCause);

        Assert.Contains(
            "replication",
            rca.TechnicalImpact!,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "RPC",
            rca.CorrectiveActions!,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "patch baseline",
            rca.PreventiveActions!,
            StringComparison.OrdinalIgnoreCase);

        // Assert - recommendations

        Assert.Equal(
            5,
            recommendations.Count);

        Assert.Contains(
            recommendations,
            recommendation =>
                recommendation.Title ==
                "Align domain controller patch baselines");

        Assert.Contains(
            recommendations,
            recommendation =>
                recommendation.Title ==
                "Validate RPC connectivity");

        Assert.Contains(
            recommendations,
            recommendation =>
                recommendation.Title ==
                "Validate domain controller health");

        Assert.Contains(
            recommendations,
            recommendation =>
                recommendation.Title ==
                "Perform post-maintenance replication validation");

        Assert.Contains(
            recommendations,
            recommendation =>
                recommendation.Title ==
                "Implement automated patch compliance monitoring");

        // Assert - recommendation linkage

        Assert.All(
            recommendations,
            recommendation =>
                Assert.Equal(
                    rca.Id,
                    recommendation.RootCauseAnalysisId));
    }

    private sealed class FakeReplicationCollector
        : IReplicationCollector
    {
        private readonly DateTimeOffset _collectedUtc;

        public FakeReplicationCollector(
            DateTimeOffset collectedUtc)
        {
            _collectedUtc = collectedUtc;
        }

        public Task<IReadOnlyCollection<ReplicationRecord>>
            CollectAsync(
                CollectorContext context,
                CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<ReplicationRecord> records =
            [
                new()
                {
                    SourceDomainController = "SFOFLEX-DC1",
                    PartnerDomainController = "ZUSW-DC1",
                    SourceSite = "SFO",
                    PartnerSite = "ZUSW",
                    Success = false,
                    ErrorCode = 1722,
                    ErrorMessage = "RPC server unavailable",
                    CollectedUtc = _collectedUtc
                },

                new()
                {
                    SourceDomainController = "ZUSW-DC1",
                    PartnerDomainController = "SFOFLEX-DC1",
                    SourceSite = "ZUSW",
                    PartnerSite = "SFO",
                    Success = true,
                    LastSuccessfulReplicationUtc =
                        _collectedUtc.AddMinutes(-5),
                    CollectedUtc = _collectedUtc
                }
            ];

            return Task.FromResult(records);
        }
    }

    private sealed class FakePatchCollector
        : IPatchCollector
    {
        private readonly DateTimeOffset _collectedUtc;

        public FakePatchCollector(
            DateTimeOffset collectedUtc)
        {
            _collectedUtc = collectedUtc;
        }

        public Task<IReadOnlyCollection<PatchRecord>>
            CollectAsync(
                CollectorContext context,
                CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<PatchRecord> records =
            [
                new()
                {
                    DomainController = "SFOFLEX-DC1",
                    Site = "SFO",
                    OperatingSystem = "Windows Server 2019",
                    OsBuild = "17763",
                    KnowledgeBaseArticle = "KB5060531",
                    PatchVersion = "June 2026 CU",
                    Installed = false,
                    CollectedUtc = _collectedUtc
                },

                new()
                {
                    DomainController = "ZUSW-DC1",
                    Site = "ZUSW",
                    OperatingSystem = "Windows Server 2019",
                    OsBuild = "17763",
                    KnowledgeBaseArticle = "KB5060531",
                    PatchVersion = "June 2026 CU",
                    Installed = true,
                    InstalledUtc =
                        _collectedUtc.AddDays(-2),
                    CollectedUtc = _collectedUtc
                }
            ];

            return Task.FromResult(records);
        }
    }

    private sealed class FakeRpcCollector
    : IRpcCollector
{
    private readonly DateTimeOffset _collectedUtc;

    public FakeRpcCollector(
        DateTimeOffset collectedUtc)
    {
        _collectedUtc = collectedUtc;
    }

    public Task<IReadOnlyCollection<RpcRecord>>
        CollectAsync(
            CollectorContext context,
            TopologyContext topology,
            CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<RpcRecord> records =
        [
            new()
            {
                DomainController = "SFOFLEX-DC1",
                Target = "ZUSW-DC1",
                Success = false,
                ErrorCode = 1722,
                ErrorMessage = "RPC server unavailable",
                CollectedUtc = _collectedUtc
            }
        ];

        return Task.FromResult(records);
    }
}
    
    private sealed class FakeSystemInfoCollector
        : ISystemInfoCollector
    {
        private readonly DateTimeOffset _collectedUtc;

        public FakeSystemInfoCollector(
            DateTimeOffset collectedUtc)
        {
            _collectedUtc = collectedUtc;
        }

        public Task<IReadOnlyCollection<SystemInfoRecord>>
            CollectAsync(
                CollectorContext context,
                CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<SystemInfoRecord> records =
            [
                new()
                {
                    DomainController = "SFOFLEX-DC1",
                    Site = "SFO",
                    ComputerName = "SFOFLEX-DC1",
                    OperatingSystem = "Windows Server 2019",
                    OsVersion = "10.0",
                    BuildNumber = "17763",
                    Edition = "Standard",
                    Architecture = "x64",
                    TimeZone = "UTC",
                    LogicalProcessors = 8,
                    PhysicalMemoryGb = 32,
                    SystemDriveFreeSpaceGb = 2.1,
                    VirtualMachine = true,
                    Hypervisor = "Azure",
                    PowerShellVersion = "7.5",
                    DotNetVersion = "10.0",
                    CollectedUtc = _collectedUtc
                },

                new()
                {
                    DomainController = "ZUSW-DC1",
                    Site = "ZUSW",
                    ComputerName = "ZUSW-DC1",
                    OperatingSystem = "Windows Server 2019",
                    OsVersion = "10.0",
                    BuildNumber = "17763",
                    Edition = "Standard",
                    Architecture = "x64",
                    TimeZone = "UTC",
                    LogicalProcessors = 8,
                    PhysicalMemoryGb = 32,
                    SystemDriveFreeSpaceGb = 80,
                    VirtualMachine = true,
                    Hypervisor = "Azure",
                    PowerShellVersion = "7.5",
                    DotNetVersion = "10.0",
                    CollectedUtc = _collectedUtc
                }
            ];

            return Task.FromResult(records);
        }
    }
}