using ADOps.Core.Entities;
using ADOps.Core.Entities.Reporting;
using ADOps.Infrastructure.Collectors.Replication;
using ADOps.Infrastructure.Ingestion.Replication;

namespace ADOps.Infrastructure.Tests;

public sealed class ReplicationReportIngestorTests
{
    [Fact]
    public void Ingest_RejectsNonReplicationReport()
    {
        var report =
            new ReportFile
            {
                ReportId = "R-001",
                Type = ReportType.Patch,
                FileName = "SFOFLEX-DC1-patch.txt",
                DomainController = "SFOFLEX-DC1",
                CollectedUtc =
                    new DateTimeOffset(
                        2026,
                        7,
                        9,
                        14,
                        30,
                        0,
                        TimeSpan.Zero),
                Content = "Patch report"
            };

        var context = CreateContext();

        var ingestor =
            new ReplicationReportIngestor(
                new ReplicationOutputParser());

        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    ingestor.Ingest(
                        report,
                        context));

        Assert.Contains(
            "replication report",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Ingest_RejectsReplicationReportWithoutDomainController()
    {
        var report =
            new ReportFile
            {
                ReportId = "R-002",
                Type = ReportType.Replication,
                FileName = "repadmin.txt",
                DomainController = null,
                CollectedUtc =
                    new DateTimeOffset(
                        2026,
                        7,
                        9,
                        14,
                        30,
                        0,
                        TimeSpan.Zero),
                Content =
                    """
                    Repadmin: running command /showrepl SFOFLEX-DC1

                    From server: ZUSW-DC1

                    Last attempt @ 2026-07-09 14:25:00 was successful.
                    """
            };

        var context = CreateContext();

        var ingestor =
            new ReplicationReportIngestor(
                new ReplicationOutputParser());

        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    ingestor.Ingest(
                        report,
                        context));

        Assert.Contains(
            "source domain controller",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Ingest_RejectsReplicationReportWithoutCollectionTimestamp()
    {
        var report =
            new ReportFile
            {
                ReportId = "R-003",
                Type = ReportType.Replication,
                FileName = "SFOFLEX-DC1-repadmin.txt",
                DomainController = "SFOFLEX-DC1",
                CollectedUtc = null,
                Content =
                    """
                    Repadmin: running command /showrepl SFOFLEX-DC1

                    From server: ZUSW-DC1

                    Last attempt @ 2026-07-09 14:25:00 was successful.
                    """
            };

        var context = CreateContext();

        var ingestor =
            new ReplicationReportIngestor(
                new ReplicationOutputParser());

        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    ingestor.Ingest(
                        report,
                        context));

        Assert.Contains(
            "when it was collected",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Ingest_ValidReplicationReport_ReturnsReplicationRecord()
    {
        var collectedUtc =
            new DateTimeOffset(
                2026,
                7,
                9,
                14,
                30,
                0,
                TimeSpan.Zero);

        var report =
            new ReportFile
            {
                ReportId = "R-004",
                Type = ReportType.Replication,
                FileName = "SFOFLEX-DC1-repadmin.txt",
                DomainController = "SFOFLEX-DC1",
                CollectedUtc = collectedUtc,
                Content =
                    """
                    Repadmin: running command /showrepl SFOFLEX-DC1

                    From server: ZUSW-DC1

                    Last attempt @ 2026-07-09 14:25:00 was successful.
                    """
            };

        var context = CreateContext();

        var ingestor =
            new ReplicationReportIngestor(
                new ReplicationOutputParser());

        var records =
            ingestor.Ingest(
                report,
                context);

        var record =
            Assert.Single(records);

        Assert.Equal(
            "SFOFLEX-DC1",
            record.SourceDomainController);

        Assert.Equal(
            "ZUSW-DC1",
            record.PartnerDomainController);

        Assert.Equal(
            "SFO",
            record.SourceSite);

        Assert.True(
            record.Success);

        Assert.Null(
            record.ErrorCode);

        Assert.Equal(
            collectedUtc,
            record.CollectedUtc);
    }

    [Fact]
    public void Ingest_RepadminShowReplWithMixedPartnerResults_ParsesFailuresAndSuccesses()
    {
        var collectedUtc =
            new DateTimeOffset(
                2026,
                7,
                9,
                14,
                30,
                0,
                TimeSpan.Zero);

        var report =
            new ReportFile
            {
                ReportId = "R-006",
                Type = ReportType.Replication,
                FileName = "SFOFLEX-DC1-repadmin.txt",
                DomainController = "SFOFLEX-DC1",
                CollectedUtc = collectedUtc,
                Content =
                    """
                    Repadmin: running command /showrepl against full DC localhost

                    Default-First-Site-Name\SFOFLEX-DC1
                    DSA Options: IS_GC
                    Site Options: (none)

                    ==== INBOUND NEIGHBORS ======================================

                    DC=apcflex,DC=aero
                        Default-First-Site-Name\ZUSW-DC1
                        From server: ZUSW-DC1
                        Last attempt @ 2026-07-09 12:00:00 was unsuccessful
                        1722 (0x6ba)
                        The RPC server is unavailable.

                        Last success @ 2026-07-09 11:45:00

                    DC=apcflex,DC=aero
                        Default-First-Site-Name\LAXFLEX-DC1
                        From server: LAXFLEX-DC1
                        Last attempt @ 2026-07-09 12:00:00 was successful
                        0 (0x0)
                    """
            };

        var context = CreateContext();

        var ingestor =
            new ReplicationReportIngestor(
                new ReplicationOutputParser());

        var records =
            ingestor.Ingest(
                report,
                context);

        Assert.Equal(
            2,
            records.Count);

        var zuswRecord =
            Assert.Single(
                records,
                x =>
                    x.PartnerDomainController ==
                    "ZUSW-DC1");

        Assert.Equal(
            "SFOFLEX-DC1",
            zuswRecord.SourceDomainController);

        Assert.False(
            zuswRecord.Success);

        Assert.Equal(
            1722,
            zuswRecord.ErrorCode);

        Assert.Contains(
            "RPC server is unavailable",
            zuswRecord.ErrorMessage);

        Assert.Equal(
            collectedUtc,
            zuswRecord.CollectedUtc);

        var laxRecord =
            Assert.Single(
                records,
                x =>
                    x.PartnerDomainController ==
                    "LAXFLEX-DC1");

        Assert.Equal(
            "SFOFLEX-DC1",
            laxRecord.SourceDomainController);

        Assert.True(
            laxRecord.Success);

        Assert.Equal(
            0,
            laxRecord.ErrorCode);

        Assert.Equal(
            collectedUtc,
            laxRecord.CollectedUtc);
    }

    [Fact]
    public void Ingest_UsesReportCollectionTimestamp_NotParserTimestamp()
    {
        var collectedUtc =
            new DateTimeOffset(
                2026,
                7,
                9,
                14,
                30,
                0,
                TimeSpan.Zero);

        var report =
            new ReportFile
            {
                ReportId = "R-005",
                Type = ReportType.Replication,
                FileName = "SFOFLEX-DC1-repadmin.txt",
                DomainController = "SFOFLEX-DC1",
                CollectedUtc = collectedUtc,
                Content =
                    """
                    Repadmin: running command /showrepl SFOFLEX-DC1

                    From server: ZUSW-DC1

                    Last attempt @ 2026-07-09 14:25:00 was successful.
                    """
            };

        var context = CreateContext();

        var ingestor =
            new ReplicationReportIngestor(
                new ReplicationOutputParser());

        var records =
            ingestor.Ingest(
                report,
                context);

        var record =
            Assert.Single(records);

        Assert.Equal(
            collectedUtc,
            record.CollectedUtc);
    }

    private static CollectorContext CreateContext()
    {
        return new CollectorContext
        {
            InvestigationId =
                "INC-SFO-20260709",

            Site =
                "SFO",

            DomainName =
                "apcflex.aero",

            DomainControllers =
                ["SFOFLEX-DC1"]
        };
    }
}