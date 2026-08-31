namespace ADOps.Core.Enums;

/// <summary>
/// Defines the lifecycle state of an operational recommendation.
/// </summary>
public enum RecommendationStatus
{
    /// <summary>
    /// Recommendation has been proposed but not yet acted upon.
    /// </summary>
    Proposed,

    /// <summary>
    /// Recommendation has been approved for implementation.
    /// </summary>
    Approved,

    /// <summary>
    /// Recommendation has been completed.
    /// </summary>
    Completed,

    /// <summary>
    /// Recommendation was rejected.
    /// </summary>
    Rejected
}