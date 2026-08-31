using ADOps.Core.Entities;
using ADOps.Infrastructure.Correlation;

namespace ADOps.Infrastructure.Tests.Correlation;

public sealed class CorrelationEngineTests
{
    [Fact]
    public void Correlate_ReplicationAndRpcFailure_CreatesCorrelation()
    {
        var engine =
            new CorrelationEngine();

        var replication =
            CreateEvidence(
                "EV-000001",
                EvidenceType.ReplicationFailure,
                "SFOFLEX-DC1",
                DateTimeOffset.UtcNow);

        var rpc =
            CreateEvidence(
                "EV-000002",
                EvidenceType.RpcFailure,
                "SFOFLEX-DC1",
                DateTimeOffset.UtcNow.AddMinutes(5));

        var evidence =
            new[]
            {
                replication,
                rpc
            };

        var result =
            engine.Correlate(
                evidence);

        var finding =
            Assert.Single(result);

        Assert.Equal(
            "INC-SFO-20260709",
            finding.InvestigationId);

        Assert.Equal(
            "Replication + RPC",
            finding.CorrelationType);

        Assert.Contains(
            "EV-000001",
            finding.EvidenceIds);

        Assert.Contains(
            "EV-000002",
            finding.EvidenceIds);

        Assert.Equal(
            0.85,
            finding.Confidence);
    }

    [Fact]
    public void Correlate_ReplicationAndInfrastructureIssue_CreatesCorrelation()
    {
        var engine =
            new CorrelationEngine();

        var replication =
            CreateEvidence(
                "EV-000010",
                EvidenceType.ReplicationFailure,
                "SFOFLEX-DC1",
                DateTimeOffset.UtcNow);

        var infrastructure =
            CreateEvidence(
                "EV-000011",
                EvidenceType.InfrastructureHealth,
                "SFOFLEX-DC1",
                DateTimeOffset.UtcNow.AddMinutes(10));

        var result =
            engine.Correlate(
                [
                    replication,
                    infrastructure
                ]);

        var finding =
            Assert.Single(result);

        Assert.Equal(
            "Replication + Infrastructure",
            finding.CorrelationType);

        Assert.Contains(
            "EV-000010",
            finding.EvidenceIds);

        Assert.Contains(
            "EV-000011",
            finding.EvidenceIds);
    }

    [Fact]
    public void Correlate_ReplicationAndPatchEvidence_CreatesCorrelation()
    {
        var engine =
            new CorrelationEngine();

        var replication =
            CreateEvidence(
                "EV-000020",
                EvidenceType.ReplicationFailure,
                "SFOFLEX-DC1",
                DateTimeOffset.UtcNow);

        var patch =
            CreateEvidence(
                "EV-000021",
                EvidenceType.Patch,
                "SFOFLEX-DC1",
                DateTimeOffset.UtcNow.AddHours(2));

        var result =
            engine.Correlate(
                [
                    replication,
                    patch
                ]);

        var finding =
            Assert.Single(result);

        Assert.Equal(
            "Replication + Patch",
            finding.CorrelationType);

        Assert.Equal(
            0.65,
            finding.Confidence);
    }

    [Fact]
    public void Correlate_DifferentTargets_DoesNotCreateCorrelation()
    {
        var engine =
            new CorrelationEngine();

        var replication =
            CreateEvidence(
                "EV-000030",
                EvidenceType.ReplicationFailure,
                "SFOFLEX-DC1",
                DateTimeOffset.UtcNow);

        var rpc =
            CreateEvidence(
                "EV-000031",
                EvidenceType.RpcFailure,
                "ZUSW-DC1",
                DateTimeOffset.UtcNow.AddMinutes(5));

        var result =
            engine.Correlate(
                [
                    replication,
                    rpc
                ]);

        Assert.Empty(
            result);
    }

    [Fact]
    public void Correlate_OutsideTimeWindow_DoesNotCreateCorrelation()
    {
        var engine =
            new CorrelationEngine();

        var replication =
            CreateEvidence(
                "EV-000040",
                EvidenceType.ReplicationFailure,
                "SFOFLEX-DC1",
                DateTimeOffset.UtcNow);

        var rpc =
            CreateEvidence(
                "EV-000041",
                EvidenceType.RpcFailure,
                "SFOFLEX-DC1",
                DateTimeOffset.UtcNow.AddHours(25));

        var result =
            engine.Correlate(
                [
                    replication,
                    rpc
                ]);

        Assert.Empty(
            result);
    }

    [Fact]
    public void Correlate_DifferentInvestigations_DoesNotCorrelate()
    {
        var engine =
            new CorrelationEngine();

        var replication =
            CreateEvidence(
                "EV-000050",
                EvidenceType.ReplicationFailure,
                "SFOFLEX-DC1",
                DateTimeOffset.UtcNow,
                "INC-SFO-20260709");

        var rpc =
            CreateEvidence(
                "EV-000051",
                EvidenceType.RpcFailure,
                "SFOFLEX-DC1",
                DateTimeOffset.UtcNow.AddMinutes(5),
                "INC-LAX-20260710");

        var result =
            engine.Correlate(
                [
                    replication,
                    rpc
                ]);

        Assert.Empty(
            result);
    }

    private static Evidence CreateEvidence(
        string evidenceId,
        EvidenceType type,
        string target,
        DateTimeOffset collectedUtc,
        string investigationId =
            "INC-SFO-20260709")
    {
        return new Evidence
        {
            EvidenceId =
                evidenceId,

            InvestigationId =
                investigationId,

            Type =
                type,

            Source =
                "Test",

            Target =
                target,

            CollectedUtc =
                collectedUtc,

            Summary =
                "Test evidence",

            IsValid =
                true
        };
    }
}