using ADOps.Application.Investigation;
using ADOps.Application.Reports;
using ADOps.Core.Entities;
using ADOps.Core.Entities.Reporting;
using ADOps.Core.Enums;
using ADOps.Core.Interfaces;
using ADOps.Portal.Services;
using Microsoft.AspNetCore.Hosting;

namespace ADOps.Portal.Tests;

public sealed class SfoDemoInvestigationProviderTests
{
    [Fact]
    public void GetInvestigation_ReturnsRunningSfoInvestigation()
    {
        var provider =
            new SfoDemoInvestigationProvider(
                new FakeReportBundleIngestionService(),
                new FakeInvestigationService(),
                new FakeWebHostEnvironment());

        var investigation = provider.GetInvestigation();

        Assert.Equal(
            "INC-SFO-20260709",
            investigation.InvestigationNumber);

        Assert.Equal(
            InvestigationStatus.Running,
            investigation.Status);

        Assert.Equal(
            "INC-SFO-20260709",
            investigation.Incident.IncidentNumber);

        Assert.Equal(
            "SFO",
            investigation.Incident.SiteCode);

        Assert.Equal(
            "Production",
            investigation.Incident.Environment);

        Assert.Equal(
            SeverityLevel.Critical,
            investigation.Incident.Severity);

        Assert.Equal(
            IncidentStatus.Investigating,
            investigation.Incident.Status);

        Assert.Equal(
            new DateTimeOffset(
                2026,
                7,
                9,
                12,
                0,
                0,
                TimeSpan.Zero),
            investigation.StartedUtc);

        Assert.Equal(
            investigation.StartedUtc,
            investigation.Incident.DetectedUtc);
    }

    private sealed class FakeReportBundleIngestionService
        : IReportBundleIngestionService
    {
        public ReportBundleIngestionResult Result { get; set; } = null!;

        public ReportBundle? ReceivedBundle { get; private set; }

        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task<ReportBundleIngestionResult> IngestAsync(
            ReportBundle bundle,
            CancellationToken cancellationToken = default)
        {
            ReceivedBundle = bundle;
            ReceivedCancellationToken = cancellationToken;

            return Task.FromResult(Result);
        }
    }

    private sealed class FakeInvestigationService
        : IInvestigationService
    {
        public InvestigationReport Result { get; set; } = null!;

        public int CallCount { get; private set; }

        public ADOps.Core.Entities.Investigation? ReceivedInvestigation
        {
            get;
            private set;
        }

        public InvestigationSnapshot? ReceivedSnapshot
        {
            get;
            private set;
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
            CallCount++;

            ReceivedInvestigation = investigation;
            ReceivedSnapshot = snapshot;
            return Task.FromResult(Result);
        }
    }

    private sealed class FakeWebHostEnvironment
        : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "ADOps.Portal.Tests";

        public string EnvironmentName { get; set; } = "Test";

        public string WebRootPath { get; set; } = string.Empty;

        public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider
        {
            get;
            set;
        } = new Microsoft.Extensions.FileProviders.NullFileProvider();

        public string ContentRootPath { get; set; } = string.Empty;

        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider
        {
            get;
            set;
        } = new Microsoft.Extensions.FileProviders.NullFileProvider();
    }

    [Fact]
    public void GetDemoContext_ReturnsExpectedSfoContext()
    {
        var provider =
            new SfoDemoInvestigationProvider(
                new FakeReportBundleIngestionService(),
                new FakeInvestigationService(),
                new FakeWebHostEnvironment());

        var context = provider.GetDemoContext();

        Assert.Equal(
            "INC-SFO-20260709",
            context.IncidentNumber);

        Assert.Equal(
            "SFO",
            context.Site);

        Assert.Equal(
            "apcflex.aero",
            context.Domain);

        Assert.Equal(
            "Production",
            context.Environment);

        Assert.Equal(
            "P1 / Critical",
            context.Severity);

        Assert.Equal(
            new DateTimeOffset(
                2026,
                7,
                9,
                12,
                0,
                0,
                TimeSpan.Zero),
            context.DetectedUtc);

        Assert.Contains(
            "500 workstations",
            context.BusinessImpact);

        Assert.Contains(
            "60 flights",
            context.BusinessImpact);

        Assert.Contains(
            "replication",
            context.TechnicalImpact,
            StringComparison.OrdinalIgnoreCase);

        Assert.Equal(
            [
                "SFOFLEX-DC1",
                "SFOFLEX-DC2",
                "ZUSW-DC1"
            ],
            context.AffectedDomainControllers);

        Assert.Contains(
            "Active Directory replication failure",
            context.KeySymptoms);

        Assert.Contains(
            "RPC communication failure",
            context.KeySymptoms);

        Assert.Contains(
            "Patch baseline drift",
            context.KeySymptoms);
    }

    [Fact]
    public async Task GetAsync_ReturnsInvestigationReport_WhenIngestionSucceeds()
    {
        var snapshot =
            new InvestigationSnapshot
            {
                InvestigationId = "INC-SFO-20260709",
                StartedUtc =
                    new DateTimeOffset(
                        2026,
                        7,
                        9,
                        12,
                        0,
                        0,
                        TimeSpan.Zero),
                OperationalContext =
                    new OperationalContext
                    {
                        Target = "SFO",
                        Site = "SFO",
                        Environment = "Production"
                    }
            };

        var expectedReport =
            new InvestigationReport
            {
                InvestigationId = "INC-SFO-20260709",
                IncidentNumber = "INC-SFO-20260709",
                GeneratedUtc =
                    new DateTimeOffset(
                        2026,
                        7,
                        9,
                        13,
                        0,
                        0,
                        TimeSpan.Zero),
                ExecutiveSummary =
                    new ExecutiveSummary
                    {
                        Summary = "Test summary",
                        BusinessImpact = "Test impact",
                        TechnicalImpact = "Test technical impact"
                    },
                Evidence = [],
                Correlations = [],
                RootCause = "Test root cause",
                Recommendations = [],
                OverallConfidence = 0.75
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
                        AcceptedReportCount = 5,
                        RejectedReportCount = 0,
                        Snapshot = snapshot
                    }
            };

        var investigationService =
            new FakeInvestigationService
            {
                Result = expectedReport
            };

        var provider =
            new SfoDemoInvestigationProvider(
                ingestionService,
                investigationService,
            new FakeWebHostEnvironment
            {
                ContentRootPath =
                    GetPortalContentRoot()
            });

        var actualReport =
            await provider.GetAsync();

        Assert.Same(
            expectedReport,
            actualReport);

        Assert.NotNull(
            ingestionService.ReceivedBundle);

        Assert.Equal(
            "BUNDLE-SFO-20260709",
            ingestionService.ReceivedBundle!.BundleId);

        Assert.Equal(
            "INC-SFO-20260709",
            ingestionService.ReceivedBundle.InvestigationId);

        Assert.Equal(
            "SFO",
            ingestionService.ReceivedBundle.Site);

        Assert.Equal(
            5,
            ingestionService.ReceivedBundle.Reports.Count);

        Assert.Same(
            snapshot,
            investigationService.ReceivedSnapshot);

        Assert.NotNull(
            investigationService.ReceivedInvestigation);

        Assert.Equal(
            "INC-SFO-20260709",
            investigationService.ReceivedInvestigation!.InvestigationNumber);
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

    [Fact]
    public async Task GetAsync_Throws_WhenIngestionFails()
    {
        var ingestionService =
            new FakeReportBundleIngestionService
            {
                Result =
                    new ReportBundleIngestionResult
                    {
                        BundleId = "BUNDLE-SFO-20260709",
                        InvestigationId = "INC-SFO-20260709",
                        Succeeded = false,
                        ReportCount = 5,
                        AcceptedReportCount = 4,
                        RejectedReportCount = 1,
                        Errors = ["Replication report was rejected."]
                    }
            };

        var investigationService =
            new FakeInvestigationService();

        var environment =
            new FakeWebHostEnvironment
            {
                ContentRootPath = GetPortalContentRoot()
            };

        var provider =
            new SfoDemoInvestigationProvider(
                ingestionService,
                investigationService,
                environment);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => provider.GetAsync());

        Assert.Contains(
            "Replication report was rejected.",
            exception.Message);

        Assert.Equal(0, investigationService.CallCount);
    }

    [Fact]
    public async Task GetAsync_Throws_WhenSnapshotIsMissing()
    {
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
                        AcceptedReportCount = 5,
                        RejectedReportCount = 0,
                        Snapshot = null,
                        Errors = []
                    }
            };

        var investigationService =
            new FakeInvestigationService();

        var environment =
            new FakeWebHostEnvironment
            {
                ContentRootPath = GetPortalContentRoot()
            };

        var provider =
            new SfoDemoInvestigationProvider(
                ingestionService,
                investigationService,
                environment);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => provider.GetAsync());

        Assert.Equal(
            "The SFO report bundle could not be ingested.",
            exception.Message);

        Assert.Equal(0, investigationService.CallCount);
    }
}