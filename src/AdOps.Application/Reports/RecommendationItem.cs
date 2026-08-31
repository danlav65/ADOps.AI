namespace ADOps.Application.Reports;

public sealed class RecommendationItem
{
    public required string Action { get; init; }

    public required string Reason { get; init; }

    public required string Priority { get; init; }
}