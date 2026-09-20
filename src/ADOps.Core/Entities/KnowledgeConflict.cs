namespace ADOps.Core.Entities;

/// <summary>
/// Represents potentially conflicting technical guidance
/// from two identifiable knowledge sources.
/// </summary>
public sealed class KnowledgeConflict
{
    /// <summary>
    /// Describes the technical topic on which the guidance differs.
    /// </summary>
    public required string Topic { get; init; }

    /// <summary>
    /// Identifier of the first knowledge source.
    /// </summary>
    public required string FirstSourceId { get; init; }

    /// <summary>
    /// Identifier of the second knowledge source.
    /// </summary>
    public required string SecondSourceId { get; init; }

    /// <summary>
    /// Describes the first source's recommendation.
    /// </summary>
    public required string FirstGuidance { get; init; }

    /// <summary>
    /// Describes the second source's recommendation.
    /// </summary>
    public required string SecondGuidance { get; init; }

    /// <summary>
    /// Explains the apparent disagreement without
    /// determining which recommendation is correct.
    /// </summary>
    public required string ConflictSummary { get; init; }
}