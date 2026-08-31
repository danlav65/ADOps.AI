using ADOps.Core.Entities;

namespace ADOps.Core.Interfaces;

/// <summary>
/// Defines correlation analysis operations for investigation evidence.
/// </summary>
public interface ICorrelationEngine
{
    /// <summary>
    /// Correlates evidence items into identified findings.
    /// </summary>
    /// <param name="evidence">Evidence collected during investigation.</param>
    /// <param name="topology">Active Directory topology context.</param>
    /// <returns>Correlated findings identified from the evidence.</returns>
    IReadOnlyCollection<CorrelatedFinding> Correlate(
        IReadOnlyCollection<Evidence> evidence,
        TopologyContext topology);
}