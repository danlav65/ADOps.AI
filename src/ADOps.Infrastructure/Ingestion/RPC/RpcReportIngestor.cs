using System.Text.Json;
using ADOps.Core.Entities;
using ADOps.Core.Entities.Reporting;
using ADOps.Core.Interfaces;

namespace ADOps.Infrastructure.Ingestion.Rpc;

public sealed class RpcReportIngestor
    : IRpcReportIngestor
{
    public IReadOnlyCollection<RpcRecord> Ingest(
        ReportFile report,
        CollectorContext context)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(context);

        if (report.Type != ReportType.Rpc)
        {
            throw new ArgumentException(
                "Report must be an RPC report.",
                nameof(report));
        }

        var domainController =
            report.DomainController
            ?? throw new ArgumentException(
                "RPC report must identify its source domain controller.",
                nameof(report));

        var collectedUtc =
            report.CollectedUtc
            ?? throw new ArgumentException(
                "RPC report must identify when it was collected.",
                nameof(report));

        using var document =
            JsonDocument.Parse(report.Content);

        var root =
            document.RootElement;

        var target =
            GetRequiredString(
                root,
                "Target",
                report);

        var success =
            GetRequiredBool(
                root,
                "Success",
                report);

        var errorCode =
            GetNullableInt(
                root,
                "ErrorCode");

        var errorMessage =
            GetString(
                root,
                "ErrorMessage");

        var sourceCommand =
            GetString(
                root,
                "SourceCommand");

        return
        [
            new RpcRecord
            {
                DomainController = domainController,
                Target = target,
                Success = success,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage,
                CollectedUtc = collectedUtc,
                SourceCommand = sourceCommand
            }
        ];
    }

    private static string GetRequiredString(
        JsonElement item,
        string propertyName,
        ReportFile report)
    {
        var value =
            GetString(
                item,
                propertyName);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                $"RPC report '{report.ReportId}' is missing required property '{propertyName}'.",
                nameof(report));
        }

        return value;
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

    private static bool GetRequiredBool(
    JsonElement item,
    string propertyName,
    ReportFile report)
    {
    if (!item.TryGetProperty(
            propertyName,
            out var property))
    {
        throw new ArgumentException(
            $"RPC report '{report.ReportId}' is missing required property '{propertyName}'.",
            nameof(report));
    }

    if (property.ValueKind == JsonValueKind.True)
    {
        return true;
    }

    if (property.ValueKind == JsonValueKind.False)
    {
        return false;
    }

    throw new ArgumentException(
        $"RPC report '{report.ReportId}' contains an invalid boolean property '{propertyName}'.",
        nameof(report));
    }

    private static int? GetNullableInt(
        JsonElement item,
        string propertyName)
    {
        if (!item.TryGetProperty(
                propertyName,
                out var property))
        {
            return null;
        }

        if (property.ValueKind ==
            JsonValueKind.Number &&
            property.TryGetInt32(out var value))
        {
            return value;
        }

        return int.TryParse(
            property.ToString(),
            out value)
            ? value
            : null;
    }
}