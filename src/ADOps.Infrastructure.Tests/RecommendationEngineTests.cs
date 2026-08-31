using ADOps.Core.Entities;
using ADOps.Core.Enums;
using ADOps.Infrastructure.Recommendations;

namespace ADOps.Infrastructure.Tests;

public sealed class RecommendationEngineTests
{
    [Fact]
    public void Generate_ThrowsWhenAnalysisIsNull()
    {
        var engine = new RecommendationEngine();

        Assert.Throws<ArgumentNullException>(
            () => engine.Generate(
                null!,
                Array.Empty<CorrelatedFinding>()));
    }

    [Fact]
    public void Generate_ThrowsWhenFindingsAreNull()
    {
        var engine = new RecommendationEngine();

        var analysis = CreateAnalysis();

        Assert.Throws<ArgumentNullException>(
            () => engine.Generate(
                analysis,
                null!));
    }

    [Fact]
    public void Generate_ReturnsEmptyWhenNoFindingsExist()
    {
        var engine = new RecommendationEngine();

        var analysis = CreateAnalysis();

        var recommendations =
            engine.Generate(
                analysis,
                Array.Empty<CorrelatedFinding>());

        Assert.Empty(recommendations);
    }

    [Fact]
    public void Generate_CreatesPatchRecommendationsWithReplication()
    {
        var engine = new RecommendationEngine();

        var analysis = CreateAnalysis();

        var findings =
            new[]
            {
                CreateFinding(
                    "Replication + Patch Baseline")
            };

        var recommendations =
            engine.Generate(
                analysis,
                findings);

        Assert.Equal(3, recommendations.Count);

        Assert.Contains(
            recommendations,
            x =>
                x.Title ==
                "Align domain controller patch baselines" &&
                x.Priority ==
                RecommendationPriority.High);

        Assert.Contains(
            recommendations,
            r => r.Title == "Perform post-maintenance replication validation");
        

        Assert.Contains(
            recommendations,
            x =>
                x.Title ==
                "Implement automated patch compliance monitoring" &&
                x.Priority ==
                RecommendationPriority.Medium);
    }

    [Fact]
    public void Generate_CreatesRpcRecommendationWithReplication()
    {
        var engine = new RecommendationEngine();

        var analysis = CreateAnalysis();

        var findings =
            new[]
            {
                CreateFinding(
                    "Replication + RPC")
            };

        var recommendations =
            engine.Generate(
                analysis,
                findings);

        Assert.Equal(2, recommendations.Count);

        var rpcRecommendation =
            Assert.Single(
                recommendations,
                x =>
                    x.Title ==
                    "Validate RPC connectivity");

        Assert.Equal(
            RecommendationPriority.High,
            rpcRecommendation.Priority);

        Assert.Contains(
            recommendations,
            x =>
                x.Title ==
                "Perform post-maintenance replication validation" &&
                x.Priority ==
                RecommendationPriority.Medium);
    }

    [Fact]
    public void Generate_CreatesInfrastructureRecommendationWithReplication()
    {
        var engine = new RecommendationEngine();

        var analysis = CreateAnalysis();

        var findings =
            new[]
            {
                CreateFinding(
                    "Replication + Infrastructure")
            };

        var recommendations =
            engine.Generate(
                analysis,
                findings);

        Assert.Equal(2, recommendations.Count);

        var healthRecommendation =
            Assert.Single(
                recommendations,
                x =>
                    x.Title ==
                    "Validate domain controller health");

        Assert.Equal(
            RecommendationPriority.Medium,
            healthRecommendation.Priority);

        Assert.Contains(
            recommendations,
            x =>
                x.Title ==
                "Perform post-maintenance replication validation");
    }

    [Fact]
    public void Generate_CreatesReplicationValidationForReplicationFinding()
    {
        var engine = new RecommendationEngine();

        var analysis = CreateAnalysis();

        var findings =
            new[]
            {
                CreateFinding(
                    "Replication + RPC")
            };

        var recommendations =
            engine.Generate(
                analysis,
                findings);

        Assert.Contains(
            recommendations,
            x =>
                x.Title ==
                "Perform post-maintenance replication validation" &&
                x.Priority ==
                RecommendationPriority.Medium);
    }

    [Fact]
    public void Generate_CreatesPatchComplianceMonitoringForPatchFindingWithoutReplication()
    {
        var engine = new RecommendationEngine();

        var analysis = CreateAnalysis();

        var findings =
            new[]
            {
                CreateFinding(
                    "Patch Baseline")
            };

        var recommendations =
            engine.Generate(
                analysis,
                findings);

        var recommendation =
            Assert.Single(recommendations);

        Assert.Equal(
            "Implement automated patch compliance monitoring",
            recommendation.Title);

        Assert.Equal(
            RecommendationPriority.Medium,
            recommendation.Priority);
    }

    [Fact]
    public void Generate_DoesNotCreateReplicationSpecificRecommendationsWithoutReplication()
    {
        var engine = new RecommendationEngine();

        var analysis = CreateAnalysis();

        var findings =
            new[]
            {
                CreateFinding(
                    "RPC"),
                CreateFinding(
                    "Infrastructure")
            };

        var recommendations =
            engine.Generate(
                analysis,
                findings);

        Assert.Empty(recommendations);
    }

    [Fact]
    public void Generate_CreatesFullRecommendationSetForIncident()
    {
        var engine = new RecommendationEngine();

        var analysis = CreateAnalysis();

        var findings =
            new[]
            {
                CreateFinding(
                    "Replication + Patch Baseline"),

                CreateFinding(
                    "Replication + RPC"),

                CreateFinding(
                    "Replication + Infrastructure")
            };

        var recommendations =
            engine.Generate(
                analysis,
                findings);

        Assert.Equal(5, recommendations.Count);

        Assert.Contains(
            recommendations,
            x =>
                x.Title ==
                "Align domain controller patch baselines" &&
                x.Priority ==
                RecommendationPriority.High);

        Assert.Contains(
            recommendations,
            x =>
                x.Title ==
                "Validate RPC connectivity" &&
                x.Priority ==
                RecommendationPriority.High);

        Assert.Contains(
            recommendations,
            x =>
                x.Title ==
                "Validate domain controller health" &&
                x.Priority ==
                RecommendationPriority.Medium);

        Assert.Contains(
            recommendations,
            x =>
                x.Title ==
                "Perform post-maintenance replication validation" &&
                x.Priority ==
                RecommendationPriority.Medium);

        Assert.Contains(
            recommendations,
            x =>
                x.Title ==
                "Implement automated patch compliance monitoring" &&
                x.Priority ==
                RecommendationPriority.Medium);
    }

    [Fact]
    public void Generate_AssociatesRecommendationsWithRootCauseAnalysis()
    {
        var engine = new RecommendationEngine();

        var analysis = CreateAnalysis();

        var findings =
            new[]
            {
                CreateFinding(
                    "Replication + RPC")
            };

        var recommendations =
            engine.Generate(
                analysis,
                findings);

        Assert.NotEmpty(recommendations);

        Assert.All(
            recommendations,
            recommendation =>
                Assert.Equal(
                    analysis.Id,
                    recommendation.RootCauseAnalysisId));
    }

    private static RootCauseAnalysis CreateAnalysis()
    {
        return new RootCauseAnalysis
        {
            Title =
                "Active Directory Incident Root Cause Analysis",

            ExecutiveSummary =
                "Test analysis.",

            RootCause =
                "Test root cause."
        };
    }

    private static CorrelatedFinding CreateFinding(
        string correlationType)
    {
        return new CorrelatedFinding
        {
            CorrelationId =
                $"COR-{Guid.NewGuid():N}",

            InvestigationId =
                "INC-SFO-20260709",

            Summary =
                $"Test finding: {correlationType}",

            EvidenceIds =
                ["E-001", "E-002"],

            CorrelationType =
                correlationType,

            CreatedUtc =
                DateTimeOffset.UtcNow,

            Confidence =
                0.80
        };
    }
}
