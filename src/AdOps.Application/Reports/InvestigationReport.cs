namespace ADOps.Application.Reports;

public sealed class InvestigationReport
{
    public required string InvestigationId { get; init; }

    public required string IncidentNumber { get; init; }

    public required DateTimeOffset GeneratedUtc { get; init; }

    public required ExecutiveSummary ExecutiveSummary { get; init; }

    public required IReadOnlyCollection<EvidenceReportItem> Evidence { get; init; }

    public required IReadOnlyCollection<CorrelationReportItem> Correlations { get; init; }

    public required string RootCause { get; init; }

    public required IReadOnlyCollection<RecommendationItem> Recommendations { get; init; }

    public required double OverallConfidence { get; init; }
}