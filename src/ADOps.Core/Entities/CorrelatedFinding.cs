namespace ADOps.Core.Entities;

public sealed class CorrelatedFinding
{
    public required string CorrelationId { get; init; }

    public required string InvestigationId { get; init; }

    public required string Summary { get; init; }

    public required IReadOnlyCollection<string> EvidenceIds { get; init; }

    public required string CorrelationType { get; init; }

    public required DateTimeOffset CreatedUtc { get; init; }

    public double Confidence { get; init; }
}