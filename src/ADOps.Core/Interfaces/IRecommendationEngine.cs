using ADOps.Core.Entities;

namespace ADOps.Core.Interfaces;

/// <summary>
/// Generates operational recommendations from a root cause analysis.
/// </summary>
public interface IRecommendationEngine
{
    /// <summary>
    /// Generates recommendations based on the supplied root cause analysis
    /// and its supporting correlated findings.
    /// </summary>
    /// <param name="analysis">The root cause analysis.</param>
    /// <param name="findings">The correlated findings supporting the analysis.</param>
    /// <returns>A collection of generated recommendations.</returns>
    IReadOnlyCollection<Recommendation> Generate(
        RootCauseAnalysis analysis,
        IEnumerable<CorrelatedFinding> findings);
}