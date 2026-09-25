using ADOps.Application.Investigation;
using ADOps.Application.Reports;
using ADOps.Core.Entities;
using ADOps.Core.Entities.Reporting;
using ADOps.Core.Enums;
using ADOps.Core.Interfaces;
using ADOps.Portal.Pages.Investigation;
using ADOps.Portal.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace ADOps.Portal.Tests;

public sealed class InvestigationAnalysisPageTests
{
    [Fact]
    public async Task OnGetAsync_ExposesInvestigationReport()
    {
        // Arrange
        var collectedUtc =
            new DateTimeOffset(
                2026,
                7,  
                9,
                12,
                0,
                0,
                TimeSpan.Zero);

        var snapshot =
            new InvestigationSnapshot
            {
                InvestigationId = "INC-SFO-20260709",
                StartedUtc = collectedUtc,
                OperationalContext =
                    new OperationalContext
                    {
                        Target = "SFO",
                        Site = "SFO",
                        Environment = "Production"
                    }
            };

        var report =
            new InvestigationReport
            {
                InvestigationId = "INC-SFO-20260709",
                IncidentNumber = "INC-SFO-20260709",
                GeneratedUtc = collectedUtc,

                ExecutiveSummary =
                    new ExecutiveSummary
                    {
                        Summary =
                            "SFO Active Directory replication incident.",
                        BusinessImpact =
                            "~500 workstations and 60 flights affected.",
                        TechnicalImpact =
                            "AD replication impaired between SFO and Azure hub."
                    },

                Evidence = [],
                Correlations = [],

                RootCause =
                    "Patch baseline drift is a probable contributing factor.",

                Recommendations = [],

                OverallConfidence = 0.75,

                SupportingKnowledge = []
            };

        var ingestionService =
            new FakeReportBundleIngestionService
            {
                Result =
                    new ReportBundleIngestionResult
                    {
                        BundleId = "BUNDLE-SFO-20260709",
                        InvestigationId = "INC-SFO-20260709",
                        Succeeded = true,
                        ReportCount = 5,
                        Snapshot = snapshot,
                        Errors = []
                    }
            };

        var provider =
            new SfoDemoInvestigationProvider(
                ingestionService,
                new FakeInvestigationService(report),
                new FakeWebHostEnvironment
                {
                    ContentRootPath =
                        GetPortalContentRoot()
                });

        var model =
            new AnalysisModel(provider);

        // Act
        await model.OnGetAsync();

        // Assert
        Assert.Same(
            report,
            model.Report);
    }

    private static string GetPortalContentRoot()
    {
        return Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..",
                "..",
                ".."));
    }

    private sealed class FakeReportBundleIngestionService
        : IReportBundleIngestionService
    {
        public ReportBundleIngestionResult Result { get; set; } =
            null!;

        public Task<ReportBundleIngestionResult> IngestAsync(
            ReportBundle bundle,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result);
        }
    }

    private sealed class FakeInvestigationService
        : IInvestigationService
    {
        private readonly InvestigationReport _report;

        public FakeInvestigationService(
            InvestigationReport report)
        {
            _report = report;
        }

        public Task<InvestigationReport> InvestigateAsync(
            ADOps.Core.Entities.Investigation investigation,
            CollectorContext context,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<InvestigationReport> InvestigateAsync(
            ADOps.Core.Entities.Investigation investigation,
            InvestigationSnapshot snapshot)
        {
            return Task.FromResult(_report);
        }
    }

    private sealed class FakeWebHostEnvironment
        : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = string.Empty;

        public IFileProvider WebRootFileProvider { get; set; } =
            new NullFileProvider();

        public string WebRootPath { get; set; } = string.Empty;

        public string EnvironmentName { get; set; } = string.Empty;

        public string ContentRootPath { get; set; } = string.Empty;

        public IFileProvider ContentRootFileProvider { get; set; } =
            new NullFileProvider();
    }
}