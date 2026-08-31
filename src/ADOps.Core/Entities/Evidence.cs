namespace ADOps.Core.Entities;

public sealed class Evidence
{
    public required string EvidenceId { get; init; }

    public required string InvestigationId { get; init; }

    public required EvidenceType Type { get; init; }

    public required string Source { get; init; }

    public required string Target { get; init; }

    public required DateTimeOffset CollectedUtc { get; init; }

    public required string Summary { get; init; }

    public string? ErrorCode { get; init; }

    public string? Details { get; init; }

    public bool IsValid { get; init; } = true;
}