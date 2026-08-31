using ADOps.Core.Entities;
using ADOps.Core.Entities.Reporting;

namespace ADOps.Core.Interfaces;

public interface IReportNormalizationService
{
    Task<ReportNormalizationSummary> NormalizeAsync(
        ReportFile report,
        CollectorContext context,
        CancellationToken cancellationToken = default);
}