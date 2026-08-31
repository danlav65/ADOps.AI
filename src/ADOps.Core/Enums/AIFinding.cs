using ADOps.Core.Enums;

namespace ADOps.Core.Entities;

/// <summary>
/// Represents an AI-generated investigation finding.
/// Findings require human validation before becoming RCA conclusions.
/// </summary>
public sealed class AIFinding : EntityBase
{
    private readonly List<Guid> _supportingEvidenceIds = [];

    /// <summary>
    /// Gets the title of the finding.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets a concise summary of the finding.
    /// </summary>
    public required string Summary { get; init; }

    /// <summary>
    /// Gets the detailed explanation supporting the finding.
    /// </summary>
    public required string Explanation { get; init; }

    /// <summary>
    /// Gets the current validation status of the finding.
    /// </summary>
    public FindingStatus Status { get; private set; }

    /// <summary>
    /// Gets the AI-generated confidence score for the finding.
    /// The value must be between 0 and 100.
    /// </summary>
    public double Confidence { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the finding was generated.
    /// </summary>
    public DateTimeOffset GeneratedUtc { get; init; }

    /// <summary>
    /// Gets the evidence identifiers supporting this finding.
    /// </summary>
    public IReadOnlyCollection<Guid> SupportingEvidenceIds =>
        _supportingEvidenceIds;

    /// <summary>
    /// Adds an evidence identifier to the finding's supporting evidence.
    /// Duplicate evidence identifiers are ignored.
    /// </summary>
    /// <param name="evidenceId">
    /// The unique identifier of the supporting evidence.
    /// </param>
    public void AddSupportingEvidence(Guid evidenceId)
    {
        if (!_supportingEvidenceIds.Contains(evidenceId))
        {
            _supportingEvidenceIds.Add(evidenceId);
            Touch();
        }
    }

    /// <summary>
    /// Sets the AI confidence score for the finding.
    /// </summary>
    /// <param name="confidence">
    /// The confidence score, from 0 to 100.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the confidence value is less than 0 or greater than 100.
    /// </exception>
    public void SetConfidence(double confidence)
    {
        if (confidence < 0 || confidence > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(confidence));
        }

        Confidence = confidence;
        Touch();
    }

    /// <summary>
    /// Updates the validation status of the finding.
    /// </summary>
    /// <param name="status">
    /// The new validation status.
    /// </param>
    public void UpdateStatus(FindingStatus status)
    {
        Status = status;
        Touch();
    }
}