using ADOps.Core.Entities;
using ADOps.Core.Interfaces;

namespace ADOps.Infrastructure.Analysis;

/// <summary>
/// Generates root cause analysis from correlated findings.
/// </summary>
public sealed class RootCauseAnalyzer : IRootCauseAnalyzer
{
    /// <summary>
    /// Generates a root cause analysis from correlated findings.
    /// </summary>
    public RootCauseAnalysis Analyze(
        IEnumerable<CorrelatedFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(findings);

        var findingList = findings.ToList();

        if (findingList.Count == 0)
        {
            throw new InvalidOperationException(
                "Cannot generate RCA without findings.");
        }

        var partnerPatchFinding =
            findingList
                .Where(f =>
                    f.CorrelationType.Contains(
                        "Replication Partner",
                        StringComparison.OrdinalIgnoreCase)
                    &&
                    f.CorrelationType.Contains(
                        "Patch",
                        StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(f => f.Confidence)
                .FirstOrDefault();

        var patchFinding =
            findingList
                .Where(f =>
                    f.CorrelationType.Contains(
                        "Patch",
                        StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(f => f.Confidence)
                .FirstOrDefault();

        var replicationFinding =
            findingList.Any(f =>
                f.CorrelationType.Contains(
                    "Replication",
                    StringComparison.OrdinalIgnoreCase));

        var rpcFinding =
            findingList.Any(f =>
                f.CorrelationType.Contains(
                    "RPC",
                    StringComparison.OrdinalIgnoreCase));

        var infrastructureFinding =
            findingList.Any(f =>
                f.CorrelationType.Contains(
                    "Infrastructure",
                    StringComparison.OrdinalIgnoreCase));

        var rootCause =
            BuildRootCause(
                partnerPatchFinding,
                patchFinding);

        var summary =
            BuildExecutiveSummary(
                findingList,
                partnerPatchFinding,
                rpcFinding,
                infrastructureFinding);

        var technicalImpact =
            replicationFinding
                ? "Active Directory replication health was impacted on the affected domain controller."
                : null;

        var correctiveActions =
            BuildCorrectiveActions(
                patchFinding,
                rpcFinding,
                infrastructureFinding,
                replicationFinding);

        var preventiveActions =
            BuildPreventiveActions(
                patchFinding,
                infrastructureFinding);

        var analysis = new RootCauseAnalysis
        {
            Title =
                "Active Directory Incident Root Cause Analysis",

            ExecutiveSummary =
                summary,

            RootCause =
                rootCause
        };

        if (partnerPatchFinding is not null)
        {
            analysis.SetPrimaryFinding(
                partnerPatchFinding.CorrelationId,
                partnerPatchFinding.Confidence,
                partnerPatchFinding.EvidenceIds);
        }
        else if (patchFinding is not null)
        {
            analysis.SetPrimaryFinding(
                patchFinding.CorrelationId,
                patchFinding.Confidence,
                patchFinding.EvidenceIds);
        }

        analysis.SetTechnicalImpact(
            technicalImpact);

        analysis.SetCorrectiveActions(
            correctiveActions);

        analysis.SetPreventiveActions(
            preventiveActions);

        return analysis;
    }

    private static string BuildRootCause(
        CorrelatedFinding? partnerPatchFinding,
        CorrelatedFinding? patchFinding)
    {
        if (partnerPatchFinding is not null)
        {
            var evidence =
                string.Join(
                    ", ",
                    partnerPatchFinding.EvidenceIds);

            return
                "Patch baseline drift between the affected domain controller " +
                "and its healthy replication partner is a probable contributing " +
                "factor to the observed Active Directory replication failure. " +
                $"The strongest partner correlation has a confidence of " +
                $"{partnerPatchFinding.Confidence:F2} and is supported by " +
                $"evidence {evidence}.";
        }

        if (patchFinding is not null)
        {
            return
                "Patch baseline drift on the affected domain controller is a " +
                "probable contributing factor to the observed Active Directory " +
                $"replication instability. The correlation has a confidence " +
                $"of {patchFinding.Confidence:F2} and is supported by evidence " +
                $"{string.Join(", ", patchFinding.EvidenceIds)}.";
        }

        return
            "The available evidence confirms Active Directory replication " +
            "instability, but is insufficient to determine a probable primary " +
            "contributing factor.";
    }

    private static string BuildExecutiveSummary(
        IReadOnlyCollection<CorrelatedFinding> findings,
        CorrelatedFinding? partnerPatchFinding,
        bool rpcFinding,
        bool infrastructureFinding)
    {
        var sentences = new List<string>
        {
            $"Analysis identified {findings.Count} correlated findings " +
            "associated with the incident."
        };

        if (partnerPatchFinding is not null)
        {
            sentences.Add(
                "The strongest correlation indicates patch baseline drift " +
                "between the affected domain controller and its healthy " +
                "replication partner.");

            sentences.Add(
                $"Supporting evidence includes " +
                $"{string.Join(", ", partnerPatchFinding.EvidenceIds)}.");
        }

        if (rpcFinding)
        {
            sentences.Add(
                "An RPC communication failure was also observed.");
        }

        if (infrastructureFinding)
        {
            sentences.Add(
                "Infrastructure health evidence identified an additional " +
                "condition on the affected domain controller.");
        }

        return string.Join(" ", sentences);
    }

    private static string? BuildCorrectiveActions(
        CorrelatedFinding? patchFinding,
        bool rpcFinding,
        bool infrastructureFinding,
        bool replicationFinding)
    {
        var actions = new List<string>();

        if (patchFinding is not null)
        {
            actions.Add(
                "Align the domain controller patch baselines and verify " +
                "that required cumulative updates are installed consistently.");
        }

        if (rpcFinding)
        {
            actions.Add(
                "Validate RPC connectivity between the affected domain " +
                "controller and its replication partners.");
        }

        if (infrastructureFinding)
        {
            actions.Add(
                "Review available disk space and other infrastructure " +
                "health indicators on the affected domain controller.");
        }

        if (replicationFinding)
        {
            actions.Add(
                "Perform Active Directory replication validation after " +
                "corrective actions are completed.");
        }

        return actions.Count == 0
            ? null
            : string.Join(" ", actions);
    }

    private static string? BuildPreventiveActions(
        CorrelatedFinding? patchFinding,
        bool infrastructureFinding)
    {
        var actions = new List<string>();

        if (patchFinding is not null)
        {
            actions.Add(
                "Implement automated domain controller patch baseline " +
                "compliance monitoring.");
        }

        if (infrastructureFinding)
        {
            actions.Add(
                "Implement proactive monitoring for domain controller " +
                "disk space and infrastructure health thresholds.");
        }

        return actions.Count == 0
            ? null
            : string.Join(" ", actions);
    }
}
