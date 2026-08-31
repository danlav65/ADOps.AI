using System.Text.Json;
using ADOps.Core.Entities;

namespace ADOps.Infrastructure.Collectors.Patch;

public sealed class PatchOutputParser
    : IPatchOutputParser
{
    public IReadOnlyCollection<PatchRecord> Parse(
        string domainController,
        string commandOutput,
        DateTimeOffset collectedUtc)
    {
        if (string.IsNullOrWhiteSpace(commandOutput))
        {
            return [];
        }

        using var document =
            JsonDocument.Parse(commandOutput);

        var items =
            document.RootElement.ValueKind ==
            JsonValueKind.Array
                ? document.RootElement.EnumerateArray().ToList()
                : new List<JsonElement>
                {
                    document.RootElement
                };

        var records =
            new List<PatchRecord>();

        foreach (var item in items)
        {
            var hotFixId =
                GetString(
                    item,
                    "HotFixID");

            if (string.IsNullOrWhiteSpace(hotFixId))
            {
                continue;
            }

            var description =
                GetString(
                    item,
                    "Description")
                ?? string.Empty;

            DateTimeOffset? installedUtc =
                ParseDate(
                    item,
                    "InstalledOn");

            records.Add(
                new PatchRecord
                {
                    DomainController =
                        domainController,

                    Site =
                        string.Empty,

                    OperatingSystem =
                        string.Empty,

                    OsBuild =
                        string.Empty,

                    KnowledgeBaseArticle =
                        hotFixId,

                    PatchVersion =
                        description,

                    InstalledUtc =
                        installedUtc,

                    Installed =
                        true,

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

        if (DateTimeOffset.TryParse(
                value,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeUniversal |
                System.Globalization.DateTimeStyles.AdjustToUniversal,
                out var parsed))
        {
            return parsed;
        }

        return null;

    }
}
