using ADOps.Core.Entities;
using ADOps.Core.Entities.Reporting;

namespace ADOps.Core.Interfaces;

public interface IRpcReportIngestor
{
    IReadOnlyCollection<RpcRecord> Ingest(
        ReportFile report,
        CollectorContext context);
}