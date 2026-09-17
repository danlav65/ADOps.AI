using ADOps.Application.Investigation;
using ADOps.Application.Reports;
using ADOps.Core.Entities;
using ADOps.Core.Entities.Reporting;
using ADOps.Core.Enums;
using ADOps.Core.Interfaces;

namespace ADOps.Portal.Services;

public sealed class SfoDemoInvestigationProvider
{
    private const string InvestigationId = "INC-SFO-20260709";
    private const string BundleId = "BUNDLE-SFO-20260709";
    private const string Site = "SFO";
    private const string BundleFolder = "SFO-20260709";

    private readonly IReportBundleIngestionService _ingestionService;
    private readonly IInvestigationService _investigationService;
    private readonly IWebHostEnvironment _environment;

    public SfoDemoInvestigationProvider(
        IReportBundleIngestionService ingestionService,
        IInvestigationService investigationService,
        IWebHostEnvironment environment)
    {
        _ingestionService =
            ingestionService ??
            throw new ArgumentNullException(nameof(ingestionService));

        _investigationService =
            investigationService ??
            throw new ArgumentNullException(nameof(investigationService));

        _environment =
            environment ??
            throw new ArgumentNullException(nameof(environment));
    }

    public ADOps.Core.Entities.Investigation GetInvestigation()
    {
        return BuildSfoInvestigation();
    }

    public async Task<InvestigationReport> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var bundle = BuildSfoBundle();

        var ingestionResult =
            await _ingestionService.IngestAsync(
                bundle,
                cancellationToken);

        if (!ingestionResult.Succeeded ||
            ingestionResult.Snapshot is null)
        {
            var errors =
                ingestionResult.Errors.Count == 0
                    ? "The SFO report bundle could not be ingested."
                    : string.Join(
                        Environment.NewLine,
                        ingestionResult.Errors);

            throw new InvalidOperationException(errors);
        }

        var investigation =
            BuildSfoInvestigation();

        return await _investigationService.InvestigateAsync(
            investigation,
            ingestionResult.Snapshot);
    }

    private ReportBundle BuildSfoBundle()
    {
        var bundlePath =
            Path.Combine(
                _environment.ContentRootPath,
                "..",
                "..",
                "test-data",
                "report-bundles",
                BundleFolder);

        bundlePath =
            Path.GetFullPath(bundlePath);

        var createdUtc =
            new DateTimeOffset(
                2026,
                7,
                9,
                12,
                0,
                0,
                TimeSpan.Zero);

        var bundle = new ReportBundle
        {
            BundleId = BundleId,
            InvestigationId = InvestigationId,
            Site = Site,
            CreatedUtc = createdUtc
        };

        bundle.Reports.Add(
            CreateReport(
                "R-001",
                ReportType.Replication,
                "replication.txt",
                "SFOFLEX-DC1"));

        bundle.Reports.Add(
            CreateReport(
                "R-005",
                ReportType.Replication,
                "replication-zusw.txt",
                "ZUSW-DC1"));

        bundle.Reports.Add(
            CreateReport(
                "R-002",
                ReportType.Patch,
                "patch.json",
                "SFOFLEX-DC1"));

        bundle.Reports.Add(
            CreateReport(
                "R-003",
                ReportType.SystemInfo,
                "systeminfo.json",
                "SFOFLEX-DC1"));

        bundle.Reports.Add(
            CreateReport(
                "R-004",
                ReportType.Rpc,
                "rpc.json",
                "SFOFLEX-DC1"));

        return bundle;

        ReportFile CreateReport(
            string reportId,
            ReportType type,
            string fileName,
            string domainController)
        {
            var filePath =
                Path.Combine(
                    bundlePath,
                    fileName);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    $"SFO demo report file was not found: {filePath}",
                    filePath);
            }

            return new ReportFile
            {
                ReportId = reportId,
                Type = type,
                FileName = fileName,
                Content = File.ReadAllText(filePath),
                DomainController = domainController,
                CollectedUtc = createdUtc
            };
        }
    }

    private static ADOps.Core.Entities.Investigation
        BuildSfoInvestigation()
    {
        var incident =
            new Incident
            {
                IncidentNumber = InvestigationId,
                Title =
                    "SFO Active Directory Replication Incident",
                Environment =
                    "Production",
                SiteCode =
                    Site,
                DetectedUtc =
                    new DateTimeOffset(
                        2026,
                        7,
                        9,
                        12,
                        0,
                        0,
                        TimeSpan.Zero)
            };

        incident.UpdateDescription(
            "P1 Active Directory replication incident at SFO");
        

        incident.UpdateSeverity(
            SeverityLevel.Critical);

        incident.ChangeStatus(
            IncidentStatus.Investigating);

        var investigation =
            new ADOps.Core.Entities.Investigation
            {
                InvestigationNumber = InvestigationId,
                Incident = incident,
                StartedUtc =
                    new DateTimeOffset(
                        2026,
                        7,
                        9,
                        12,
                        0,
                        0,
                        TimeSpan.Zero)
            };

        investigation.ChangeStatus(
            InvestigationStatus.Running);

        return investigation;
    }
}