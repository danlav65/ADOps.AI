using System.Text.Json;
using ADOps.Core.Entities;
using ADOps.Core.Entities.Reporting;

namespace ADOps.Infrastructure.Ingestion.Patch;

public sealed class PatchReportIngestor
    : IPatchReportIngestor
{
    public IReadOnlyCollection<PatchRecord> Ingest(
        ReportFile report,
        CollectorContext context)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(context);

        if (report.Type != ReportType.Patch)
        {
            throw new ArgumentException(
                "Report must be a Patch report.",
                nameof(report));
        }

        if (string.IsNullOrWhiteSpace(report.Content))
        {
            return [];
        }

        var collectedUtc =
            report.CollectedUtc
            ?? DateTimeOffset.UtcNow;

        using var document =
            JsonDocument.Parse(report.Content);

        var items =
            document.RootElement.ValueKind ==
            JsonValueKind.Array
                ? document.RootElement.EnumerateArray().ToList()
                : [document.RootElement];

        var records =
            new List<PatchRecord>();

        foreach (var item in items)
        {
            var knowledgeBaseArticle =
                GetString(item, "KnowledgeBaseArticle")
                ?? GetString(item, "HotFixID");

            if (string.IsNullOrWhiteSpace(
                    knowledgeBaseArticle))
            {
                continue;
            }

            records.Add(
                new PatchRecord
                {
                    DomainController =
                        GetString(
                            item,
                            "DomainController")
                        ?? report.DomainController
                        ?? string.Empty,

                    Site =
                        GetString(
                            item,
                            "Site")
                        ?? context.Site,

                    OperatingSystem =
                        GetString(
                            item,
                            "OperatingSystem")
                        ?? GetString(
                            item,
                            "Caption")
                        ?? string.Empty,

                    OsBuild =
                        GetString(
                            item,
                            "OsBuild")
                        ?? GetString(
                            item,
                            "BuildNumber")
                        ?? string.Empty,

                    KnowledgeBaseArticle =
                        knowledgeBaseArticle,

                    PatchVersion =
                        GetString(
                            item,
                            "PatchVersion")
                        ?? GetString(
                            item,
                            "Description")
                        ?? string.Empty,

                    InstalledUtc =
                        ParseDate(
                            item,
                            "InstalledUtc")
                        ?? ParseDate(
                            item,
                            "InstalledOn"),

                    Installed =
                        GetBool(
                            item,
                            "Installed",
                            true),

                    CollectedUtc =
                        collectedUtc
                });
        }

        return records;
    }

    private static string? GetString(
        JsonElement item,
        string propertyName)
    {
        if (!item.TryGetProperty(
                propertyName,
                out var property))
        {
            return null;
        }

        return property.ValueKind ==
               JsonValueKind.String
            ? property.GetString()
            : property.ToString();
    }

    private static bool GetBool(
        JsonElement item,
        string propertyName,
        bool defaultValue)
    {
        if (!item.TryGetProperty(
                propertyName,
                out var property))
        {
            return defaultValue;
        }

        if (property.ValueKind ==
            JsonValueKind.True)
        {
            return true;
        }

        if (property.ValueKind ==
            JsonValueKind.False)
        {
            return false;
        }

        return bool.TryParse(
            property.ToString(),
            out var value)
            ? value
            : defaultValue;
    }

    private static DateTimeOffset? ParseDate(
        JsonElement item,
        string propertyName)
    {
        var value =
            GetString(
                item,
                propertyName);

        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!DateTimeOffset.TryParse(
                value,
                out var parsed))
        {
            return null;
        }

        return parsed.ToUniversalTime();
    }
}
