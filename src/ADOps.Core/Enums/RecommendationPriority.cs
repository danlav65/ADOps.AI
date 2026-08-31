namespace ADOps.Core.Enums;

/// <summary>
/// Defines the priority assigned to an operational recommendation.
/// </summary>
public enum RecommendationPriority
{
    /// <summary>
    /// Low priority recommendation that can be addressed during normal operations.
    /// </summary>
    Low,

    /// <summary>
    /// Medium priority recommendation that should be addressed in a planned timeframe.
    /// </summary>
    Medium,

    /// <summary>
    /// High priority recommendation that should be addressed promptly.
    /// </summary>
    High,

    /// <summary>
    /// Critical priority recommendation requiring immediate attention.
    /// </summary>
    Critical
}