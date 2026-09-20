using ADOps.Core.Enums;

namespace ADOps.Core.Entities;

/// <summary>
/// Describes the origin and provenance of retrieved knowledge.
/// </summary>
public sealed class KnowledgeSource
{
    /// <summary>
    /// Stable identifier assigned to this source.
    /// </summary>
    public required string SourceId { get; init; }

    /// <summary>
    /// Human-readable source title.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Organization or publisher responsible for the content.
    /// </summary>
    public required string Publisher { get; init; }

    /// <summary>
    /// Classification of the source.
    /// </summary>
    public KnowledgeSourceType SourceType { get; init; } =
        KnowledgeSourceType.Unknown;

    /// <summary>
    /// Original source location, when available.
    /// </summary>
    public Uri? SourceUri { get; init; }

    /// <summary>
    /// Publication date, when known.
    /// </summary>
    public DateTimeOffset? PublishedUtc { get; init; }

    /// <summary>
    /// Last known modification date, when available.
    /// </summary>
    public DateTimeOffset? ModifiedUtc { get; init; }

    /// <summary>
    /// When ADOps AI retrieved or recorded this source.
    /// </summary>
    public required DateTimeOffset RetrievedUtc { get; init; }
}