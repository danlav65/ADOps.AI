using ADOps.Application.Investigation;
using ADOps.Application.InvestigationDetails;
using ADOps.Application.Reports;
using ADOps.Core.Entities;
using ADOps.Core.Entities.Reporting;
using ADOps.Core.Interfaces;
using ADOps.Portal.Pages.Investigation;
using ADOps.Portal.Services;
using Microsoft.AspNetCore.Hosting;

namespace ADOps.Portal.Tests;

public sealed class InvestigationDetailsPageTests
{
    [Fact]
    public async Task OnGetAsync_ExposesSupportingKnowledgeFromReport()
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
                        Summary = "Test summary",
                        BusinessImpact = "Test impact",
                        TechnicalImpact = "Test technical impact"
                    },

                Evidence = [],
                Correlations = [],
                RootCause = "Test root cause",
                Recommendations = [],

                SupportingKnowledge =
                [
                    new SupportingKnowledgeItem
                    {
                        Source =
                            "Microsoft Learn AD Replication",

                        Description =
                            "RPC connectivity should be validated when investigating replication failures.",

                        SourceType =
                            "MicrosoftDocumentation",

                        SourceUri =
                            "https://learn.microsoft.com/ad-replication"
                    }
                ],

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
                        Snapshot = snapshot,
                        Errors = []
                    }
            };

        var investigationService =
            new FakeInvestigationService
            {
                Result = report
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

        var model =
            new DetailsModel(
                provider,
                new InvestigationDetailService());

        // Act
        await model.OnGetAsync();

        // Assert
        var knowledge =
            Assert.Single(
                model.SupportingKnowledge);

        Assert.Equal(
            "Microsoft Learn AD Replication",
            knowledge.Source);

        Assert.Equal(
            "RPC connectivity should be validated when investigating replication failures.",
            knowledge.Description);

        Assert.Equal(
            "MicrosoftDocumentation",
            knowledge.SourceType);

        Assert.Equal(
            "https://learn.microsoft.com/ad-replication",
            knowledge.SourceUri);
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
        public InvestigationReport Result { get; set; } =
            null!;

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
            return Task.FromResult(Result);
        }
    }

    private sealed class FakeWebHostEnvironment
        : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } =
            "ADOps.Portal.Tests";

        public string EnvironmentName { get; set; } =
            "Test";

        public string WebRootPath { get; set; } =
            string.Empty;

        public Microsoft.Extensions.FileProviders.IFileProvider
            WebRootFileProvider
        {
            get;
            set;
        } =
            new Microsoft.Extensions.FileProviders.NullFileProvider();

        public string ContentRootPath { get; set; } =
            string.Empty;

        public Microsoft.Extensions.FileProviders.IFileProvider
            ContentRootFileProvider
        {
            get;
            set;
        } =
            new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}