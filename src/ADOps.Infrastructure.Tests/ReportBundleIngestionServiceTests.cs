using ADOps.Core.Entities;
using ADOps.Core.Interfaces;
using EvidenceEntity = ADOps.Core.Entities.Evidence;
using ADOps.Core.Entities.Replication;
using ADOps.Core.Entities.Reporting;
using ADOps.Infrastructure.Evidence;
using ADOps.Infrastructure.Ingestion.Patch;
using ADOps.Infrastructure.Ingestion.Replication;
using ADOps.Infrastructure.Ingestion.Reporting;
using ADOps.Infrastructure.Ingestion.SystemInfo;
using ADOps.Infrastructure.Ingestion.Rpc;

namespace ADOps.Infrastructure.Tests;

public sealed class ReportBundleIngestionServiceTests
{
    [Fact]
    public async Task IngestAsync_ValidBundle_Succeeds()
    {
        var validator = new ReportBundleValidator();

        var replication =
            new FakeReplicationReportIngestor();

        var patch =
            new FakePatchReportIngestor();

        var systemInfo =
            new FakeSystemInfoReportIngestor();

        var evidence =
            new FakeEvidenceNormalizer();

        var service =
            CreateService(
                validator,
                replication,
                patch,
                systemInfo,
                evidence);

        var bundle = CreateValidBundle();

        var result =
            await service.IngestAsync(bundle);

        Assert.True(result.Succeeded);
        Assert.Equal(
            bundle.BundleId,
            result.BundleId);

        Assert.Equal(
            bundle.InvestigationId,
            result.InvestigationId);

        Assert.Equal(2, result.ReportCount);
        Assert.Equal(2, result.AcceptedReportCount);
        Assert.Equal(0, result.RejectedReportCount);

        Assert.NotNull(result.Snapshot);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task IngestAsync_DispatchesReportsToCorrectIngestors()
    {
        var replication =
            new FakeReplicationReportIngestor();

        var patch =
            new FakePatchReportIngestor();

        var systemInfo =
            new FakeSystemInfoReportIngestor();

        var evidence =
            new FakeEvidenceNormalizer();

        var service =
            CreateService(
                new ReportBundleValidator(),
                replication,
                patch,
                systemInfo,
                evidence);

        var bundle =
            CreateThreeReportBundle();

        var result =
            await service.IngestAsync(bundle);

        Assert.True(result.Succeeded);

        Assert.Equal(
            ["R-001"],
            replication.IngestedReportIds);

        Assert.Equal(
            ["R-002"],
            patch.IngestedReportIds);

        Assert.Equal(
            ["R-003"],
            systemInfo.IngestedReportIds);
    }

    [Fact]
    public async Task IngestAsync_RpcReport_DispatchesToRpcIngestor()
    {
        var rpc =
            new FakeRpcReportIngestor(
                CreateRpcRecord());

        var service =
            CreateService(
                rpc: rpc);

        var bundle =
            CreateValidBundle();

        bundle.Reports.Add(
            new ReportFile
            {
                ReportId = "R-003",
                Type = ReportType.Rpc,
                FileName = "rpc.txt",
                Content = "rpc test",
                DomainController = "SFOFLEX-DC1",
                CollectedUtc = bundle.CreatedUtc
            });

        var result =
            await service.IngestAsync(bundle);

        Assert.True(result.Succeeded);

        Assert.Equal(
            ["R-003"],
            rpc.IngestedReportIds);

        Assert.NotNull(result.Snapshot);

        var snapshot =
            result.Snapshot!;

        Assert.Single(snapshot.Rpc);

        var record =
            snapshot.Rpc[0];

        Assert.Equal(
            "SFOFLEX-DC1",
            record.DomainController);

        Assert.Equal(
            "ZUSW-DC1",
            record.Target);

        Assert.False(
            record.Success);
    }

    [Fact]
    public async Task IngestAsync_PopulatesSnapshotWithIngestedRecords()
    {
        var replicationRecord =
            CreateReplicationRecord();

        var patchRecord =
            CreatePatchRecord();

        var systemInfoRecord =
            CreateSystemInfoRecord();

        var service =
            CreateService(
                new ReportBundleValidator(),
                new FakeReplicationReportIngestor(
                    replicationRecord),
                new FakePatchReportIngestor(
                    patchRecord),
                new FakeSystemInfoReportIngestor(
                    systemInfoRecord),
                new FakeEvidenceNormalizer());

        var result =
            await service.IngestAsync(
                CreateThreeReportBundle());

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Snapshot);

        Assert.Single(
            result.Snapshot!.Replication);

        Assert.Same(
            replicationRecord,
            result.Snapshot.Replication[0]);

        Assert.Single(
            result.Snapshot.Patches);

        Assert.Same(
            patchRecord,
            result.Snapshot.Patches[0]);

        Assert.Single(
            result.Snapshot.SystemInfo);

        Assert.Same(
            systemInfoRecord,
            result.Snapshot.SystemInfo[0]);
    }

    [Fact]
    public async Task IngestAsync_NormalizesEvidenceAfterIngestion()
    {
        var evidenceRecord =
            new EvidenceEntity
            {
                EvidenceId = "EV-TEST-001",
                InvestigationId = "INC-SFO-20260709",
                Type =
                    EvidenceType.InfrastructureHealth,
                Source = "TestNormalizer",
                Target = "SFOFLEX-DC1",
                CollectedUtc =
                    new DateTimeOffset(
                        2026,
                        7,
                        9,
                        12,
                        0,
                        0,
                        TimeSpan.Zero),
                Summary = "Test evidence"
            };

        var normalizer =
            new FakeEvidenceNormalizer(
                evidenceRecord);

        var service =
            CreateService(
                new ReportBundleValidator(),
                new FakeReplicationReportIngestor(),
                new FakePatchReportIngestor(),
                new FakeSystemInfoReportIngestor(),
                normalizer);

        var result =
            await service.IngestAsync(
                CreateValidBundle());

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Snapshot);

        Assert.True(
            normalizer.WasCalled);

        Assert.Single(
            result.Snapshot!.Evidence);

        Assert.Same(
            evidenceRecord,
            result.Snapshot.Evidence[0]);
    }

    [Fact]
    public async Task IngestAsync_ReportFailure_DoesNotDiscardSuccessfulReports()
    {
        var replication =
            new FakeReplicationReportIngestor(
                throwOnIngest: true);

        var patchRecord =
            CreatePatchRecord();

        var service =
            CreateService(
                new ReportBundleValidator(),
                replication,
                new FakePatchReportIngestor(
                    patchRecord),
                new FakeSystemInfoReportIngestor(),
                new FakeEvidenceNormalizer());

        var result =
            await service.IngestAsync(
                CreateValidBundle());

        Assert.False(result.Succeeded);

        Assert.Equal(2, result.ReportCount);
        Assert.Equal(1, result.AcceptedReportCount);
        Assert.Equal(1, result.RejectedReportCount);

        Assert.NotNull(result.Snapshot);

        Assert.Single(
            result.Snapshot!.Patches);

        Assert.Empty(
            result.Snapshot.Replication);

        Assert.Contains(
            result.Errors,
            error =>
                error.Contains(
                    "R-001",
                    StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task IngestAsync_MissingBundleId_RejectsBundle()
    {
        var service =
            CreateService();

        var bundle =
            CreateValidBundle(
                bundleId: "");

        var result =
            await service.IngestAsync(bundle);

        Assert.False(result.Succeeded);

        Assert.Contains(
            "BundleId is required.",
            result.Errors);

        Assert.Null(result.Snapshot);
    }

    [Fact]
    public async Task IngestAsync_MissingInvestigationId_RejectsBundle()
    {
        var service =
            CreateService();

        var bundle =
            CreateValidBundle(
                investigationId: "");

        var result =
            await service.IngestAsync(bundle);

        Assert.False(result.Succeeded);

        Assert.Contains(
            "InvestigationId is required.",
            result.Errors);

        Assert.Null(result.Snapshot);
    }

    [Fact]
    public async Task IngestAsync_MissingSite_RejectsBundle()
    {
        var service =
            CreateService();

        var bundle =
            CreateValidBundle(
                site: "");

        var result =
            await service.IngestAsync(bundle);

        Assert.False(result.Succeeded);

        Assert.Contains(
            "Site is required.",
            result.Errors);

        Assert.Null(result.Snapshot);
    }

    [Fact]
    public async Task IngestAsync_EmptyReports_RejectsBundle()
    {
        var service =
            CreateService();

        var bundle = new ReportBundle
        {
            BundleId = "BUNDLE-001",
            InvestigationId =
                "INC-SFO-20260709",
            Site = "SFO"
        };

        var result =
            await service.IngestAsync(bundle);

        Assert.False(result.Succeeded);
        Assert.Equal(0, result.ReportCount);

        Assert.Contains(
            "Report bundle must contain at least one report.",
            result.Errors);

        Assert.Null(result.Snapshot);
    }

    [Fact]
    public async Task IngestAsync_UnknownReportType_RejectsBundle()
    {
        var service =
            CreateService();

        var bundle =
            CreateValidBundle();

        bundle.Reports.Add(
            new ReportFile
            {
                ReportId = "R-003",
                Type = ReportType.Unknown,
                FileName = "unknown.txt",
                Content = "test content"
            });

        var result =
            await service.IngestAsync(bundle);

        Assert.False(result.Succeeded);

        Assert.Contains(
            result.Errors,
            error =>
                error.Contains(
                    "unknown report type",
                    StringComparison.OrdinalIgnoreCase));

        Assert.Null(result.Snapshot);
    }

    [Fact]
    public async Task IngestAsync_EmptyReportContent_RejectsBundle()
    {
        var service =
            CreateService();

        var bundle =
            CreateValidBundle();

        bundle.Reports.Add(
            new ReportFile
            {
                ReportId = "R-003",
                Type = ReportType.SystemInfo,
                FileName = "systeminfo.txt",
                Content = ""
            });

        var result =
            await service.IngestAsync(bundle);

        Assert.False(result.Succeeded);

        Assert.Contains(
            result.Errors,
            error =>
                error.Contains(
                    "empty content",
                    StringComparison.OrdinalIgnoreCase));

        Assert.Null(result.Snapshot);
    }

    [Fact]
    public async Task IngestAsync_DuplicateReportId_RejectsBundle()
    {
        var service =
            CreateService();

        var bundle =
            CreateValidBundle();

        bundle.Reports.Add(
            new ReportFile
            {
                ReportId = "R-001",
                Type = ReportType.SystemInfo,
                FileName = "systeminfo.txt",
                Content = "test content"
            });

        var result =
            await service.IngestAsync(bundle);

        Assert.False(result.Succeeded);

        Assert.Contains(
            result.Errors,
            error =>
                error.Contains(
                    "Duplicate ReportId",
                    StringComparison.OrdinalIgnoreCase));

        Assert.Null(result.Snapshot);
    }

    [Fact]
    public async Task IngestAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        var service =
            CreateService();

        var bundle =
            CreateValidBundle();

        using var cancellationTokenSource =
            new CancellationTokenSource();

        cancellationTokenSource.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () =>
                service.IngestAsync(
                    bundle,
                    cancellationTokenSource.Token));
    }

    private static ReportBundleIngestionService CreateService(
        ReportBundleValidator? validator = null,
        FakeReplicationReportIngestor? replication = null,
        FakePatchReportIngestor? patch = null,
        FakeSystemInfoReportIngestor? systemInfo = null,
        FakeEvidenceNormalizer? evidence = null,
        FakeRpcReportIngestor? rpc = null)
        
    {
        return new ReportBundleIngestionService(
            validator ?? new ReportBundleValidator(),
            replication ??
                new FakeReplicationReportIngestor(),
            patch ??
                new FakePatchReportIngestor(),
            systemInfo ??
                new FakeSystemInfoReportIngestor(),
            rpc ??
                new FakeRpcReportIngestor(),
            evidence ??
                new FakeEvidenceNormalizer());
    }

    private static ReportBundle CreateValidBundle(
        string bundleId = "BUNDLE-001",
        string investigationId =
            "INC-SFO-20260709",
        string site = "SFO")
    {
        var bundle = new ReportBundle
        {
            BundleId = bundleId,
            InvestigationId = investigationId,
            Site = site,
            CreatedUtc =
                new DateTimeOffset(
                    2026,
                    7,
                    9,
                    12,
                    0,
                    0,
                    TimeSpan.Zero)
        };

        bundle.Reports.Add(
            new ReportFile
            {
                ReportId = "R-001",
                Type = ReportType.Replication,
                FileName = "replication.txt",
                Content = "replication test",
                DomainController = "SFOFLEX-DC1",
                CollectedUtc =
                    bundle.CreatedUtc
            });

        bundle.Reports.Add(
            new ReportFile
            {
                ReportId = "R-002",
                Type = ReportType.Patch,
                FileName = "patch.json",
                Content = "patch test",
                DomainController = "SFOFLEX-DC1",
                CollectedUtc =
                    bundle.CreatedUtc
            });

        return bundle;
    }

    private static ReportBundle CreateThreeReportBundle()
    {
        var bundle =
            CreateValidBundle();

        bundle.Reports.Add(
            new ReportFile
            {
                ReportId = "R-003",
                Type = ReportType.SystemInfo,
                FileName = "systeminfo.json",
                Content = "system info test",
                DomainController = "SFOFLEX-DC1",
                CollectedUtc =
                    bundle.CreatedUtc
            });

        return bundle;
    }

    private static ReplicationRecord CreateReplicationRecord()
    {
        return new ReplicationRecord
        {
            SourceDomainController =
                "SFOFLEX-DC1",
            PartnerDomainController =
                "ZUSW-DC1",
            SourceSite = "SFO",
            PartnerSite = "ZUSW",
            Success = false,
            ErrorCode = 1722,
            ErrorMessage =
                "RPC server unavailable",
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
    }

    private static RpcRecord CreateRpcRecord()
    {
        return new RpcRecord
        {
            DomainController = "SFOFLEX-DC1",
            Target = "ZUSW-DC1",
            SourceAddress = "10.10.1.10",
            RemoteAddress = "10.20.1.10",
            RemotePort = 135,
            InterfaceAlias = "Ethernet",
            Success = false,
            ErrorCode = 1722,
            ErrorMessage = "RPC server unavailable",
            CollectedUtc =
                new DateTimeOffset(
                    2026,
                    7,
                    9,
                    12,
                    0,
                    0,
                    TimeSpan.Zero),
            SourceCommand = "Test-NetConnection"
        };
    }
    
    private static PatchRecord CreatePatchRecord()
    {
        return new PatchRecord
        {
            DomainController =
                "SFOFLEX-DC1",
            Site = "SFO",
            OperatingSystem =
                "Windows Server 2019",
            OsBuild = "17763",
            KnowledgeBaseArticle =
                "KB5060531",
            PatchVersion = "Test",
            Installed = false,
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
    }

    private static SystemInfoRecord CreateSystemInfoRecord()
    {
        return new SystemInfoRecord
        {
            DomainController =
                "SFOFLEX-DC1",
            Site = "SFO",
            ComputerName =
                "SFOFLEX-DC1",
            OperatingSystem =
                "Windows Server 2019",
            OsVersion = "10.0",
            BuildNumber = "17763",
            Edition = "Standard",
            Architecture = "x64",
            TimeZone = "UTC",
            LogicalProcessors = 4,
            PhysicalMemoryGb = 16,
            SystemDriveFreeSpaceGb = 100,
            VirtualMachine = true,
            PowerShellVersion = "7",
            DotNetVersion = "10",
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
    }

    private sealed class FakeReplicationReportIngestor
        : IReplicationReportIngestor
    {
        private readonly IReadOnlyCollection<ReplicationRecord>
            _records;

        private readonly bool _throwOnIngest;

        public List<string> IngestedReportIds { get; } = [];

        public FakeReplicationReportIngestor(
            ReplicationRecord? record = null,
            bool throwOnIngest = false)
        {
            _records =
                record is null
                    ? []
                    : [record];

            _throwOnIngest = throwOnIngest;
        }

        public IReadOnlyCollection<ReplicationRecord> Ingest(
            ReportFile report,
            CollectorContext context)
        {
            IngestedReportIds.Add(
                report.ReportId);

            if (_throwOnIngest)
            {
                throw new InvalidOperationException(
                    "Simulated replication ingestion failure.");
            }

            return _records;
        }
    }

    private sealed class FakePatchReportIngestor
        : IPatchReportIngestor
    {
        private readonly IReadOnlyCollection<PatchRecord>
            _records;

        public List<string> IngestedReportIds { get; } = [];

        public FakePatchReportIngestor(
            PatchRecord? record = null)
        {
            _records =
                record is null
                    ? []
                    : [record];
        }

        public IReadOnlyCollection<PatchRecord> Ingest(
            ReportFile report,
            CollectorContext context)
        {
            IngestedReportIds.Add(
                report.ReportId);

            return _records;
        }
    }

    private sealed class FakeSystemInfoReportIngestor
        : ISystemInfoReportIngestor
    {
        private readonly IReadOnlyCollection<SystemInfoRecord>
            _records;

        public List<string> IngestedReportIds { get; } = [];

        public FakeSystemInfoReportIngestor(
            SystemInfoRecord? record = null)
        {
            _records =
                record is null
                    ? []
                    : [record];
        }

        public IReadOnlyCollection<SystemInfoRecord> Ingest(
            ReportFile report,
            CollectorContext context)
        {
            IngestedReportIds.Add(
                report.ReportId);

            return _records;
        }
    }

    private sealed class FakeRpcReportIngestor
    : IRpcReportIngestor
    {
    private readonly IReadOnlyCollection<RpcRecord>
        _records;

    public List<string> IngestedReportIds { get; } = [];

    public FakeRpcReportIngestor(
        RpcRecord? record = null)
    {
        _records =
            record is null
                ? []
                : [record];
    }

    public IReadOnlyCollection<RpcRecord> Ingest(
        ReportFile report,
        CollectorContext context)
    {
        IngestedReportIds.Add(
            report.ReportId);

        return _records;
    }
}

    private sealed class FakeEvidenceNormalizer
        : IEvidenceNormalizer
    {
        private readonly IReadOnlyCollection<EvidenceEntity>
            _evidence;

        public bool WasCalled { get; private set; }

        public FakeEvidenceNormalizer(
            EvidenceEntity? evidence = null)
        {
            _evidence =
                evidence is null
                    ? []
                    : [evidence];
        }

        public IReadOnlyCollection<EvidenceEntity> Normalize(
            InvestigationSnapshot snapshot)
        {
            WasCalled = true;

            return _evidence;
        }
    }
}
