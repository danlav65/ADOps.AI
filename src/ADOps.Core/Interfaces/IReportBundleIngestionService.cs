using ADOps.Core.Entities.Reporting;

namespace ADOps.Core.Interfaces;

public interface IReportBundleIngestionService
{
    Task<ReportBundleIngestionResult> IngestAsync(
        ReportBundle bundle,
        CancellationToken cancellationToken = default);
}