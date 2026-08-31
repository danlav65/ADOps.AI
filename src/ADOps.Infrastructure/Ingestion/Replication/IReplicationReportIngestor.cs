using ADOps.Core.Entities;
using ADOps.Core.Entities.Replication;
using ADOps.Core.Entities.Reporting;

namespace ADOps.Infrastructure.Ingestion.Replication;

public interface IReplicationReportIngestor
{
    IReadOnlyCollection<ReplicationRecord> Ingest(
        ReportFile report,
        CollectorContext context);
}