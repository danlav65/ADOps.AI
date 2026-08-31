using ADOps.Core.Entities;
using ADOps.Core.Entities.Reporting;
using ADOps.Core.Interfaces;
using ADOps.Infrastructure.Evidence;
using ADOps.Infrastructure.Ingestion.Patch;
using ADOps.Infrastructure.Ingestion.Replication;
using ADOps.Infrastructure.Ingestion.SystemInfo;
using ADOps.Infrastructure.Ingestion.Rpc;

namespace ADOps.Infrastructure.Ingestion.Reporting;

public sealed class ReportBundleIngestionService
    : IReportBundleIngestionService
{
    private readonly ReportBundleValidator _validator;
    private readonly IReplicationReportIngestor _replicationIngestor;
    private readonly IPatchReportIngestor _patchIngestor;
    private readonly ISystemInfoReportIngestor _systemInfoIngestor;
    private readonly IRpcReportIngestor _rpcIngestor;
    private readonly IEvidenceNormalizer _evidenceNormalizer;

    public ReportBundleIngestionService(
        ReportBundleValidator validator,
        IReplicationReportIngestor replicationIngestor,
        IPatchReportIngestor patchIngestor,
        ISystemInfoReportIngestor systemInfoIngestor,
        IRpcReportIngestor rpcReportIngestor,
        IEvidenceNormalizer evidenceNormalizer)
    {
        _validator = validator;
        _replicationIngestor = replicationIngestor;
        _patchIngestor = patchIngestor;
        _systemInfoIngestor = systemInfoIngestor;
        _rpcIngestor = rpcReportIngestor;
        _evidenceNormalizer = evidenceNormalizer;
    }

    public Task<ReportBundleIngestionResult> IngestAsync(
        ReportBundle bundle,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(bundle);

        cancellationToken.ThrowIfCancellationRequested();

        var validationErrors =
            _validator.Validate(bundle);

        if (validationErrors.Count > 0)
        {
            return Task.FromResult(
                new ReportBundleIngestionResult
                {
                    BundleId = bundle.BundleId,
                    InvestigationId = bundle.InvestigationId,
                    Succeeded = false,
                    ReportCount = bundle.Reports.Count,
                    AcceptedReportCount = 0,
                    RejectedReportCount = bundle.Reports.Count,
                    Snapshot = null,
                    Errors = validationErrors
                });
        }

        var context = BuildContext(bundle);

        var snapshot = new InvestigationSnapshot
        {
            InvestigationId =
                bundle.InvestigationId,

            StartedUtc =
                bundle.CreatedUtc,

            OperationalContext =
                new OperationalContext
                {
                    Target = context.DomainName ?? "Unknown",
                    Site = context.Site,
                    Environment = "Unknown"
                }
        };

        var errors = new List<string>();
        var acceptedCount = 0;
        var rejectedCount = 0;

        foreach (var report in bundle.Reports)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                IngestReport(
                    report,
                    context,
                    snapshot);

                acceptedCount++;
            }
            catch (Exception ex)
                when (ex is not OperationCanceledException)
            {
                rejectedCount++;

                errors.Add(
                    $"Report '{report.ReportId}' failed ingestion: {ex.Message}");
            }
        }

        snapshot.Topology =
            BuildTopology(snapshot.Replication);

        var evidence =
            _evidenceNormalizer.Normalize(snapshot);

        snapshot.Evidence.AddRange(evidence);

        snapshot.CompletedUtc =
            DateTimeOffset.UtcNow;

        return Task.FromResult(
            new ReportBundleIngestionResult
            {
                BundleId = bundle.BundleId,
                InvestigationId = bundle.InvestigationId,
                Succeeded = rejectedCount == 0,
                ReportCount = bundle.Reports.Count,
                AcceptedReportCount = acceptedCount,
                RejectedReportCount = rejectedCount,
                Snapshot = snapshot,
                Errors = errors
            });
    }

    private void IngestReport(
        ReportFile report,
        CollectorContext context,
        InvestigationSnapshot snapshot)
    {
        switch (report.Type)
        {
            case ReportType.Replication:
                snapshot.Replication.AddRange(
                    _replicationIngestor.Ingest(
                        report,
                        context));
                break;

            case ReportType.Patch:
                snapshot.Patches.AddRange(
                    _patchIngestor.Ingest(
                        report,
                        context));
                break;

            case ReportType.SystemInfo:
                snapshot.SystemInfo.AddRange(
                    _systemInfoIngestor.Ingest(
                        report,
                        context));
                break;

            case ReportType.Rpc:
                snapshot.Rpc.AddRange(
                    _rpcIngestor.Ingest(
                        report,
                        context));
            break;

            default:
                throw new ArgumentException(
                    $"Unsupported report type: {report.Type}.",
                    nameof(report));
        }
    }

    private static CollectorContext BuildContext(
        ReportBundle bundle)
    {
        var domainControllers =
            bundle.Reports
                .Select(report => report.DomainController)
                .Where(controller =>
                    !string.IsNullOrWhiteSpace(controller))
                .Select(controller => controller!)
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        return new CollectorContext
        {
            InvestigationId =
                bundle.InvestigationId,

            Site =
                bundle.Site,

            DomainControllers =
                domainControllers
        };
    }

    private static TopologyContext BuildTopology(
        IReadOnlyCollection<
            ADOps.Core.Entities.Replication.ReplicationRecord>
            records)
    {
        var relationships =
            records
                .Where(record =>
                    !string.IsNullOrWhiteSpace(
                        record.SourceDomainController) &&
                    !string.IsNullOrWhiteSpace(
                        record.PartnerDomainController))
                .GroupBy(
                    record => new
                    {
                        record.SourceDomainController,
                        record.PartnerDomainController
                    })
                .Select(group =>
                {
                    var latest =
                        group
                            .OrderByDescending(
                                record => record.CollectedUtc)
                            .First();

                    return new ReplicationPartnerRelationship
                    {
                        SourceDomainController =
                            latest.SourceDomainController,

                        PartnerDomainController =
                            latest.PartnerDomainController,

                        SourceSite =
                            latest.SourceSite ?? string.Empty,

                        PartnerSite =
                            latest.PartnerSite ?? string.Empty,

                        DiscoveredUtc =
                            latest.CollectedUtc,

                        IsActive = true
                    };
                })
                .ToList();

        return new TopologyContext
        {
            ReplicationPartners =
                relationships
        };
    }
}
