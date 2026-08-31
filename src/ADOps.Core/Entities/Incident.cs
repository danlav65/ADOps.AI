using ADOps.Core.Enums;

namespace ADOps.Core.Entities;

/// <summary>
/// Represents an operational incident being investigated.
/// </summary>
public sealed class Incident : EntityBase
{
    private readonly List<Guid> _evidenceIds = [];
    private readonly List<Guid> _timelineEventIds = [];

    /// <summary>
    /// Unique incident identifier.
    /// </summary>
    public required string IncidentNumber { get; init; }

    /// <summary>
    /// Title of the incident.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Description of the incident.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Environment where the incident occurred.
    /// </summary>
    public required string Environment { get; init; }

    /// <summary>
    /// Site code associated with the incident.
    /// </summary>
    public required string SiteCode { get; init; }

    /// <summary>
    /// Severity level assigned to the incident.
    /// </summary>
    public SeverityLevel Severity { get; private set; }

    /// <summary>
    /// Current status of the incident.
    /// </summary>
    public IncidentStatus Status { get; private set; }

    /// <summary>
    /// UTC timestamp when the incident was detected.
    /// </summary>
    public DateTimeOffset DetectedUtc { get; init; }

    /// <summary>
    /// UTC timestamp when the incident was resolved.
    /// </summary>
    public DateTimeOffset? ResolvedUtc { get; private set; }

    /// <summary>
    /// Evidence identifiers associated with the incident.
    /// </summary>
    public IReadOnlyCollection<Guid> EvidenceIds => _evidenceIds;

    /// <summary>
    /// Timeline event identifiers associated with the incident.
    /// </summary>
    public IReadOnlyCollection<Guid> TimelineEventIds => _timelineEventIds;

    /// <summary>
    /// Changes the incident status.
    /// </summary>
    public void ChangeStatus(IncidentStatus status)
    {
        Status = status;

        if (status == IncidentStatus.Resolved)
        {
            ResolvedUtc = DateTimeOffset.UtcNow;
        }

        Touch();
    }

    /// <summary>
    /// Updates the incident severity.
    /// </summary>
    public void UpdateSeverity(SeverityLevel severity)
    {
        Severity = severity;
        Touch();
    }

    /// <summary>
    /// Adds evidence associated with this incident.
    /// </summary>
    public void AddEvidence(Guid evidenceId)
    {
        if (!_evidenceIds.Contains(evidenceId))
        {
            _evidenceIds.Add(evidenceId);
            Touch();
        }
    }

    /// <summary>
    /// Adds a timeline event associated with this incident.
    /// </summary>
    public void AddTimelineEvent(Guid timelineEventId)
    {
        if (!_timelineEventIds.Contains(timelineEventId))
        {
            _timelineEventIds.Add(timelineEventId);
            Touch();
        }
    }

    /// <summary>
    /// Updates the incident description.
    /// </summary>
    public void UpdateDescription(string? description)
    {
        Description = description;
        Touch();
    }
}