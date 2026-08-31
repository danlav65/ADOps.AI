namespace ADOps.Core.Entities.Reporting;

public sealed class ReportFile
{
    public required string ReportId { get; init; }

    public required ReportType Type { get; init; }

    public required string FileName { get; init; }

    public required string Content { get; init; }

    public string? DomainController { get; init; }

    public DateTimeOffset? CollectedUtc { get; init; }
}