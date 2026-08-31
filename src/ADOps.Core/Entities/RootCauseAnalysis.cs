namespace ADOps.Core.Entities;

/// <summary>
/// Represents the final Root Cause Analysis for an investigation.
/// </summary>
public sealed class RootCauseAnalysis : EntityBase
{
    /// <summary>
    /// Title of the root cause analysis.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Executive summary of the investigation findings.
    /// </summary>
    public required string ExecutiveSummary { get; init; }

    /// <summary>
    /// Identified technical root cause.
    /// </summary>
    public required string RootCause { get; init; }

    /// <summary>
    /// Business impact resulting from the incident.
    /// </summary>
    public string? BusinessImpact { get; private set; }

    /// <summary>
    /// Technical impact resulting from the incident.
    /// </summary>
    public string? TechnicalImpact { get; private set; }

    /// <summary>
    /// Corrective actions taken to resolve the issue.
    /// </summary>
    public string? CorrectiveActions { get; private set; }

    /// <summary>
    /// Preventive actions recommended to avoid recurrence.
    /// </summary>
    public string? PreventiveActions { get; private set; }

    /// <summary>
    /// Correlation finding selected as the primary basis for the root cause.
    /// </summary>
    public string? PrimaryFindingId { get; private set; }

    /// <summary>
    /// Confidence associated with the primary root cause finding.
    /// </summary>
    public double? Confidence { get; private set; }

    /// <summary>
    /// Evidence supporting the primary root cause finding.
    /// </summary>
    public IReadOnlyCollection<string> SupportingEvidenceIds
        { get; private set; } = [];

    /// <summary>
    /// Indicates whether the root cause analysis has been approved.
    /// </summary>
    public bool IsApproved { get; private set; }

    /// <summary>
    /// Timestamp when the analysis was approved.
    /// </summary>
    public DateTimeOffset? ApprovedUtc { get; private set; }

    /// <summary>
    /// Approves the root cause analysis.
    /// </summary>
    public void Approve()
    {
        IsApproved = true;
        ApprovedUtc = DateTimeOffset.UtcNow;

        Touch();
    }

    /// <summary>
    /// Sets the business impact.
    /// </summary>
    public void SetBusinessImpact(string? businessImpact)
    {
        BusinessImpact = businessImpact;
        Touch();
    }

    /// <summary>
    /// Sets the technical impact.
    /// </summary>
    public void SetTechnicalImpact(string? technicalImpact)
    {
        TechnicalImpact = technicalImpact;
        Touch();
    }

    /// <summary>
    /// Sets the corrective actions.
    /// </summary>
    public void SetCorrectiveActions(string? correctiveActions)
    {
        CorrectiveActions = correctiveActions;
        Touch();
    }

    /// <summary>
    /// Sets the preventive actions.
    /// </summary>
    public void SetPreventiveActions(string? preventiveActions)
    {
        PreventiveActions = preventiveActions;
        Touch();
    }

    /// <summary>
    /// Sets the primary finding and its supporting evidence.
    /// </summary>
    public void SetPrimaryFinding(
        string? findingId,
        double? confidence,
        IReadOnlyCollection<string>? evidenceIds)
    {
        PrimaryFindingId = findingId;
        Confidence = confidence;
        SupportingEvidenceIds =
            evidenceIds?.ToArray() ?? [];

        Touch();
    }
}