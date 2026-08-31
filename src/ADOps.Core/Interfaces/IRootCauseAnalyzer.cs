using ADOps.Core.Entities;

namespace ADOps.Core.Interfaces;

/// <summary>
/// Analyzes correlated findings and produces a root cause analysis.
/// </summary>
public interface IRootCauseAnalyzer
{
    /// <summary>
    /// Creates a root cause analysis from correlated findings.
    /// </summary>
    /// <param name="findings">The correlated findings.</param>
    /// <returns>A generated root cause analysis.</returns>
    RootCauseAnalysis Analyze(
        IEnumerable<CorrelatedFinding> findings);
}