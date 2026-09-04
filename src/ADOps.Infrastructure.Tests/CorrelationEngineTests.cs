using ADOps.Core.Entities;
using ADOps.Infrastructure.Correlation;

using EvidenceEntity = ADOps.Core.Entities.Evidence;

namespace ADOps.Infrastructure.Tests;

public sealed class CorrelationEngineTests
{
    [Fact]
public void Correlate_IncidentWithMultipleSignals_ProducesDistinctCorrelationFindings()
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

    var replication =
        CreateEvidence(
            "E-001",
            EvidenceType.ReplicationFailure,
            "ZUSW-DC1",
            collectedUtc,
            source: "SFOFLEX-DC1");

    var rpc =
        CreateEvidence(
            "E-002",
            EvidenceType.RpcFailure,
            "ZUSW-DC1",
            collectedUtc.AddMinutes(5),
            source: "SFOFLEX-DC1");

    var infrastructure =
        CreateEvidence(
            "E-003",
            EvidenceType.InfrastructureHealth,
            "SFOFLEX-DC1",
            collectedUtc.AddMinutes(10),
            summary: "Low disk space on system drive.");

    var sourcePatch =
        CreateEvidence(
            "E-004",
            EvidenceType.Patch,
            "SFOFLEX-DC1",
            collectedUtc.AddMinutes(15),
            summary: "Patch baseline mismatch detected.",
            details: "Required June cumulative update is missing.");

    var partnerReplication =
        CreateEvidence(
            "E-005",
            EvidenceType.ReplicationSuccess,
            "ZUSW-DC1",
            collectedUtc.AddMinutes(5),
            source: "ZUSW-DC1");

    var partnerPatch =
        CreateEvidence(
            "E-006",
            EvidenceType.Patch,
            "ZUSW-DC1",
            collectedUtc.AddMinutes(15),
            summary: "Patch baseline: KB5061178");

    var engine =
        new CorrelationEngine();

    var findings =
        engine.Correlate(
            [
                replication,
                rpc,
                infrastructure,
                sourcePatch,
                partnerReplication,
                partnerPatch
            ],
            CreateTopology());

    Assert.Equal(
        4,
        findings.Count);

    Assert.Equal(
        4,
        findings
            .Select(x => x.CorrelationType)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count());

    Assert.Contains(
        findings,
        x =>
            x.CorrelationType ==
            "Replication + RPC");

    Assert.Contains(
        findings,
        x =>
            x.CorrelationType ==
            "Replication + Infrastructure");

    Assert.Contains(
        findings,
        x =>
            x.CorrelationType ==
            "Replication + Patch Baseline");

    Assert.Contains(
        findings,
        x =>
            x.CorrelationType ==
            "Replication Partner + Patch Baseline");

    var partnerFinding =
        Assert.Single(
            findings,
            x =>
                x.CorrelationType ==
                "Replication Partner + Patch Baseline");

    Assert.Equal(
        4,
        partnerFinding.EvidenceIds.Count);

    Assert.Contains(
        "E-001",
        partnerFinding.EvidenceIds);

    Assert.Contains(
        "E-005",
        partnerFinding.EvidenceIds);

    Assert.Contains(
        "E-004",
        partnerFinding.EvidenceIds);

    Assert.Contains(
        "E-006",
        partnerFinding.EvidenceIds);
}
    
    [Fact]
    public void Correlate_Throws_WhenEvidenceIsNull()
    {
        var engine =
            new CorrelationEngine();

        var exception =
            Assert.Throws<ArgumentNullException>(
                () =>
                    engine.Correlate(
                        null!,
                        CreateTopology()));

        Assert.Equal(
            "evidence",
            exception.ParamName);
    }

    [Fact]
    public void Correlate_Throws_WhenTopologyIsNull()
    {
        var engine =
            new CorrelationEngine();

        var exception =
            Assert.Throws<ArgumentNullException>(
                () =>
                    engine.Correlate(
                        [],
                        null!));

        Assert.Equal(
            "topology",
            exception.ParamName);
    }

    [Fact]
    public void Correlate_ReplicationFailureAndRpcFailure_ProducesFinding()
    {
        var collectedUtc =
            new DateTimeOffset(2026, 7, 9, 14, 30, 0, TimeSpan.Zero);

        var replication =
            CreateEvidence(
                "E-001",
                EvidenceType.ReplicationFailure,
                "ZUSW-DC1",
                collectedUtc,
                source: "SFOFLEX-DC1");

        var rpc =
            CreateEvidence(
                "E-002",
                EvidenceType.RpcFailure,
                "ZUSW-DC1",
                collectedUtc.AddMinutes(5),
                source: "SFOFLEX-DC1");

        var engine =
            new CorrelationEngine();

        var findings =
            engine.Correlate(
                [replication, rpc],
                CreateTopology());

        var finding =
            Assert.Single(findings);

        Assert.Equal(
            "Replication + RPC",
            finding.CorrelationType);

        Assert.Equal(
            0.85,
            finding.Confidence);

        Assert.Contains(
            "E-001",
            finding.EvidenceIds);

        Assert.Contains(
            "E-002",
            finding.EvidenceIds);
    }

    [Fact]
    public void Correlate_ReplicationFailureAndInfrastructureIssue_ProducesFinding()
    {
        var collectedUtc =
            new DateTimeOffset(2026, 7, 9, 14, 30, 0, TimeSpan.Zero);

        var replication =
            CreateEvidence(
                "E-001",
                EvidenceType.ReplicationFailure,
                "ZUSW-DC1",
                collectedUtc,
                source: "SFOFLEX-DC1");

        var infrastructure =
            CreateEvidence(
                "E-003",
                EvidenceType.InfrastructureHealth,
                "SFOFLEX-DC1",
                collectedUtc.AddMinutes(10),
                summary: "Low disk space on system drive.");

        var engine =
            new CorrelationEngine();

        var findings =
            engine.Correlate(
                [replication, infrastructure],
                CreateTopology());

        var finding =
            Assert.Single(findings);

        Assert.Equal(
            "Replication + Infrastructure",
            finding.CorrelationType);

        Assert.Equal(
            0.80,
            finding.Confidence);

        Assert.Contains(
            "E-001",
            finding.EvidenceIds);

        Assert.Contains(
            "E-003",
            finding.EvidenceIds);
    }

    [Fact]
    public void Correlate_ReplicationFailureAndPatchMismatch_ProducesFinding()
    {
        var collectedUtc =
            new DateTimeOffset(2026, 7, 9, 14, 30, 0, TimeSpan.Zero);

        var replication =
            CreateEvidence(
                "E-001",
                EvidenceType.ReplicationFailure,
                "ZUSW-DC1",
                collectedUtc,
                source: "SFOFLEX-DC1");

        var patch =
            CreateEvidence(
                "E-004",
                EvidenceType.Patch,
                "SFOFLEX-DC1",
                collectedUtc.AddMinutes(15),
                summary: "Patch baseline mismatch detected.",
                details: "Required June cumulative update is missing.");

        var engine =
            new CorrelationEngine();

        var findings =
            engine.Correlate(
                [replication, patch],
                CreateTopology());

        var finding =
            Assert.Single(findings);

        Assert.Equal(
            "Replication + Patch Baseline",
            finding.CorrelationType);

        Assert.Equal(
            0.75,
            finding.Confidence);

        Assert.Contains(
            "E-001",
            finding.EvidenceIds);

        Assert.Contains(
            "E-004",
            finding.EvidenceIds);
    }

    [Fact]
    public void Correlate_FailedSourceHealthyPartnerAndPatchDifference_ProducesFinding()
    {
        var collectedUtc =
            new DateTimeOffset(2026, 7, 9, 14, 30, 0, TimeSpan.Zero);

        var sourceReplication =
            CreateEvidence(
                "E-001",
                EvidenceType.ReplicationFailure,
                "ZUSW-DC1",
                collectedUtc,
                source: "SFOFLEX-DC1");

        var partnerReplication =
            CreateEvidence(
                "E-005",
                EvidenceType.ReplicationSuccess,
                "ZUSW-DC1",
                collectedUtc.AddMinutes(5),
                source: "ZUSW-DC1");

        var sourcePatch =
            CreateEvidence(
                "E-004",
                EvidenceType.Patch,
                "SFOFLEX-DC1",
                collectedUtc.AddMinutes(10),
                summary: "Patch baseline: KB5060531");

        var partnerPatch =
            CreateEvidence(
                "E-006",
                EvidenceType.Patch,
                "ZUSW-DC1",
                collectedUtc.AddMinutes(10),
                summary: "Patch baseline: KB5061178");

        var engine =
            new CorrelationEngine();

        var findings =
            engine.Correlate(
                [
                    sourceReplication,
                    partnerReplication,
                    sourcePatch,
                    partnerPatch
                ],
                CreateTopology());

        var finding =
            Assert.Single(
                findings,
                    x =>
                        x.CorrelationType ==
                        "Replication Partner + Patch Baseline");

        Assert.Equal(
            0.75,
            finding.Confidence);

        Assert.Contains(
            "E-001",
            finding.EvidenceIds);

        Assert.Contains(
            "E-005",
            finding.EvidenceIds);

        Assert.Contains(
            "E-004",
            finding.EvidenceIds);

        Assert.Contains(
            "E-006",
            finding.EvidenceIds);
    }

    [Fact]
    public void Correlate_EvidenceOutsideCorrelationWindow_DoesNotProduceFinding()
    {
        var replicationTime =
            new DateTimeOffset(2026, 7, 9, 14, 30, 0, TimeSpan.Zero);

        var rpcTime =
            replicationTime.AddHours(25);

        var replication =
            CreateEvidence(
                "E-001",
                EvidenceType.ReplicationFailure,
                "SFOFLEX-DC1",
                replicationTime);

        var rpc =
            CreateEvidence(
                "E-002",
                EvidenceType.RpcFailure,
                "SFOFLEX-DC1",
                rpcTime);

        var engine =
            new CorrelationEngine();

        var findings =
            engine.Correlate(
                [replication, rpc],
                CreateTopology());

        Assert.Empty(findings);
    }

    private static EvidenceEntity CreateEvidence(
        string evidenceId,
        EvidenceType type,
        string target,
        DateTimeOffset collectedUtc,
        string? summary = null,
        string? details = null,
        string? source = null)
    {
        return new EvidenceEntity
        {
            EvidenceId = evidenceId,
            InvestigationId = "INC-SFO-20260709",
            Type = type,
            Source = source ?? "TestCollector",
            Target = target,
            CollectedUtc = collectedUtc,
            Summary = summary ?? type.ToString(),
            Details = details
        };
    }

    private static TopologyContext CreateTopology()
    {
        return new TopologyContext
        {
            ReplicationPartners =
            [
                new ReplicationPartnerRelationship
                {
                    SourceDomainController = "SFOFLEX-DC1",
                    PartnerDomainController = "ZUSW-DC1",
                    SourceSite = "SFO",
                    PartnerSite = "ZUSW",
                    DiscoveredUtc =
                        new DateTimeOffset(
                            2026,
                            7,
                            9,
                            14,
                            0,
                            0,
                            TimeSpan.Zero),
                    IsActive = true
                }
            ]
        };
    }
}