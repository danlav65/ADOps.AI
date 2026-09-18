namespace ADOps.Application.Reports;

public sealed class EvidenceReportItem
{
    public required string Type { get; init; }

    public required string Source { get; init; }

    public required string Target { get; init; }

    public required string Summary { get; init; }

    public required bool IsValid { get; init; }

    public required string EvidenceId { get; init; }

    public required DateTimeOffset CollectedUtc { get; init; }

    public string? ErrorCode { get; init; }

    public string? Details { get; init; }
}