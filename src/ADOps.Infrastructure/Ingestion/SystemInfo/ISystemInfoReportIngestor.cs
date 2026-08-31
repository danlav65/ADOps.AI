using ADOps.Core.Entities;
using ADOps.Core.Entities.Reporting;

namespace ADOps.Infrastructure.Ingestion.SystemInfo;

public interface ISystemInfoReportIngestor
{
    IReadOnlyCollection<SystemInfoRecord> Ingest(
        ReportFile report,
        CollectorContext context);
}