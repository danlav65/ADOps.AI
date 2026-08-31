using Microsoft.Extensions.DependencyInjection;
using ADOps.Core.Interfaces;
using ADOps.Infrastructure;
using ADOps.Core.Entities;
using ADOps.Core.Entities.Reporting;
using ADOps.Infrastructure.Analysis;
using ADOps.Infrastructure.Correlation;
using ADOps.Infrastructure.Recommendations;

Console.WriteLine("================================================");
Console.WriteLine("ADOps AI Test Harness");
Console.WriteLine("================================================");

var failures = new List<string>();

var services = new ServiceCollection();

services.AddInfrastructure();

using var serviceProvider =
    services.BuildServiceProvider();

using var scope =
    serviceProvider.CreateScope();

var reportBundleIngestionService =
    scope.ServiceProvider.GetRequiredService<
        IReportBundleIngestionService>();

Console.WriteLine(
    "Report bundle ingestion service resolved successfully.");

void Assert(
    bool condition,
    string description)
{
    if (condition)
    {
        Console.WriteLine($"PASS: {description}");
    }
    else
    {
        Console.WriteLine($"FAIL: {description}");
        failures.Add(description);
    }
}

var investigationId = "INC-SFO-20260709";

var bundlePath =
    Path.Combine(
        "test-data",
        "report-bundles",
        "SFO-20260709");

var bundle = new ReportBundle
{
    BundleId = "BUNDLE-SFO-20260709",
    InvestigationId = investigationId,
    Site = "SFO",
    CreatedUtc = new DateTimeOffset(
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
        Content =
            File.ReadAllText(
                Path.Combine(
                    bundlePath,
                    "replication.txt")),
        DomainController = "SFOFLEX-DC1",
        CollectedUtc = bundle.CreatedUtc
    });

bundle.Reports.Add(
    new ReportFile
    {
        ReportId = "R-005",
        Type = ReportType.Replication,
        FileName = "replication-zusw.txt",
        Content =
            File.ReadAllText(
                Path.Combine(
                    bundlePath,
                    "replication-zusw.txt")),
        DomainController = "ZUSW-DC1",
        CollectedUtc = bundle.CreatedUtc
    });

bundle.Reports.Add(
    new ReportFile
    {
        ReportId = "R-002",
        Type = ReportType.Patch,
        FileName = "patch.json",
        Content =
            File.ReadAllText(
                Path.Combine(
                    bundlePath,
                    "patch.json")),
        DomainController = "SFOFLEX-DC1",
        CollectedUtc = bundle.CreatedUtc
    });

bundle.Reports.Add(
    new ReportFile
    {
        ReportId = "R-003",
        Type = ReportType.SystemInfo,
        FileName = "systeminfo.json",
        Content =
            File.ReadAllText(
                Path.Combine(
                    bundlePath,
                    "systeminfo.json")),
        DomainController = "SFOFLEX-DC1",
        CollectedUtc = bundle.CreatedUtc
    });

bundle.Reports.Add(
    new ReportFile
    {
        ReportId = "R-004",
        Type = ReportType.Rpc,
        FileName = "rpc.json",
        Content =
            File.ReadAllText(
                Path.Combine(
                    bundlePath,
                    "rpc.json")),
        DomainController = "SFOFLEX-DC1",
        CollectedUtc = bundle.CreatedUtc
    });

Console.WriteLine();
Console.WriteLine("Report Bundle Ingestion");
Console.WriteLine("--------------------------------");

var ingestionResult =
    await reportBundleIngestionService.IngestAsync(
        bundle);

Console.WriteLine(
    $"Bundle: {ingestionResult.BundleId}");

Console.WriteLine(
    $"Reports: {ingestionResult.ReportCount}");

Console.WriteLine(
    $"Accepted: {ingestionResult.AcceptedReportCount}");

Console.WriteLine(
    $"Rejected: {ingestionResult.RejectedReportCount}");

Assert(
    ingestionResult.Succeeded,
    "Report bundle ingestion succeeds.");

Assert(
    ingestionResult.ReportCount == 5,
    "Five reports are present in the report bundle.");

Assert(
    ingestionResult.AcceptedReportCount == 5,
    "All five reports are accepted.");

Assert(
    ingestionResult.RejectedReportCount == 0,
    "No reports are rejected.");

Assert(
    ingestionResult.Snapshot is not null,
    "Investigation snapshot is produced.");

var snapshot =
    ingestionResult.Snapshot!;

Console.WriteLine();
Console.WriteLine("Snapshot");
Console.WriteLine("--------------------------------");

Console.WriteLine(
    $"Replication records: {snapshot.Replication.Count}");

Console.WriteLine(
    $"Patch records: {snapshot.Patches.Count}");

Console.WriteLine(
    $"SystemInfo records: {snapshot.SystemInfo.Count}");

Console.WriteLine(
    $"RPC records: {snapshot.Rpc.Count}");

Console.WriteLine(
    $"Evidence records: {snapshot.Evidence.Count}");

Console.WriteLine(
    $"Topology relationships: {snapshot.Topology?.ReplicationPartners.Count ?? 0}");

Assert(
    snapshot.Replication.Count == 3,
    "Three replication records are ingested.");

Assert(
    snapshot.Patches.Count == 2,
    "Two patch records are ingested.");

Assert(
    snapshot.SystemInfo.Count == 2,
    "Two system information records are ingested.");

Assert(
    snapshot.Rpc.Count == 1,
    "One RPC record is ingested.");

Assert(
    snapshot.Evidence.Count == 8,
    "Eight normalized evidence records are produced.");

Assert(
    snapshot.Topology?.ReplicationPartners.Count == 3,
    "Three replication topology relationships are produced.");

Console.WriteLine();
Console.WriteLine("Correlation");
Console.WriteLine("--------------------------------");

Console.WriteLine();
Console.WriteLine("Topology Diagnostics");
Console.WriteLine("--------------------------------");

foreach (var relationship in
         snapshot.Topology?.ReplicationPartners ?? [])
{
    Console.WriteLine(
        $"{relationship.SourceDomainController} -> " +
        $"{relationship.PartnerDomainController}");
}

Console.WriteLine();
Console.WriteLine("Replication Evidence Diagnostics");
Console.WriteLine("--------------------------------");

foreach (var evidence in snapshot.Evidence.Where(e =>
             e.Type == EvidenceType.ReplicationFailure ||
             e.Type == EvidenceType.ReplicationSuccess))
{
    Console.WriteLine(
        $"{evidence.Type}: " +
        $"{evidence.Source} -> {evidence.Target}");
}

Console.WriteLine();
Console.WriteLine("Patch Evidence Diagnostics");
Console.WriteLine("--------------------------------");

foreach (var evidence in snapshot.Evidence.Where(e =>
             e.Type == EvidenceType.Patch))
{
    Console.WriteLine(
        $"{evidence.Target}: {evidence.Summary}");
}

var correlationEngine = new CorrelationEngine();

var findings =
    correlationEngine.Correlate(
        snapshot.Evidence,
        snapshot.Topology
            ?? new TopologyContext
            {
                ReplicationPartners = []
            });

Console.WriteLine($"Correlation findings: {findings.Count}");

foreach (var finding in findings)
{
    Console.WriteLine("--------------------------------");
    Console.WriteLine(finding.CorrelationType);
    Console.WriteLine(finding.Summary);
    Console.WriteLine($"Confidence: {finding.Confidence:F2}");
    Console.WriteLine(
        $"Evidence: {string.Join(", ", finding.EvidenceIds)}");
}

var replicationFailureEvidence =
    snapshot.Evidence.FirstOrDefault(e =>
        e.Type == EvidenceType.ReplicationFailure &&
        e.Source == "SFOFLEX-DC1" &&
        e.Target == "ZUSW-DC1");

var rpcFailureEvidence =
    snapshot.Evidence.FirstOrDefault(e =>
        e.Type == EvidenceType.RpcFailure &&
        e.Source == "SFOFLEX-DC1" &&
        e.Target == "ZUSW-DC1");

var infrastructureEvidence =
    snapshot.Evidence.FirstOrDefault(e =>
        e.Type == EvidenceType.InfrastructureHealth &&
        e.Target == "SFOFLEX-DC1");

var patchEvidence =
    snapshot.Evidence.FirstOrDefault(e =>
        e.Type == EvidenceType.Patch &&
        e.Target == "SFOFLEX-DC1");

var healthyPartnerEvidence =
    snapshot.Evidence.FirstOrDefault(e =>
        e.Type == EvidenceType.ReplicationSuccess &&
        e.Source == "ZUSW-DC1" &&
        e.Target == "SFOFLEX-DC1");

var partnerPatchEvidence =
    snapshot.Evidence.FirstOrDefault(e =>
        e.Type == EvidenceType.Patch &&
        e.Target == "ZUSW-DC1");

Assert(
    replicationFailureEvidence is not null,
    "Replication failure evidence exists for SFOFLEX-DC1 to ZUSW-DC1.");

Assert(
    rpcFailureEvidence is not null,
    "RPC failure evidence exists for SFOFLEX-DC1 to ZUSW-DC1.");

Assert(
    infrastructureEvidence is not null,
    "Infrastructure health evidence exists for SFOFLEX-DC1.");

Assert(
    patchEvidence is not null,
    "Patch evidence exists for SFOFLEX-DC1.");

var rpcFinding =
    findings.FirstOrDefault(f =>
        f.CorrelationType == "Replication + RPC");

Assert(
    rpcFinding is not null,
    "Replication + RPC correlation exists.");

Assert(
    replicationFailureEvidence is not null &&
    rpcFailureEvidence is not null &&
    rpcFinding?.EvidenceIds.Contains(
        replicationFailureEvidence.EvidenceId) == true &&
    rpcFinding.EvidenceIds.Contains(
        rpcFailureEvidence.EvidenceId),
    "Replication + RPC correlation references the expected evidence.");

var infrastructureFinding =
    findings.FirstOrDefault(f =>
        f.CorrelationType == "Replication + Infrastructure");

Assert(
    infrastructureFinding is not null,
    "Replication + Infrastructure correlation exists.");

Assert(
    replicationFailureEvidence is not null &&
    infrastructureEvidence is not null &&
    infrastructureFinding?.EvidenceIds.Contains(
        replicationFailureEvidence.EvidenceId) == true &&
    infrastructureFinding.EvidenceIds.Contains(
        infrastructureEvidence.EvidenceId),
    "Replication + Infrastructure correlation references the expected evidence.");

var patchFinding =
    findings.FirstOrDefault(f =>
        f.CorrelationType == "Replication + Patch Baseline");

Assert(
    patchFinding is not null,
    "Replication + Patch Baseline correlation exists.");

Assert(
    replicationFailureEvidence is not null &&
    patchEvidence is not null &&
    patchFinding?.EvidenceIds.Contains(
        replicationFailureEvidence.EvidenceId) == true &&
    patchFinding.EvidenceIds.Contains(
        patchEvidence.EvidenceId),
    "Replication + Patch Baseline correlation references the expected evidence.");

Assert(
    patchFinding?.Confidence == 0.75,
    "Replication + Patch Baseline confidence is 0.75.");

var partnerPatchFinding =
    findings.FirstOrDefault(f =>
        f.CorrelationType ==
        "Replication Partner + Patch Baseline");

Assert(
    partnerPatchFinding is not null,
    "Replication Partner + Patch Baseline correlation exists.");

Assert(
    replicationFailureEvidence is not null &&
    patchEvidence is not null &&
    healthyPartnerEvidence is not null &&
    partnerPatchEvidence is not null &&
    partnerPatchFinding?.EvidenceIds.Contains(
        replicationFailureEvidence.EvidenceId) == true &&
    partnerPatchFinding.EvidenceIds.Contains(
        patchEvidence.EvidenceId) &&
    partnerPatchFinding.EvidenceIds.Contains(
        healthyPartnerEvidence.EvidenceId) &&
    partnerPatchFinding.EvidenceIds.Contains(
        partnerPatchEvidence.EvidenceId),
    "Partner correlation references the expected evidence.");

Assert(
    partnerPatchFinding?.Confidence == 0.75,
    "Partner correlation confidence is 0.75.");

Assert(
    partnerPatchFinding?.Summary.Contains(
        "ZUSW-DC1",
        StringComparison.OrdinalIgnoreCase) == true,
    "Partner correlation identifies ZUSW-DC1 as the healthy partner.");

Console.WriteLine();
Console.WriteLine("Root Cause Analysis");
Console.WriteLine("--------------------------------");

var analyzer = new RootCauseAnalyzer();

var rca =
    analyzer.Analyze(findings);

Console.WriteLine("Title:");
Console.WriteLine(rca.Title);

Console.WriteLine();
Console.WriteLine("Executive Summary:");
Console.WriteLine(rca.ExecutiveSummary);

Console.WriteLine();
Console.WriteLine("Root Cause:");
Console.WriteLine(rca.RootCause);

Console.WriteLine();
Console.WriteLine("Technical Impact:");
Console.WriteLine(rca.TechnicalImpact);

Console.WriteLine();
Console.WriteLine("Corrective Actions:");
Console.WriteLine(rca.CorrectiveActions);

Console.WriteLine();
Console.WriteLine("Preventive Actions:");
Console.WriteLine(rca.PreventiveActions);

Assert(
    rca.RootCause.Contains(
        "Patch baseline drift",
        StringComparison.OrdinalIgnoreCase),
    "RCA identifies patch baseline drift as a probable contributing factor.");

Assert(
    rca.RootCause.Contains(
        "probable contributing factor",
        StringComparison.OrdinalIgnoreCase),
    "RCA uses qualified causality language.");

Assert(
    rca.RootCause.Contains(
        "0.75",
        StringComparison.OrdinalIgnoreCase),
    "RCA includes the strongest correlation confidence.");

Assert(
    rca.RootCause.Contains("EV-000001") &&
    rca.RootCause.Contains("EV-000003"),
    "RCA identifies the supporting evidence IDs.");

Assert(
    rca.TechnicalImpact?.Contains(
        "replication",
        StringComparison.OrdinalIgnoreCase) == true,
    "RCA identifies Active Directory replication impact.");

Assert(
    rca.CorrectiveActions?.Contains(
        "RPC",
        StringComparison.OrdinalIgnoreCase) == true,
    "RCA includes RPC corrective action.");

Assert(
    rca.PreventiveActions?.Contains(
        "patch baseline",
        StringComparison.OrdinalIgnoreCase) == true,
    "RCA includes patch baseline preventive action.");

Console.WriteLine();
Console.WriteLine("Recommended Actions");
Console.WriteLine("--------------------------------");

var recommendationEngine =
    new RecommendationEngine();

var recommendations =
    recommendationEngine.Generate(
        rca,
        findings);

foreach (var recommendation in recommendations)
{
    Console.WriteLine(
        recommendation.Priority.ToString().ToUpperInvariant());

    Console.WriteLine(recommendation.Title);
    Console.WriteLine(recommendation.Description);
    Console.WriteLine();
}

Assert(
    recommendations.Count == 5,
    "Five operational recommendations are produced.");

Assert(
    recommendations.Any(r =>
        r.Title ==
        "Align domain controller patch baselines"),
    "Patch baseline recommendation exists.");

Assert(
    recommendations.Any(r =>
        r.Title ==
        "Validate RPC connectivity"),
    "RPC validation recommendation exists.");

Assert(
    recommendations.Any(r =>
        r.Title ==
        "Validate domain controller health"),
    "Domain controller health recommendation exists.");

Assert(
    recommendations.Any(r =>
        r.Title ==
        "Perform post-maintenance replication validation"),
    "Post-maintenance replication recommendation exists.");

Assert(
    recommendations.Any(r =>
        r.Title ==
        "Implement automated patch compliance monitoring"),
    "Automated patch compliance recommendation exists.");

Console.WriteLine();
Console.WriteLine("================================================");

if (failures.Count == 0)
{
    Console.WriteLine("ADOps AI PIPELINE VALIDATION: PASS");
}
else
{
    Console.WriteLine("ADOps AI PIPELINE VALIDATION: FAIL");
    Console.WriteLine();
    Console.WriteLine("Failures:");

    foreach (var failure in failures)
    {
        Console.WriteLine($"- {failure}");
    }

    Environment.ExitCode = 1;
}

Console.WriteLine("================================================");



