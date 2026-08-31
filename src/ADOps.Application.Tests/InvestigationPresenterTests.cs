using ADOps.Application.Presentation;
using ADOps.Core.Entities;
using ADOps.Core.Enums;

namespace ADOps.Application.Tests;

public sealed class InvestigationPresenterTests
{
    [Fact]
    public void Build_MapsInvestigationEvidenceCorrelationsRcaAndRecommendations()
    {
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

        var evidence =
            new List<Evidence>
            {
                new()
                {
                    EvidenceId = "EV-000001",
                    InvestigationId =
                        investigation.Id.ToString(),
                    Type =
                        EvidenceType.ReplicationFailure,
                    Source = "SFOFLEX-DC1",
                    Target = "ZUSW-DC1",
                    CollectedUtc = collectedUtc,
                    Summary =
                        "Replication failed from SFOFLEX-DC1 to ZUSW-DC1.",
                    IsValid = true
                }
            };

        var findings =
            new List<CorrelatedFinding>
            {
                new()
                {
                    CorrelationId = "COR-001",
                    InvestigationId =
                        investigation.Id.ToString(),
                    CorrelationType =
                        "Replication + Patch Baseline",
                    Summary =
                        "Replication failure correlates with patch baseline drift.",
                    EvidenceIds =
                        ["EV-000001", "EV-000003"],
                    CreatedUtc = collectedUtc,
                    Confidence = 0.75
                }
            };

        var rootCauseAnalysis =
            new RootCauseAnalysis
            {
                Title =
                    "Active Directory Incident Root Cause Analysis",

                ExecutiveSummary =
                    "Patch baseline drift is the probable contributing factor.",

                RootCause =
                    "Patch baseline drift is the probable contributing factor."
            };

        rootCauseAnalysis.SetPrimaryFinding(
            "COR-001",
            0.75,
            ["EV-000001", "EV-000003"]);

        rootCauseAnalysis.SetBusinessImpact(
            "SFO Active Directory replication failure affected production operations.");

        rootCauseAnalysis.SetTechnicalImpact(
            "Replication and RPC communication failures were observed between domain controllers.");

        var recommendations =
            new List<Recommendation>
            {
                new(
                    "Align domain controller patch baselines",
                    "Review and align the patch baseline across affected domain controllers.",
                    RecommendationPriority.High)
                {
                    RootCauseAnalysisId = rootCauseAnalysis.Id
                }
            };

        var presenter =
            new InvestigationPresenter();

        var report =
            presenter.Build(
                investigation,
                evidence,
                findings,
                rootCauseAnalysis,
                recommendations);

        Assert.Equal(
            investigation.Id.ToString(),
            report.InvestigationId);

        Assert.Equal(
            "INC-SFO-20260709",
            report.IncidentNumber);

        Assert.NotEqual(
            default,
            report.GeneratedUtc);

        var evidenceItem =
            Assert.Single(report.Evidence);

        Assert.Equal(
            "ReplicationFailure",
            evidenceItem.Type);

        Assert.Equal(
            "SFOFLEX-DC1",
            evidenceItem.Source);

        Assert.Equal(
            "ZUSW-DC1",
            evidenceItem.Target);

        Assert.True(
            evidenceItem.IsValid);

        var correlationItem =
            Assert.Single(report.Correlations);

        Assert.Equal(
            "Replication + Patch Baseline",
            correlationItem.Type);

        Assert.Equal(
            0.75,
            correlationItem.Confidence);

        Assert.Equal(
            "Patch baseline drift is the probable contributing factor.",
            report.ExecutiveSummary.Summary);

        Assert.Equal(
            "SFO Active Directory replication failure affected production operations.",
            report.ExecutiveSummary.BusinessImpact);

        Assert.Equal(
            "Replication and RPC communication failures were observed between domain controllers.",
            report.ExecutiveSummary.TechnicalImpact);

        Assert.Equal(
            "Patch baseline drift is the probable contributing factor.",
            report.RootCause);

        var recommendationItem =
            Assert.Single(report.Recommendations);

        Assert.Equal(
            "Align domain controller patch baselines",
            recommendationItem.Action);

        Assert.Equal(
            "High",
            recommendationItem.Priority);

        Assert.Equal(
            0.75,
            report.OverallConfidence);

        Assert.Equal(
            "EV-000001",
            evidenceItem.EvidenceId);

        Assert.Equal(
            collectedUtc,
            evidenceItem.CollectedUtc);

        Assert.Equal(
            ["EV-000001", "EV-000003"],
            correlationItem.EvidenceIds);

        var replicationEvidence =
            Assert.Single(
                report.Evidence,
                evidence =>
                evidence.Type == "ReplicationFailure");

            Assert.Equal(
                "EV-000001",
                replicationEvidence.EvidenceId);

            Assert.Equal(
                "SFOFLEX-DC1",
                replicationEvidence.Source);

            Assert.Equal(
                "ZUSW-DC1",
                replicationEvidence.Target);

            Assert.Equal(
                collectedUtc,
                replicationEvidence.CollectedUtc);

            Assert.Equal(
                "Replication failed from SFOFLEX-DC1 to ZUSW-DC1.",
                replicationEvidence.Summary);
    }

    [Fact]
    public void Build_UsesRcaConfidenceWhenAvailable()
    {
        var collectedUtc =
            new DateTimeOffset(
                2026,
                7,
                9,
                12,
                0,
                0,
                TimeSpan.Zero);

        var investigation =
            CreateInvestigation(collectedUtc);

        var findings =
            new List<CorrelatedFinding>
            {
                CreateFinding("COR-001", 0.60),
                CreateFinding("COR-002", 0.80)
            };

        var rootCauseAnalysis =
            CreateRootCauseAnalysis();

        rootCauseAnalysis.SetPrimaryFinding(
            "COR-001",
            0.95,
            ["EV-000001"]);

        var presenter =
            new InvestigationPresenter();

        var report =
            presenter.Build(
                investigation,
                [],
                findings,
                rootCauseAnalysis,
                []);

        Assert.Equal(
            0.95,
            report.OverallConfidence);
    }

    [Fact]
    public void Build_CalculatesAverageFindingConfidenceWhenRcaConfidenceIsUnavailable()
    {
        var collectedUtc =
            new DateTimeOffset(
                2026,
                7,
                9,
                12,
                0,
                0,
                TimeSpan.Zero);

        var investigation =
            CreateInvestigation(collectedUtc);

        var findings =
            new List<CorrelatedFinding>
            {
                CreateFinding("COR-001", 0.60),
                CreateFinding("COR-002", 0.80)
            };

        var rootCauseAnalysis =
            CreateRootCauseAnalysis();

        var presenter =
            new InvestigationPresenter();

        var report =
            presenter.Build(
                investigation,
                [],
                findings,
                rootCauseAnalysis,
                []);

        Assert.Equal(
            0.70,
            report.OverallConfidence);
    }

    [Fact]
    public void Build_ReturnsZeroConfidenceWhenNoFindingsExistAndRcaConfidenceIsUnavailable()
    {
        var collectedUtc =
            new DateTimeOffset(
                2026,
                7,
                9,
                12,
                0,
                0,
                TimeSpan.Zero);

        var investigation =
            CreateInvestigation(collectedUtc);

        var rootCauseAnalysis =
            CreateRootCauseAnalysis();

        var presenter =
            new InvestigationPresenter();

        var report =
            presenter.Build(
                investigation,
                [],
                [],
                rootCauseAnalysis,
                []);

        Assert.Equal(
            0,
            report.OverallConfidence);
    }

    private static ADOps.Core.Entities.Investigation CreateInvestigation(
        DateTimeOffset collectedUtc)
    {
        var incident =
            new Incident
            {
                IncidentNumber = "INC-SFO-20260709",
                Title = "AD replication failure",
                Environment = "Production",
                SiteCode = "SFO",
                DetectedUtc = collectedUtc
            };

        return new ADOps.Core.Entities.Investigation
        {
            InvestigationNumber = "INV-SFO-20260709",
            Incident = incident,
            StartedUtc = collectedUtc
        };
    }

    private static CorrelatedFinding CreateFinding(
        string correlationId,
        double confidence)
    {
        return new CorrelatedFinding
        {
            CorrelationId = correlationId,
            InvestigationId = "INC-SFO-20260709",
            CorrelationType = "Test",
            Summary = "Test correlation",
            EvidenceIds = ["EV-000001"],
            CreatedUtc =
                new DateTimeOffset(
                    2026,
                    7,
                    9,
                    12,
                    0,
                    0,
                    TimeSpan.Zero),
            Confidence = confidence
        };
    }

    private static RootCauseAnalysis CreateRootCauseAnalysis()
    {
        return new RootCauseAnalysis
        {
            Title =
                "Active Directory Incident Root Cause Analysis",

            ExecutiveSummary =
                "Test root cause analysis.",

            RootCause =
                "Test root cause."
        };
    }

}
