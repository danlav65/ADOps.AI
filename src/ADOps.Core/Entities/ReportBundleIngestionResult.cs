using ADOps.Core.Entities;

namespace ADOps.Core.Entities.Reporting;

public sealed class ReportBundleIngestionResult
{
    public required string BundleId { get; init; }

    public required string InvestigationId { get; init; }

    public bool Succeeded { get; init; }

    public int ReportCount { get; init; }

    public int AcceptedReportCount { get; init; }

    public int RejectedReportCount { get; init; }

    public InvestigationSnapshot? Snapshot { get; init; }

    public IReadOnlyCollection<string> Errors { get; init; }
        = [];
}