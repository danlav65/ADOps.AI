using ADOps.Core.Entities;
using ADOps.Core.Entities.Reporting;

namespace ADOps.Core.Interfaces;

public interface IReportNormalizer<T>
{
    ReportType SupportedType { get; }

    Task<ReportNormalizationResult<T>> NormalizeAsync(
        ReportFile report,
        CollectorContext context,
        CancellationToken cancellationToken = default);
}