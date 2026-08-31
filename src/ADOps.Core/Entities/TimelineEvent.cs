using ADOps.Core.Enums;

namespace ADOps.Core.Entities;

/// <summary>
/// Represents an event in the chronological history of an investigation.
/// </summary>
public sealed class TimelineEvent : EntityBase
{
    public required DateTimeOffset EventTimeUtc { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }

    public TimelineEventType Type { get; init; }

    public string? Source { get; init; }

    public Guid? RelatedEvidenceId { get; init; }
}