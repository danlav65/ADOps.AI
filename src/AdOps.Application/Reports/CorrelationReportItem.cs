namespace ADOps.Application.Reports;

    public sealed class CorrelationReportItem
    {
        public required string Type { get; init; }

        public required string Summary { get; init; }

        public required double Confidence { get; init; }

        public required IReadOnlyCollection<string> EvidenceIds { get; init; }
    }