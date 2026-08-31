using ADOps.Core.Entities;
using ADOps.Infrastructure.Analysis;

namespace ADOps.Infrastructure.Tests;

public sealed class RootCauseAnalyzerTests
{
    [Fact]
    public void Analyze_ThrowsWhenFindingsAreEmpty()
    {
        var analyzer = new RootCauseAnalyzer();

        var exception =
            Assert.Throws<InvalidOperationException>(
                () => analyzer.Analyze(
                    Array.Empty<CorrelatedFinding>()));

        Assert.Equal(
            "Cannot generate RCA without findings.",
            exception.Message);
    }

    [Fact]
    public void Analyze_IdentifiesPartnerPatchDriftAsPrimaryContributingFactor()
    {
        var analyzer = new RootCauseAnalyzer();

        var finding =
            CreateFinding(
                correlationType:
                    "Replication Partner + Patch Baseline",
                summary:
                    "Replication failure on SFOFLEX-DC1 correlates with healthy replication on partner ZUSW-DC1 and a patch baseline difference.",
                confidence:
                    0.75,
                evidenceIds:
                    ["E-001", "E-005", "E-004", "E-006"]);

        var rca =
            analyzer.Analyze([finding]);

        Assert.Equal(
            "Active Directory Incident Root Cause Analysis",
            rca.Title);

        Assert.Contains(
            "patch baseline drift",
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
            "E-001",
            rca.RootCause);

        Assert.Contains(
            "1 correlated findings",
            rca.ExecutiveSummary,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Analyze_AssociatesPrimaryFindingWithPartnerPatchCorrelation()
    {
        var analyzer = new RootCauseAnalyzer();

        var finding =
            CreateFinding(
                correlationType:
                    "Replication Partner + Patch Baseline",
                summary:
                    "Replication failure correlates with healthy replication " +
                    "on the partner and a patch baseline difference.",
                confidence:
                    0.75,
                evidenceIds:
                    ["E-001", "E-002", "E-003", "E-004"]);

        var rca =
            analyzer.Analyze([finding]);

        Assert.Equal(
            finding.CorrelationId,
            rca.PrimaryFindingId);

        Assert.Equal(
            0.75,
            rca.Confidence);

        Assert.Equal(
            finding.EvidenceIds,
            rca.SupportingEvidenceIds);
    }
    
    [Fact]
    public void Analyze_IncludesRpcFindingInSummaryAndCorrectiveActions()
    {
        var analyzer = new RootCauseAnalyzer();

        var finding =
            CreateFinding(
                correlationType:
                    "Replication + RPC",
                summary:
                    "Replication failure correlates with an RPC communication failure.",
                confidence:
                    0.85);

        var rca =
            analyzer.Analyze([finding]);

        Assert.Contains(
            "RPC communication failure",
            rca.ExecutiveSummary,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "RPC connectivity",
            rca.CorrectiveActions,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Analyze_IncludesInfrastructureFindingInSummaryAndActions()
    {
        var analyzer = new RootCauseAnalyzer();

        var finding =
            CreateFinding(
                correlationType:
                    "Replication + Infrastructure",
                summary:
                    "Replication issue correlates with an infrastructure health issue.",
                confidence:
                    0.80);

        var rca =
            analyzer.Analyze([finding]);

        Assert.Contains(
            "Infrastructure health evidence",
            rca.ExecutiveSummary,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "disk space",
            rca.CorrectiveActions,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "infrastructure health thresholds",
            rca.PreventiveActions,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Analyze_IncludesReplicationImpactAndValidationAction()
    {
        var analyzer = new RootCauseAnalyzer();

        var finding =
            CreateFinding(
                correlationType:
                    "Replication + RPC",
                summary:
                    "Replication failure correlates with RPC failure.",
                confidence:
                    0.85);

        var rca =
            analyzer.Analyze([finding]);

        Assert.Contains(
            "replication health was impacted",
            rca.TechnicalImpact,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "replication validation",
            rca.CorrectiveActions,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Analyze_UsesPatchFindingWhenPartnerCorrelationIsUnavailable()
    {
        var analyzer = new RootCauseAnalyzer();

        var finding =
            CreateFinding(
                correlationType:
                    "Replication + Patch Baseline",
                summary:
                    "Replication failure correlates with patch baseline drift.",
                confidence:
                    0.75);

        var rca =
            analyzer.Analyze([finding]);

        Assert.Contains(
            "Patch baseline drift on the affected domain controller",
            rca.RootCause,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "0.75",
            rca.RootCause);

        Assert.Contains(
            "patch baseline compliance monitoring",
            rca.PreventiveActions,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Analyze_AssociatesPrimaryFindingWithPatchCorrelationWhenPartnerCorrelationIsUnavailable()
    {
        var analyzer = new RootCauseAnalyzer();

        var finding =
            CreateFinding(
                correlationType:
                    "Replication + Patch Baseline",
                summary:
                    "Replication failure correlates with patch baseline drift.",
                confidence:
                    0.70,
                evidenceIds:
                    ["E-001", "E-003"]);

        var rca =
            analyzer.Analyze([finding]);

        Assert.Equal(
            finding.CorrelationId,
            rca.PrimaryFindingId);

        Assert.Equal(
            0.70,
            rca.Confidence);

        Assert.Equal(
            finding.EvidenceIds,
            rca.SupportingEvidenceIds);
    }
    
    [Fact]
    public void Analyze_ReportsInsufficientEvidenceWithoutPatchCorrelation()
    {
        var analyzer = new RootCauseAnalyzer();

        var finding =
            CreateFinding(
                correlationType:
                    "Replication + RPC",
                summary:
                    "Replication failure correlates with RPC failure.",
                confidence:
                    0.85);

        var rca =
            analyzer.Analyze([finding]);

        Assert.Contains(
            "insufficient",
            rca.RootCause,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "probable primary contributing factor",
            rca.RootCause,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Analyze_UsesHighestConfidencePartnerPatchFinding()
    {
        var analyzer = new RootCauseAnalyzer();

        var lowerConfidence =
            CreateFinding(
                correlationType:
                    "Replication Partner + Patch Baseline",
                summary:
                    "Lower confidence finding.",
                confidence:
                    0.60,
                evidenceIds:
                    ["E-LOW"]);

        var higherConfidence =
            CreateFinding(
                correlationType:
                    "Replication Partner + Patch Baseline",
                summary:
                    "Higher confidence finding.",
                confidence:
                    0.90,
                evidenceIds:
                    ["E-HIGH"]);

        var rca =
            analyzer.Analyze(
                [
                    lowerConfidence,
                    higherConfidence
                ]);

        Assert.Contains(
            "0.90",
            rca.RootCause);

        Assert.Contains(
            "E-HIGH",
            rca.RootCause);

        Assert.DoesNotContain(
            "E-LOW",
            rca.RootCause);
    }

    [Fact]
    public void Analyze_CombinesMultipleFindingTypesIntoOneRca()
    {
        var analyzer = new RootCauseAnalyzer();

        var findings =
            new[]
            {
                CreateFinding(
                    "Replication + RPC",
                    "RPC failure.",
                    0.85),

                CreateFinding(
                    "Replication + Infrastructure",
                    "Infrastructure issue.",
                    0.80),

                CreateFinding(
                    "Replication + Patch Baseline",
                    "Patch drift.",
                    0.75)
            };

        var rca =
            analyzer.Analyze(findings);

        Assert.Contains(
            "3 correlated findings",
            rca.ExecutiveSummary);

        Assert.Contains(
            "RPC communication failure",
            rca.ExecutiveSummary,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "Infrastructure health evidence",
            rca.ExecutiveSummary,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "patch baseline",
            rca.CorrectiveActions,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "RPC connectivity",
            rca.CorrectiveActions,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "disk space",
            rca.CorrectiveActions,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "replication validation",
            rca.CorrectiveActions,
            StringComparison.OrdinalIgnoreCase);
    }

    private static CorrelatedFinding CreateFinding(
        string correlationType,
        string summary,
        double confidence,
        IReadOnlyCollection<string>? evidenceIds = null)
    {
        return new CorrelatedFinding
        {
            CorrelationId =
                $"COR-{Guid.NewGuid():N}",

            InvestigationId =
                "INC-SFO-20260709",

            Summary =
                summary,

            EvidenceIds =
                evidenceIds ??
                ["E-001", "E-002"],

            CorrelationType =
                correlationType,

            CreatedUtc =
                DateTimeOffset.UtcNow,

            Confidence =
                confidence
        };
    }
}
