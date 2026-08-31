using ADOps.Core.Entities;
using ADOps.Core.Enums;
using ADOps.Core.Interfaces;

namespace ADOps.Infrastructure.Recommendations;

/// <summary>
/// Generates deterministic operational recommendations from investigation findings.
/// </summary>
public sealed class RecommendationEngine : IRecommendationEngine
{
    /// <inheritdoc />
    public IReadOnlyCollection<Recommendation> Generate(
        RootCauseAnalysis analysis,
        IEnumerable<CorrelatedFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        ArgumentNullException.ThrowIfNull(findings);

        var findingList = findings.ToList();

        if (findingList.Count == 0)
        {
            return [];
        }

        var recommendations = new List<Recommendation>();

        var hasReplication =
            findingList.Any(f =>
                f.CorrelationType.Contains(
                    "Replication",
                    StringComparison.OrdinalIgnoreCase));

        var hasRpc =
            findingList.Any(f =>
                f.CorrelationType.Contains(
                    "RPC",
                    StringComparison.OrdinalIgnoreCase));

        var hasPatch =
            findingList.Any(f =>
                f.CorrelationType.Contains(
                    "Patch",
                    StringComparison.OrdinalIgnoreCase));

        var hasInfrastructure =
            findingList.Any(f =>
                f.CorrelationType.Contains(
                    "Infrastructure",
                    StringComparison.OrdinalIgnoreCase));


        if (hasPatch && hasReplication)
        {
            recommendations.Add(
                new Recommendation(
                    "Align domain controller patch baselines",
                    "Review and align the patch baseline across affected " +
                    "domain controllers. Verify that required cumulative " +
                    "updates are installed consistently before validating " +
                    "Active Directory replication.",
                    RecommendationPriority.High)
                {
                    RootCauseAnalysisId = analysis.Id
                });
        }


        if (hasRpc && hasReplication)
        {
            recommendations.Add(
                new Recommendation(
                    "Validate RPC connectivity",
                    "Validate RPC communication between affected Active " +
                    "Directory replication partners and confirm that required " +
                    "network communication is functioning normally.",
                    RecommendationPriority.High)
                {
                    RootCauseAnalysisId = analysis.Id
                });
        }


        if (hasInfrastructure && hasReplication)
        {
            recommendations.Add(
                new Recommendation(
                    "Validate domain controller health",
                    "Review infrastructure health on affected domain " +
                    "controllers, including available disk space, CPU, " +
                    "memory, and relevant operating system health indicators.",
                    RecommendationPriority.Medium)
                {
                    RootCauseAnalysisId = analysis.Id
                });
        }


        if (hasReplication)
        {
            recommendations.Add(
                new Recommendation(
                    "Perform post-maintenance replication validation",
                    "After corrective actions are completed, validate " +
                    "Active Directory replication health and confirm that " +
                    "replication partners are operating normally.",
                    RecommendationPriority.Medium)
                {
                    RootCauseAnalysisId = analysis.Id
                });
        }


        if (hasPatch)
        {
            recommendations.Add(
                new Recommendation(
                    "Implement automated patch compliance monitoring",
                    "Introduce automated monitoring to identify domain " +
                    "controller patch baseline inconsistencies before they " +
                    "can contribute to service-impacting incidents.",
                    RecommendationPriority.Medium)
                {
                    RootCauseAnalysisId = analysis.Id
                });
        }

        return recommendations;
    }
}