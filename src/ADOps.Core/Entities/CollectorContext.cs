namespace ADOps.Core.Entities;

public sealed class CollectorContext
{
    public required string InvestigationId { get; init; }

    public required string Site { get; init; }

    public IReadOnlyCollection<string> DomainControllers { get; init; }
        = [];

    public string? DomainName { get; init; }

    public string? CorrelationId { get; init; }
}