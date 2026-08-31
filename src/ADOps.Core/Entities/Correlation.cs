using ADOps.Core.Enums;

namespace ADOps.Core.Entities;

/// <summary>
/// Represents a correlation identified during investigation analysis.
/// </summary>
public sealed class Correlation : EntityBase
{
    private readonly List<Guid> _supportingEvidenceIds = new();

    /// <summary>
    /// Name of the correlation.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Description of the correlated condition.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Confidence score assigned to this correlation.
    /// </summary>
    public double Confidence { get; private set; }

    /// <summary>
    /// Evidence identifiers supporting this correlation.
    /// </summary>
    public IReadOnlyCollection<Guid> SupportingEvidenceIds =>
        _supportingEvidenceIds;

    /// <summary>
    /// Category of the identified correlation.
    /// </summary>
    public CorrelationCategory Category { get; init; }
}