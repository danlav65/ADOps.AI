using ADOps.Core.Entities.Reporting;
using ADOps.Core.Entities;
using ADOps.Infrastructure.Ingestion.Patch;

namespace ADOps.Infrastructure.Tests;

public sealed class PatchReportIngestorTests
{
    [Fact]
    public void Ingest_ValidPatchReport_ProducesRecords()
    {
        var report =
            new ReportFile
            {
                ReportId = "PATCH-001",
                Type = ReportType.Patch,
                FileName = "patch.json",
                DomainController = "SFOFLEX-DC1",
                CollectedUtc =
                    new DateTimeOffset(
                        2026,
                        7,
                        9,
                        12,
                        0,
                        0,
                        TimeSpan.Zero),
                Content =
                    """
                    [
                      {
                        "DomainController": "SFOFLEX-DC1",
                        "Site": "SFO",
                        "OperatingSystem": "Windows Server 2019",
                        "OsBuild": "17763",
                        "KnowledgeBaseArticle": "KB5060531",
                        "PatchVersion": "Security Update",
                        "InstalledUtc": "2026-07-01T00:00:00-04:00",
                        "Installed": true
                      }
                    ]
                    """
            };

        var context =
            new CollectorContext
            {
                InvestigationId = "INV-SITA-20260709",
                Site = "SFO",
                DomainControllers =
                    ["SFOFLEX-DC1"]
            };

        var ingestor =
            new PatchReportIngestor();

        var records =
            ingestor.Ingest(
                report,
                context);

        Assert.Single(records);

        var record =
            records.Single();

        Assert.Equal(
            "SFOFLEX-DC1",
            record.DomainController);

        Assert.Equal(
            "SFO",
            record.Site);

        Assert.Equal(
            "Windows Server 2019",
            record.OperatingSystem);

        Assert.Equal(
            "17763",
            record.OsBuild);

        Assert.Equal(
            "KB5060531",
            record.KnowledgeBaseArticle);

        Assert.Equal(
            "Security Update",
            record.PatchVersion);

        Assert.Equal(
            new DateTimeOffset(
                2026,
                7,
                1,
                4,
                0,
                0,
                TimeSpan.Zero),
            record.InstalledUtc);

        Assert.True(record.Installed);

        Assert.Equal(
            new DateTimeOffset(
                2026,
                7,
                9,
                12,
                0,
                0,
                TimeSpan.Zero),
            record.CollectedUtc);
    }

    [Fact]
    public void Ingest_SingleObject_ProducesOneRecord()
    {
        var report =
            new ReportFile
            {
                ReportId = "PATCH-002",
                Type = ReportType.Patch,
                FileName = "patch.json",
                DomainController = "ZUSW-DC1",
                Content =
                    """
                    {
                      "HotFixID": "KB5060531",
                      "Description": "Security Update",
                      "InstalledOn": "2026-07-01T00:00:00Z"
                    }
                    """
            };

        var context =
            new CollectorContext
            {
                InvestigationId = "INV-SITA-20260709",
                Site = "ZUSW",
                DomainControllers =
                    ["ZUSW-DC1"]
            };

        var ingestor =
            new PatchReportIngestor();

        var records =
            ingestor.Ingest(
                report,
                context);

        Assert.Single(records);

        var record =
            records.Single();

        Assert.Equal(
            "ZUSW-DC1",
            record.DomainController);

        Assert.Equal(
            "ZUSW",
            record.Site);

        Assert.Equal(
            "KB5060531",
            record.KnowledgeBaseArticle);
    }

    [Fact]
    public void Ingest_UsesReportDomainController_WhenRecordDoesNotContainOne()
    {
        var report =
            new ReportFile
            {
                ReportId = "PATCH-003",
                Type = ReportType.Patch,
                FileName = "patch.json",
                DomainController = "SFOFLEX-DC1",
                Content =
                    """
                    {
                      "HotFixID": "KB5060531",
                      "Description": "Security Update"
                    }
                    """
            };

        var context =
            new CollectorContext
            {
                InvestigationId = "INV-SITA-20260709",
                Site = "SFO",
                DomainControllers =
                    ["SFOFLEX-DC1"]
            };

        var ingestor =
            new PatchReportIngestor();

        var records =
            ingestor.Ingest(
                report,
                context);

        Assert.Single(records);

        Assert.Equal(
            "SFOFLEX-DC1",
            records.Single().DomainController);
    }

    [Fact]
    public void Ingest_WrongReportType_Throws()
    {
        var report =
            new ReportFile
            {
                ReportId = "SYS-001",
                Type = ReportType.SystemInfo,
                FileName = "systeminfo.json",
                Content = "{}"
            };

        var context =
            new CollectorContext
            {
                InvestigationId = "INV-SITA-20260709",
                Site = "SFO"
            };

        var ingestor =
            new PatchReportIngestor();

        Assert.Throws<ArgumentException>(
            () =>
                ingestor.Ingest(
                    report,
                    context));
    }

    [Fact]
    public void Ingest_EmptyContent_ReturnsEmptyCollection()
    {
        var report =
            new ReportFile
            {
                ReportId = "PATCH-004",
                Type = ReportType.Patch,
                FileName = "patch.json",
                Content = ""
            };

        var context =
            new CollectorContext
            {
                InvestigationId = "INV-SITA-20260709",
                Site = "SFO"
            };

        var ingestor =
            new PatchReportIngestor();

        var records =
            ingestor.Ingest(
                report,
                context);

        Assert.Empty(records);
    }
}
