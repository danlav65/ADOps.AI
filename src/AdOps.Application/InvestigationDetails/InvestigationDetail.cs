using ADOps.Core.Enums;

namespace ADOps.Application.InvestigationDetails;

public sealed class InvestigationDetail
{
    public required string InvestigationNumber { get; init; }

    public InvestigationStatus Status { get; init; }

    public required DateTimeOffset StartedUtc { get; init; }

    public DateTimeOffset? CompletedUtc { get; init; }

    public required string IncidentNumber { get; init; }

    public required string Title { get; init; }

    public string? Description { get; init; }

    public required string Environment { get; init; }

    public required string SiteCode { get; init; }

    public SeverityLevel Severity { get; init; }

    public IncidentStatus IncidentStatus { get; init; }

    public required DateTimeOffset DetectedUtc { get; init; }

    public DateTimeOffset? ResolvedUtc { get; init; }
}
