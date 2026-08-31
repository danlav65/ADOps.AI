using ADOps.Core.Entities;
using ADOps.Core.Entities.Reporting;
using ADOps.Core.Interfaces;
using ADOps.Infrastructure.Ingestion.SystemInfo;

namespace ADOps.Infrastructure.Tests;

public sealed class SystemInfoReportIngestorTests
{
    [Fact]
    public void Ingest_ArrayReport_ProducesSystemInfoRecords()
    {
        var report =
            new ReportFile
            {
                ReportId = "SYS-001",
                Type = ReportType.SystemInfo,
                FileName = "systeminfo-sfo.json",
                Content =
                    """
                    [
                      {
                        "DomainController": "SFOFLEX-DC1",
                        "Site": "SFO",
                        "ComputerName": "SFOFLEX-DC1",
                        "OperatingSystem": "Windows Server 2019",
                        "OsVersion": "10.0",
                        "BuildNumber": "17763",
                        "Edition": "Standard",
                        "Architecture": "x64",
                        "TimeZone": "UTC",
                        "LogicalProcessors": 8,
                        "PhysicalMemoryGb": 32,
                        "SystemDriveFreeSpaceGb": 2.1,
                        "VirtualMachine": true,
                        "Hypervisor": "Azure",
                        "PowerShellVersion": "7.5",
                        "DotNetVersion": "10.0"
                      }
                    ]
                    """,
                DomainController = "SFOFLEX-DC1",
                CollectedUtc =
                    new DateTimeOffset(
                        2026,
                        7,
                        9,
                        12,
                        0,
                        0,
                        TimeSpan.Zero)
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
            new SystemInfoReportIngestor();

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
            "SFOFLEX-DC1",
            record.ComputerName);

        Assert.Equal(
            "Windows Server 2019",
            record.OperatingSystem);

        Assert.Equal(
            "10.0",
            record.OsVersion);

        Assert.Equal(
            "17763",
            record.BuildNumber);

        Assert.Equal(
            "Standard",
            record.Edition);

        Assert.Equal(
            "x64",
            record.Architecture);

        Assert.Equal(
            8,
            record.LogicalProcessors);

        Assert.Equal(
            32,
            record.PhysicalMemoryGb);

        Assert.Equal(
            2.1,
            record.SystemDriveFreeSpaceGb);

        Assert.True(
            record.VirtualMachine);

        Assert.Equal(
            "Azure",
            record.Hypervisor);

        Assert.Equal(
            "7.5",
            record.PowerShellVersion);

        Assert.Equal(
            "10.0",
            record.DotNetVersion);
    }

    [Fact]
    public void Ingest_SingleObject_ProducesOneRecord()
    {
        var report =
            new ReportFile
            {
                ReportId = "SYS-002",
                Type = ReportType.SystemInfo,
                FileName = "systeminfo.json",
                Content =
                    """
                    {
                      "ComputerName": "ZUSW-DC1",
                      "Site": "ZUSW",
                      "OperatingSystem": "Windows Server 2019",
                      "OsVersion": "10.0",
                      "BuildNumber": "17763",
                      "Edition": "Standard",
                      "Architecture": "x64",
                      "PhysicalMemoryGb": 32,
                      "SystemDriveFreeSpaceGb": 80,
                      "VirtualMachine": true,
                      "Hypervisor": "Azure",
                      "PowerShellVersion": "7.5",
                      "DotNetVersion": "10.0"
                    }
                    """,
                DomainController = "ZUSW-DC1"
            };

        var context =
            new CollectorContext
            {
                InvestigationId = "INV-SITA-20260709",
                Site = "SFO",
                DomainControllers =
                    ["ZUSW-DC1"]
            };

        var ingestor =
            new SystemInfoReportIngestor();

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
            80,
            record.SystemDriveFreeSpaceGb);
    }

    [Fact]
    public void Ingest_UsesReportDomainController_WhenRecordDoesNotContainOne()
    {
        var report =
            new ReportFile
            {
                ReportId = "SYS-003",
                Type = ReportType.SystemInfo,
                FileName = "systeminfo.json",
                Content =
                    """
                    {
                      "ComputerName": "SFOFLEX-DC1",
                      "Site": "SFO",
                      "OperatingSystem": "Windows Server 2019"
                    }
                    """,
                DomainController = "SFOFLEX-DC1"
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
            new SystemInfoReportIngestor();

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
                ReportId = "PATCH-001",
                Type = ReportType.Patch,
                FileName = "patch.json",
                Content = "{}",
                DomainController = "SFOFLEX-DC1"
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
            new SystemInfoReportIngestor();

        Assert.Throws<ArgumentException>(
            () =>
                ingestor.Ingest(
                    report,
                    context));
    }
}
