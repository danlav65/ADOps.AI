using ADOps.Core.Enums;

namespace ADOps.Core.Entities;

/// <summary>
/// Represents an AI-generated operational recommendation.
/// </summary>
public sealed class Recommendation : EntityBase
{
    /// <summary>
    /// Recommendation title.
    /// </summary>
    public string Title { get; init; }

    /// <summary>
    /// Detailed recommendation description.
    /// </summary>
    public string Description { get; init; }

    /// <summary>
    /// Priority assigned to the recommendation.
    /// </summary>
    public RecommendationPriority Priority { get; private set; }

    /// <summary>
    /// Current lifecycle status.
    /// </summary>
    public RecommendationStatus Status { get; private set; }

    /// <summary>
    /// Associated root cause analysis identifier.
    /// </summary>
    public Guid? RootCauseAnalysisId { get; init; }

    /// <summary>
    /// Notes regarding implementation.
    /// </summary>
    public string? ImplementationNotes { get; private set; }

    /// <summary>
    /// Creates a recommendation.
    /// </summary>
    public Recommendation(
        string title,
        string description,
        RecommendationPriority priority)
    {
        Title = title;
        Description = description;
        Priority = priority;
        Status = RecommendationStatus.Proposed;
    }

    /// <summary>
    /// Approves the recommendation.
    /// </summary>
    public void Approve()
    {
        Status = RecommendationStatus.Approved;
        Touch();
    }

    /// <summary>
    /// Marks the recommendation complete.
    /// </summary>
    public void Complete()
    {
        Status = RecommendationStatus.Completed;
        Touch();
    }

    /// <summary>
    /// Rejects the recommendation.
    /// </summary>
    public void Reject()
    {
        Status = RecommendationStatus.Rejected;
        Touch();
    }
}