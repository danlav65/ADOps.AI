using ADOps.Application.Reports;
using ADOps.Core.Entities;

namespace ADOps.Application.Presentation;

public sealed class InvestigationPresenter
{
    public InvestigationReport Build(
        ADOps.Core.Entities.Investigation investigation,
        IReadOnlyCollection<Evidence> evidence,
        IReadOnlyCollection<CorrelatedFinding> findings,
        RootCauseAnalysis rootCauseAnalysis,
        IReadOnlyCollection<Recommendation> recommendations)
    {
        ArgumentNullException.ThrowIfNull(investigation);
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(findings);
        ArgumentNullException.ThrowIfNull(rootCauseAnalysis);
        ArgumentNullException.ThrowIfNull(recommendations);

        return new InvestigationReport
        {
            InvestigationId =
                investigation.Id.ToString(),

            IncidentNumber =
                investigation.Incident.IncidentNumber,

            GeneratedUtc =
                DateTimeOffset.UtcNow,

            ExecutiveSummary =
                BuildExecutiveSummary(
                    investigation,
                    findings,
                    rootCauseAnalysis),

            Evidence =
                evidence
                    .Select(MapEvidence)
                    .ToList(),

            Correlations =
                findings
                    .Select(MapCorrelation)
                    .ToList(),

            RootCause =
                rootCauseAnalysis.RootCause,

            Recommendations =
                recommendations
                    .Select(MapRecommendation)
                    .ToList(),

            OverallConfidence =
                rootCauseAnalysis.Confidence
                ?? CalculateConfidence(findings)
        };
    }

    private static ExecutiveSummary BuildExecutiveSummary(
        ADOps.Core.Entities.Investigation investigation,
        IReadOnlyCollection<CorrelatedFinding> findings,
        RootCauseAnalysis rootCauseAnalysis)
    {
        var summary =
            string.IsNullOrWhiteSpace(
                rootCauseAnalysis.ExecutiveSummary)
                ? $"Investigation {investigation.InvestigationNumber} " +
                $"identified {findings.Count} correlated findings."
                : rootCauseAnalysis.ExecutiveSummary;

        var businessImpact =
            string.IsNullOrWhiteSpace(
                rootCauseAnalysis.BusinessImpact)
                ? $"Incident {investigation.Incident.IncidentNumber} " +
                $"occurred at site {investigation.Incident.SiteCode}."
                : rootCauseAnalysis.BusinessImpact;

        var technicalImpact =
            string.IsNullOrWhiteSpace(
                rootCauseAnalysis.TechnicalImpact)
                ? "Active Directory health indicators require analysis."
                : rootCauseAnalysis.TechnicalImpact;

        return new ExecutiveSummary
        {
            Summary = summary,
            BusinessImpact = businessImpact,
            TechnicalImpact = technicalImpact
        };
    }

    private static EvidenceReportItem MapEvidence(
        Evidence evidence)
    {
        return new EvidenceReportItem
        {
            EvidenceId = evidence.EvidenceId,
            Type = evidence.Type.ToString(),
            Source = evidence.Source,
            Target = evidence.Target,
            CollectedUtc = evidence.CollectedUtc,
            Summary = evidence.Summary,
            IsValid = evidence.IsValid,
            ErrorCode = evidence.ErrorCode,
            Details = evidence.Details
        };
    }

    private static CorrelationReportItem MapCorrelation(
        CorrelatedFinding finding)
    {
        return new CorrelationReportItem
        {
            Type = finding.CorrelationType,
            Summary = finding.Summary,
            Confidence = finding.Confidence,
            EvidenceIds = finding.EvidenceIds
        };
    }

    private static RecommendationItem MapRecommendation(
        Recommendation recommendation)
    {
        return new RecommendationItem
        {
            Action = recommendation.Title,
            Reason = recommendation.Description,
            Priority = recommendation.Priority.ToString()
        };
    }

    private static double CalculateConfidence(
        IReadOnlyCollection<CorrelatedFinding> findings)
    {
        if (!findings.Any())
        {
            return 0;
        }

        return Math.Round(
            findings.Average(x => x.Confidence),
            2);
    }
}
