using ADOps.Core.Enums;

namespace ADOps.Core.Entities;

/// <summary>
/// Represents knowledge retrieved for an investigation,
/// including any identified guidance conflicts.
/// </summary>
public sealed class KnowledgeRetrievalResult
{
    /// <summary>
    /// Knowledge matches returned by retrieval.
    /// </summary>
    public required IReadOnlyCollection<KnowledgeMatch> Matches
    {
        get;
        init;
    }

    /// <summary>
    /// Potential conflicts identified among the guidance.
    /// </summary>
    public required IReadOnlyCollection<KnowledgeConflict> Conflicts
    {
        get;
        init;
    }

    /// <summary>
    /// UTC timestamp when the retrieval result was assembled.
    /// </summary>
    public required DateTimeOffset RetrievedUtc
    {
        get;
        init;
    }

    /// <summary>
    /// Indicates whether analysis was performed.
    /// An empty Conflicts collection does not imply analysis was completed.
    /// </summary>
    public KnowledgeConflictAnalysisStatus ConflictAnalysisStatus
    {
        get;
        init;
    } = KnowledgeConflictAnalysisStatus.NotPerformed;
}