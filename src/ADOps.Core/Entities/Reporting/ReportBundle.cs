namespace ADOps.Core.Entities.Reporting;

public sealed class ReportBundle
{
    public required string BundleId { get; init; }

    public required string InvestigationId { get; init; }

    public required string Site { get; init; }

    public DateTimeOffset CreatedUtc { get; init; }

    public List<ReportFile> Reports { get; } = [];
}