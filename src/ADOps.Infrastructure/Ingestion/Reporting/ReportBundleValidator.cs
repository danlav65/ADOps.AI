using ADOps.Core.Entities.Reporting;

namespace ADOps.Infrastructure.Ingestion.Reporting;

public sealed class ReportBundleValidator
{
    public IReadOnlyCollection<string> Validate(ReportBundle bundle)
    {
        ArgumentNullException.ThrowIfNull(bundle);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(bundle.BundleId))
        {
            errors.Add("BundleId is required.");
        }

        if (string.IsNullOrWhiteSpace(bundle.InvestigationId))
        {
            errors.Add("InvestigationId is required.");
        }

        if (string.IsNullOrWhiteSpace(bundle.Site))
        {
            errors.Add("Site is required.");
        }

        if (bundle.Reports.Count == 0)
        {
            errors.Add("Report bundle must contain at least one report.");
        }

        var reportIds = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var report in bundle.Reports)
        {
            if (string.IsNullOrWhiteSpace(report.ReportId))
            {
                errors.Add("ReportId is required.");
            }
            else if (!reportIds.Add(report.ReportId))
            {
                errors.Add(
                    $"Duplicate ReportId detected: {report.ReportId}.");
            }

            if (report.Type == ReportType.Unknown)
            {
                errors.Add(
                    $"Report '{report.ReportId}' has an unknown report type.");
            }

            if (string.IsNullOrWhiteSpace(report.FileName))
            {
                errors.Add(
                    $"Report '{report.ReportId}' is missing a file name.");
            }

            if (string.IsNullOrWhiteSpace(report.Content))
            {
                errors.Add(
                    $"Report '{report.ReportId}' has empty content.");
            }
        }

        return errors;
    }
}