using ADOps.Core.Entities;
using ADOps.Core.Entities.Replication;
using ADOps.Core.Entities.Reporting;
using ADOps.Infrastructure.Collectors.Replication;

namespace ADOps.Infrastructure.Ingestion.Replication;

public sealed class ReplicationReportIngestor
    : IReplicationReportIngestor
{
    private readonly IReplicationOutputParser _parser;

    public ReplicationReportIngestor(
        IReplicationOutputParser parser)
    {
        _parser = parser;
    }

    public IReadOnlyCollection<ReplicationRecord> Ingest(
        ReportFile report,
        CollectorContext context)
    {
        if (report.Type != ReportType.Replication)
        {
            throw new ArgumentException(
                "Report must be a replication report.",
                nameof(report));
        }

        var sourceDomainController =
            report.DomainController
            ?? throw new ArgumentException(
                "Replication report must identify its source domain controller.",
                nameof(report));

        var collectedUtc =
    report.CollectedUtc
    ?? throw new ArgumentException(
        "Replication report must identify when it was collected.",
        nameof(report));

var records =
    _parser.Parse(
        sourceDomainController,
        report.Content,
        context);

return records
            .Select(record =>
                new ReplicationRecord
                {
                    SourceDomainController =
                        record.SourceDomainController,

                    PartnerDomainController =
                        record.PartnerDomainController,

                    SourceSite =
                        record.SourceSite,

                    PartnerSite =
                        record.PartnerSite,

                    Success =
                        record.Success,

                    ErrorCode =
                        record.ErrorCode,

                    ErrorMessage =
                        record.ErrorMessage,

                    LastSuccessfulReplicationUtc =
                        record.LastSuccessfulReplicationUtc,

                    CollectedUtc =
                        collectedUtc,

                    SourceCommand =
                        record.SourceCommand
                })
            .ToList();
    }
}