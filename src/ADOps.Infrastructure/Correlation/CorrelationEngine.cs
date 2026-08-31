using System.Diagnostics;
using ADOps.Core.Entities;
using ADOps.Core.Interfaces;
using EvidenceEntity = ADOps.Core.Entities.Evidence;

namespace ADOps.Infrastructure.Correlation;

public sealed class CorrelationEngine : ICorrelationEngine
{
    private static readonly TimeSpan CorrelationWindow =
        TimeSpan.FromHours(24);

    public IReadOnlyCollection<CorrelatedFinding> Correlate(
        IReadOnlyCollection<EvidenceEntity> evidence,
        TopologyContext topology)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(topology);

        var findings = new List<CorrelatedFinding>();

        var groups = evidence.GroupBy(x => x.InvestigationId);

        foreach (var investigationGroup in groups)
        {
            findings.AddRange(
                CorrelateInvestigation(
                    investigationGroup,
                    topology));
        }

        return findings;
    }

    private static IEnumerable<CorrelatedFinding> CorrelateInvestigation(
        IEnumerable<EvidenceEntity> evidence,
        TopologyContext topology)
    {
        var evidenceList = evidence.ToList();

        var findings = new List<CorrelatedFinding>();

        findings.AddRange(
            CorrelateReplicationAndRpc(evidenceList));

        findings.AddRange(
            CorrelateReplicationAndInfrastructure(evidenceList));

        findings.AddRange(
            CorrelateReplicationAndPatch(evidenceList));

        findings.AddRange(
            CorrelateAcrossReplicationPartners(
                evidenceList,
                topology));

        return findings
            .GroupBy(x => new
            {
                x.CorrelationType,
                x.Summary
            })
            .Select(x => x.First());
    }

    private static IEnumerable<CorrelatedFinding>
        CorrelateReplicationAndRpc(
            IReadOnlyCollection<EvidenceEntity> evidence)
    {
        var replicationFailures = evidence
            .Where(x =>
                x.Type == EvidenceType.ReplicationFailure)
            .ToList();

        var rpcFailures = evidence
            .Where(x =>
                x.Type == EvidenceType.RpcFailure)
            .ToList();

        foreach (var replication in replicationFailures)
        {
            foreach (var rpc in rpcFailures)
            {
                if (!string.Equals(
                    replication.Source,
                    rpc.Source,
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!WithinCorrelationWindow(
                        replication,
                        rpc))
                {
                    continue;
                }

                yield return CreateFinding(
                    replication,
                    rpc,
                    "Replication + RPC",
                    $"Replication failure on " +
                    $"{replication.Source} correlates with " +
                    $"an RPC communication failure.",
                    0.85);
            }
        }
    }

    private static IEnumerable<CorrelatedFinding>
        CorrelateReplicationAndInfrastructure(
            IReadOnlyCollection<EvidenceEntity> evidence)
    {
        var replicationEvidence = evidence
            .Where(x =>
                x.Type == EvidenceType.ReplicationFailure)
            .ToList();

        var infrastructureEvidence = evidence
            .Where(x =>
                x.Type ==
                EvidenceType.InfrastructureHealth)
            .ToList();

        foreach (var replication in replicationEvidence)
        {
            foreach (var infrastructure in infrastructureEvidence)
            {
                if (!string.Equals(
                        replication.Source,
                        infrastructure.Target,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!WithinCorrelationWindow(
                        replication,
                        infrastructure))
                {
                    continue;
                }

                yield return CreateFinding(
                    replication,
                    infrastructure,
                    "Replication + Infrastructure",
                    $"Replication issue on " +
                    $"{replication.Source} correlates with " +
                    $"an infrastructure health issue on the " +
                    $"same target.",
                    0.80);
            }
        }
    }

    private static IEnumerable<CorrelatedFinding>
        CorrelateReplicationAndPatch(
            IReadOnlyCollection<EvidenceEntity> evidence)
    {
        var replicationEvidence = evidence
            .Where(x =>
                x.Type == EvidenceType.ReplicationFailure)
            .ToList();

        var patchEvidence = evidence
            .Where(x =>
                x.Type == EvidenceType.Patch)
            .ToList();

        foreach (var replication in replicationEvidence)
        {
            foreach (var patch in patchEvidence)
            {
                if (!string.Equals(
                        replication.Source,
                        patch.Target,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!WithinCorrelationWindow(
                        replication,
                        patch))
                {
                    continue;
                }

                if (!PatchIndicatesMismatch(patch))
                {
                    continue;
                }

                yield return CreateFinding(
                    replication,
                    patch,
                    "Replication + Patch Baseline",
                    $"Replication failure on " +
                    $"{replication.Source} correlates with " +
                    $"patch baseline drift.",
                    0.75);
            }
        }
    }

    private static IEnumerable<CorrelatedFinding>
        CorrelateAcrossReplicationPartners(
            IReadOnlyCollection<EvidenceEntity> evidence,
            TopologyContext topology)
    {
        var replicationEvidence = evidence
            .Where(IsReplicationEvidence)
            .ToList();

        var patchEvidence = evidence
            .Where(x =>
                x.Type ==
                EvidenceType.Patch)
            .ToList();

        foreach (var relationship in
                 topology.ReplicationPartners)
        {
            var sourceReplication = replicationEvidence
                .Where(x =>
                    string.Equals(
                        x.Source,
                        relationship.SourceDomainController,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

            var partnerReplication = replicationEvidence
                .Where(x =>
                    string.Equals(
                        x.Source,
                        relationship.PartnerDomainController,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

            var sourcePatch = patchEvidence
                .Where(x =>
                    string.Equals(
                        x.Target,
                        relationship.SourceDomainController,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

            var partnerPatch = patchEvidence
                .Where(x =>
                    string.Equals(
                        x.Target,
                        relationship.PartnerDomainController,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var sourceReplicationRecord
                     in sourceReplication)
            {
                foreach (var partnerReplicationRecord
                         in partnerReplication)
                {
                    if (sourceReplicationRecord.Type !=
                        EvidenceType.ReplicationFailure)
                    {
                        continue;
                    }

                    if (partnerReplicationRecord.Type !=
                        EvidenceType.ReplicationSuccess)
                    {
                        continue;
                    }

                    if (!WithinCorrelationWindow(
                            sourceReplicationRecord,
                            partnerReplicationRecord))
                    {
                        continue;
                    }

                    foreach (var sourcePatchRecord
                             in sourcePatch)
                    {
                        foreach (var partnerPatchRecord
                                 in partnerPatch)
                        {
                            if (!WithinCorrelationWindow(
                                    sourcePatchRecord,
                                    partnerPatchRecord))
                            {
                                continue;
                            }

                            if (!PatchBaselinesDiffer(
                                    sourcePatchRecord,
                                    partnerPatchRecord))
                            {
                                continue;
                            }

                            yield return CreateFinding(
                                sourceReplicationRecord,
                                partnerReplicationRecord,
                                sourcePatchRecord,
                                partnerPatchRecord,
                                relationship);
                        }
                    }
                }
            }
        }
    }

    private static CorrelatedFinding CreateFinding(
        EvidenceEntity sourceReplication,
        EvidenceEntity partnerReplication,
        EvidenceEntity sourcePatch,
        EvidenceEntity partnerPatch,
        ReplicationPartnerRelationship relationship)
    {
        return new CorrelatedFinding
        {
            CorrelationId =
                $"COR-{Guid.NewGuid():N}",

            InvestigationId =
                sourceReplication.InvestigationId,

            Summary =
                $"Replication failure on " +
                $"{relationship.SourceDomainController} " +
                $"correlates with healthy replication on partner " +
                $"{relationship.PartnerDomainController} " +
                $"and a patch baseline difference between " +
                $"the replication partners.",

            EvidenceIds =
            [
                sourceReplication.EvidenceId,
                partnerReplication.EvidenceId,
                sourcePatch.EvidenceId,
                partnerPatch.EvidenceId
            ],

            CorrelationType =
                "Replication Partner + Patch Baseline",

            CreatedUtc =
                DateTimeOffset.UtcNow,

            Confidence =
                0.75
        };
    }

    private static bool PatchBaselinesDiffer(
        EvidenceEntity first,
        EvidenceEntity second)
    {
        var firstDetails =
            first.Details ?? first.Summary;

        var secondDetails =
            second.Details ?? second.Summary;

        return !string.Equals(
            firstDetails,
            secondDetails,
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool PatchIndicatesMismatch(
        EvidenceEntity patch)
    {
        var text =
            $"{patch.Summary} {patch.Details}";

        return text.Contains(
                   "mismatch",
                   StringComparison.OrdinalIgnoreCase)
            || text.Contains(
                   "missing",
                   StringComparison.OrdinalIgnoreCase)
            || text.Contains(
                   "drift",
                   StringComparison.OrdinalIgnoreCase)
            || text.Contains(
                   "not installed",
                   StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsReplicationEvidence(
        EvidenceEntity evidence)
    {
        return
            evidence.Type ==
                EvidenceType.ReplicationFailure
            ||
            evidence.Type ==
                EvidenceType.ReplicationSuccess;
    }

    private static bool SameTarget(
        EvidenceEntity first,
        EvidenceEntity second)
    {
        return string.Equals(
            first.Target,
            second.Target,
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool WithinCorrelationWindow(
        EvidenceEntity first,
        EvidenceEntity second)
    {
        var difference =
            (first.CollectedUtc -
             second.CollectedUtc)
            .Duration();

        return difference <=
            CorrelationWindow;
    }

    private static CorrelatedFinding CreateFinding(
        EvidenceEntity first,
        EvidenceEntity second,
        string correlationType,
        string summary,
        double confidence)
    {
        return new CorrelatedFinding
        {
            CorrelationId =
                $"COR-{Guid.NewGuid():N}",

            InvestigationId =
                first.InvestigationId,

            Summary =
                summary,

            EvidenceIds =
            [
                first.EvidenceId,
                second.EvidenceId
            ],

            CorrelationType =
                correlationType,

            CreatedUtc =
                DateTimeOffset.UtcNow,

            Confidence =
                confidence
        };
    }
}

