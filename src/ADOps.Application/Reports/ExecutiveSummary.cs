namespace ADOps.Application.Reports;

public sealed class ExecutiveSummary
{
    public required string Summary { get; init; }

    public required string BusinessImpact { get; init; }

    public required string TechnicalImpact { get; init; }
}