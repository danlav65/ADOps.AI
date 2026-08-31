using ADOps.Core.Entities;
using ADOps.Core.Entities.Reporting;

namespace ADOps.Infrastructure.Ingestion.Patch;

public interface IPatchReportIngestor
{
    IReadOnlyCollection<PatchRecord> Ingest(
        ReportFile report,
        CollectorContext context);
}
