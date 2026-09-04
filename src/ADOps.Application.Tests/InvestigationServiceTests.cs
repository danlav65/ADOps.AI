using ADOps.Application.Investigation;
using ADOps.Application.Presentation;
using ADOps.Core.Entities;
using ADOps.Core.Entities.Replication;
using ADOps.Core.Interfaces;
using ADOps.Infrastructure.Analysis;
using ADOps.Infrastructure.Collectors;
using ADOps.Infrastructure.Correlation;
using ADOps.Infrastructure.Evidence;
using ADOps.Infrastructure.Investigation;
using ADOps.Infrastructure.Recommendations;

namespace ADOps.Application.Tests;

public sealed class InvestigationServiceTests
{
    [Fact]
    public async Task InvestigateAsync_ProducesCompleteInvestigationReport()
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

        var context =
            new CollectorContext
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

        var incident =
            new Incident
            {
                IncidentNumber = "INC-SFO-20260709",
                Title = "AD replication failure",
                Environment = "Production",
                SiteCode = "SFO",
                DetectedUtc = collectedUtc
            };

        var investigation =
            new ADOps.Core.Entities.Investigation
            {
                InvestigationNumber = "INV-SFO-20260709",
                Incident = incident,
                StartedUtc = collectedUtc
            };

        var snapshotBuilder =
            new InvestigationSnapshotBuilder(
                new FakeReplicationCollector(collectedUtc),
                new FakePatchCollector(collectedUtc),
                new FakeSystemInfoCollector(collectedUtc),
                new FakeRpcCollector(collectedUtc),
                new EvidenceNormalizer(
                    new EvidenceIdGenerator()));

        var service =
            new InvestigationService(
                snapshotBuilder,
                new CorrelationEngine(),
                new RootCauseAnalyzer(),
                new RecommendationEngine(),
                new InvestigationPresenter());

        // Act

        var report =
            await service.InvestigateAsync(
                investigation,
                context);

        // Assert - report identity

        Assert.Equal(
            investigation.Id.ToString(),
            report.InvestigationId);

        Assert.Equal(
            "INC-SFO-20260709",
            report.IncidentNumber);

        Assert.NotEqual(
            default,
            report.GeneratedUtc);

        // Assert - evidence

        Assert.Equal(
            7,
            report.Evidence.Count);

        Assert.Contains(
            report.Evidence,
            evidence =>
                evidence.Type == "ReplicationFailure");

        Assert.Contains(
            report.Evidence,
            evidence =>
                evidence.Type == "RpcFailure");

        // Assert - correlations

        Assert.Equal(
            4,
            report.Correlations.Count);

        Assert.Contains(
            report.Correlations,
            correlation =>
                correlation.Type ==
                "Replication + RPC");

        Assert.Contains(
            report.Correlations,
            correlation =>
                correlation.Type ==
                "Replication + Infrastructure");

        Assert.Contains(
            report.Correlations,
            correlation =>
                correlation.Type ==
                "Replication + Patch Baseline");

        // Assert - RCA

        Assert.NotNull(report.RootCause);

        Assert.Contains(
            "Patch baseline drift",
            report.RootCause,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
        "Analysis identified 4 correlated findings",
        report.ExecutiveSummary.Summary,
        StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "patch baseline drift",
            report.ExecutiveSummary.Summary,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "RPC communication failure",
            report.ExecutiveSummary.Summary,
            StringComparison.OrdinalIgnoreCase);

        Assert.Equal(
            "Incident INC-SFO-20260709 occurred at site SFO.",
            report.ExecutiveSummary.BusinessImpact);

        Assert.Equal(
            "Active Directory replication health was impacted on the affected domain controller.",
            report.ExecutiveSummary.TechnicalImpact);

        // Assert - recommendations

        Assert.Equal(
            5,
            report.Recommendations.Count);

        Assert.Contains(
            report.Recommendations,
            recommendation =>
                recommendation.Action ==
                "Align domain controller patch baselines");

        Assert.Contains(
            report.Recommendations,
            recommendation =>
                recommendation.Action ==
                "Validate RPC connectivity");

        Assert.Contains(
            report.Recommendations,
            recommendation =>
                recommendation.Action ==
                "Validate domain controller health");

        Assert.Contains(
            report.Recommendations,
            recommendation =>
                recommendation.Action ==
                "Perform post-maintenance replication validation");

        Assert.Contains(
            report.Recommendations,
            recommendation =>
                recommendation.Action ==
                "Implement automated patch compliance monitoring");

        // Assert - overall confidence

        Assert.Equal(
            0.75,
            report.OverallConfidence);
    }

    [Fact]
    public async Task InvestigateAsync_FromSnapshot_ProducesCompleteInvestigationReport()
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

        var incident =
            new Incident
            {
                IncidentNumber = "INC-SFO-20260709",
                Title = "AD replication failure",
                Environment = "Production",
                SiteCode = "SFO",
                DetectedUtc = collectedUtc
            };

        var investigation =
            new ADOps.Core.Entities.Investigation
            {
                InvestigationNumber = "INV-SFO-20260709",
                Incident = incident,
                StartedUtc = collectedUtc
            };

        var snapshot =
            new InvestigationSnapshot
            {
                InvestigationId =
                    "INC-SFO-20260709",

                StartedUtc =
                    collectedUtc,

                CompletedUtc =
                    collectedUtc.AddMinutes(5),

                OperationalContext =
                    new OperationalContext
                    {
                        Target = "apcflex.aero",
                        Site = "SFO",
                        Environment = "Production"
                    },

                Topology =
                    new TopologyContext
                    {
                        ReplicationPartners =
                        [
                            new ReplicationPartnerRelationship
                            {
                                SourceDomainController =
                                    "SFOFLEX-DC1",

                                PartnerDomainController =
                                    "ZUSW-DC1",

                                SourceSite = "SFO",
                                PartnerSite = "ZUSW",
                                DiscoveredUtc = collectedUtc,
                                IsActive = true
                            }
                        ]
                    }
            };

        snapshot.Evidence.AddRange(
        [
            new Evidence
            {
                EvidenceId = "EV-000001",
                InvestigationId = "INC-SFO-20260709",
                Type = EvidenceType.ReplicationFailure,
                Source = "SFOFLEX-DC1",
                Target = "ZUSW-DC1",
                CollectedUtc = collectedUtc,
                Summary =
                    "Replication failed from SFOFLEX-DC1 to ZUSW-DC1."
            },

            new Evidence
            {
                EvidenceId = "EV-000002",
                InvestigationId = "INC-SFO-20260709",
                Type = EvidenceType.RpcFailure,
                Source = "SFOFLEX-DC1",
                Target = "ZUSW-DC1",
                CollectedUtc = collectedUtc,
                Summary =
                    "RPC connectivity failed from SFOFLEX-DC1 to ZUSW-DC1."
            },

            new Evidence
            {
                EvidenceId = "EV-000003",
                InvestigationId = "INC-SFO-20260709",
                Type = EvidenceType.Patch,
                Source = "ReportBundle",
                Target = "SFOFLEX-DC1",
                CollectedUtc = collectedUtc,
                Summary =
                    "Patch baseline is missing on SFOFLEX-DC1."
            },

            new Evidence
            {
                EvidenceId = "EV-000004",
                InvestigationId = "INC-SFO-20260709",
                Type = EvidenceType.Patch,
                Source = "ReportBundle",
                Target = "ZUSW-DC1",
                CollectedUtc = collectedUtc,
                Summary =
                    "Patch baseline is present on ZUSW-DC1."
            }
        ]);

        var service =
            new InvestigationService(
                new ThrowingSnapshotBuilder(),
                new CorrelationEngine(),
                new RootCauseAnalyzer(),
                new RecommendationEngine(),
                new InvestigationPresenter());

        // Act

        var report =
            await service.InvestigateAsync(
                investigation,
                snapshot);

        // Assert

        Assert.Equal(
            investigation.Id.ToString(),
            report.InvestigationId);

        Assert.Equal(
            "INC-SFO-20260709",
            report.IncidentNumber);

        Assert.Equal(
            4,
            report.Evidence.Count);

        Assert.Equal(
            2,
            report.Correlations.Count);

        Assert.Contains(
            report.Correlations,
            correlation =>
            correlation.Type ==
            "Replication + RPC");

        Assert.NotNull(
            report.RootCause);

        Assert.Contains(
            "Patch baseline drift",
            report.RootCause,
            StringComparison.OrdinalIgnoreCase);

        Assert.NotEmpty(
            report.Recommendations);

        Assert.Equal(
            0.75,
            report.OverallConfidence);
    }

    [Fact]
    public async Task InvestigateAsync_Throws_WhenInvestigationIsNull()
    {
        // Arrange

        var service =
            new InvestigationService(
                new ThrowingSnapshotBuilder(),
                new CorrelationEngine(),
                new RootCauseAnalyzer(),
                new RecommendationEngine(),
                new InvestigationPresenter());

        var context =
            new CollectorContext
            {
                InvestigationId = "INC-TEST",
                Site = "SFO",
                DomainName = "apcflex.aero",
                DomainControllers =
                [
                    "SFOFLEX-DC1"
                ]
            };

        // Act & Assert

        await Assert.ThrowsAsync<ArgumentNullException>(
            () =>
                service.InvestigateAsync(
                    null!,
                    context));
    }

    [Fact]
    public async Task InvestigateAsync_Throws_WhenContextIsNull()
    {
        // Arrange

        var service =
            new InvestigationService(
                new ThrowingSnapshotBuilder(),
                new CorrelationEngine(),
                new RootCauseAnalyzer(),
                new RecommendationEngine(),
                new InvestigationPresenter());

        var investigation =
            new ADOps.Core.Entities.Investigation
            {
                InvestigationNumber = "INV-TEST",
                Incident =
                    new Incident
                    {
                        IncidentNumber = "INC-TEST",
                        Title = "Test investigation",
                        Environment = "Production",
                        SiteCode = "SFO",
                        DetectedUtc = DateTimeOffset.UtcNow
                    }
            };

        // Act & Assert

        await Assert.ThrowsAsync<ArgumentNullException>(
            () =>
                service.InvestigateAsync(
                    investigation,
                    null!));
    }

    [Fact]
    public async Task InvestigateAsync_FromSnapshot_Throws_WhenTopologyIsMissing()
    {
        // Arrange

        var service =
            new InvestigationService(
                new ThrowingSnapshotBuilder(),
                new CorrelationEngine(),
                new RootCauseAnalyzer(),
                new RecommendationEngine(),
                new InvestigationPresenter());

        var investigation =
            new ADOps.Core.Entities.Investigation
            {
                InvestigationNumber = "INV-TEST",
                Incident =
                    new Incident
                    {
                        IncidentNumber = "INC-TEST",
                        Title = "Test investigation",
                        Environment = "Production",
                        SiteCode = "SFO",
                        DetectedUtc = DateTimeOffset.UtcNow
                    }
            };

        var snapshot =
            new InvestigationSnapshot
            {
                InvestigationId = "INC-TEST",
                StartedUtc = DateTimeOffset.UtcNow,
                CompletedUtc = DateTimeOffset.UtcNow,
                OperationalContext =
                    new OperationalContext
                    {
                        Target = "apcflex.aero",
                        Site = "SFO",
                        Environment = "Production"
                    }
            };

        // Act & Assert

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () =>
                    service.InvestigateAsync(
                        investigation,
                        snapshot));

        Assert.Equal(
            "Investigation snapshot does not contain topology information.",
            exception.Message);
    }
    [Fact]
    public async Task InvestigateAsync_PropagatesSnapshotBuilderFailure()
    {
        // Arrange

        var service =
            new InvestigationService(
                new ThrowingSnapshotBuilder(),
                new CorrelationEngine(),
                new RootCauseAnalyzer(),
                new RecommendationEngine(),
                new InvestigationPresenter());

        var investigation =
            new ADOps.Core.Entities.Investigation
            {
                InvestigationNumber = "INV-TEST",
                Incident =
                    new Incident
                    {
                        IncidentNumber = "INC-TEST",
                        Title = "Test investigation",
                        Environment = "Production",
                        SiteCode = "SFO",
                        DetectedUtc = DateTimeOffset.UtcNow
                    }
            };

        var context =
            new CollectorContext
            {
                InvestigationId = "INC-TEST",
                Site = "SFO",
                DomainName = "apcflex.aero",
                DomainControllers = ["SFOFLEX-DC1"]
            };

        // Act & Assert

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () =>
                    service.InvestigateAsync(
                        investigation,
                        context));

        Assert.Equal(
            "Snapshot builder should not be called for snapshot-based investigation.",
            exception.Message);
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

    private sealed class ThrowingSnapshotBuilder
        : IInvestigationSnapshotBuilder
    {
        public Task<InvestigationSnapshot> BuildAsync(
            CollectorContext context,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "Snapshot builder should not be called for snapshot-based investigation.");
        }
    }
}




