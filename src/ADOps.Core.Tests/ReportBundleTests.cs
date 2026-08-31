using ADOps.Core.Entities.Reporting;

namespace ADOps.Core.Tests;

public sealed class ReportBundleTests
{
    [Fact]
    public void ReportBundle_CanContainMultipleRawReports()
    {
        var createdUtc =
            DateTimeOffset.Parse(
                "2026-07-09T15:00:00Z");

        var bundle =
            new ReportBundle
            {
                BundleId = "B-001",
                InvestigationId = "INC-SFO-20260709",
                Site = "SFO",
                CreatedUtc = createdUtc
            };

        bundle.Reports.Add(
            new ReportFile
            {
                ReportId = "R-001",
                Type = ReportType.Replication,
                FileName = "SFOFLEX-DC1-repadmin.txt",
                DomainController = "SFOFLEX-DC1",
                CollectedUtc =
                    DateTimeOffset.Parse(
                        "2026-07-09T14:30:00Z"),
                Content =
                    "raw replication report"
            });

        bundle.Reports.Add(
            new ReportFile
            {
                ReportId = "R-002",
                Type = ReportType.Patch,
                FileName = "SFOFLEX-DC1-hotfix.txt",
                DomainController = "SFOFLEX-DC1",
                CollectedUtc =
                    DateTimeOffset.Parse(
                        "2026-07-09T14:31:00Z"),
                Content =
                    "raw patch report"
            });

        Assert.Equal(
            "B-001",
            bundle.BundleId);

        Assert.Equal(
            "INC-SFO-20260709",
            bundle.InvestigationId);

        Assert.Equal(
            "SFO",
            bundle.Site);

        Assert.Equal(
            createdUtc,
            bundle.CreatedUtc);

        Assert.Equal(
            2,
            bundle.Reports.Count);

        Assert.Contains(
            bundle.Reports,
            report =>
                report.Type == ReportType.Replication);

        Assert.Contains(
            bundle.Reports,
            report =>
                report.Type == ReportType.Patch);
    }
}