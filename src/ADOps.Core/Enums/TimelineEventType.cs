namespace ADOps.Core.Enums;

/// <summary>
/// Defines the type of event recorded in an investigation timeline.
/// </summary>
public enum TimelineEventType
{
    /// <summary>
    /// Investigation was created.
    /// </summary>
    InvestigationCreated,

    /// <summary>
    /// New evidence was collected.
    /// </summary>
    EvidenceCollected,

    /// <summary>
    /// A finding was identified.
    /// </summary>
    FindingCreated,

    /// <summary>
    /// Root cause analysis was generated or updated.
    /// </summary>
    RootCauseIdentified,

    /// <summary>
    /// A recommendation was generated.
    /// </summary>
    RecommendationCreated,

    /// <summary>
    /// A recommendation status changed.
    /// </summary>
    RecommendationUpdated,

    /// <summary>
    /// Investigation was completed.
    /// </summary>
    InvestigationCompleted
}