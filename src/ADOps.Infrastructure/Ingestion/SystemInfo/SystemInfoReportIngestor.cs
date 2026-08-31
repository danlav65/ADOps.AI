using System.Text.Json;
using ADOps.Core.Entities;
using ADOps.Core.Entities.Reporting;

namespace ADOps.Infrastructure.Ingestion.SystemInfo;

public sealed class SystemInfoReportIngestor
    : ISystemInfoReportIngestor
{
    public IReadOnlyCollection<SystemInfoRecord> Ingest(
        ReportFile report,
        CollectorContext context)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(context);

        if (report.Type != ReportType.SystemInfo)
        {
            throw new ArgumentException(
                "Report must be a SystemInfo report.",
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
            new List<SystemInfoRecord>();

        foreach (var item in items)
        {
            records.Add(
                ParseRecord(
                    item,
                    report,
                    context,
                    collectedUtc));
        }

        return records;
    }

    private static SystemInfoRecord ParseRecord(
        JsonElement item,
        ReportFile report,
        CollectorContext context,
        DateTimeOffset collectedUtc)
    {
        var domainController =
            GetString(item, "DomainController")
            ?? report.DomainController
            ?? string.Empty;

        var computerName =
            GetString(item, "ComputerName")
            ?? GetString(item, "CSName")
            ?? domainController;

        return new SystemInfoRecord
        {
            DomainController = domainController,

            Site =
                GetString(item, "Site")
                ?? string.Empty,

            ComputerName =
                computerName,

            OperatingSystem =
                GetString(item, "OperatingSystem")
                ?? GetString(item, "Caption")
                ?? string.Empty,

            OsVersion =
                GetString(item, "OsVersion")
                ?? GetString(item, "Version")
                ?? string.Empty,

            BuildNumber =
                GetString(item, "BuildNumber")
                ?? string.Empty,

            Edition =
                GetString(item, "Edition")
                ?? string.Empty,

            Architecture =
                GetString(item, "Architecture")
                ?? GetString(item, "OSArchitecture")
                ?? string.Empty,

            InstallDate =
                ParseDate(item, "InstallDate"),

            LastBootTime =
                ParseDate(item, "LastBootTime")
                ?? ParseDate(item, "LastBootUpTime"),

            TimeZone =
                GetString(item, "TimeZone")
                ?? string.Empty,

            LogicalProcessors =
                GetInt(item, "LogicalProcessors"),

            PhysicalMemoryGb =
                GetInt(item, "PhysicalMemoryGb"),

            SystemDriveFreeSpaceGb =
                GetDouble(item, "SystemDriveFreeSpaceGb"),

            VirtualMachine =
                GetBool(item, "VirtualMachine"),

            Hypervisor =
                GetString(item, "Hypervisor"),

            PowerShellVersion =
                GetString(item, "PowerShellVersion")
                ?? string.Empty,

            DotNetVersion =
                GetString(item, "DotNetVersion")
                ?? string.Empty,

            CollectedUtc =
                collectedUtc
        };
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

    private static int GetInt(
        JsonElement item,
        string propertyName)
    {
        if (!item.TryGetProperty(
                propertyName,
                out var property))
        {
            return 0;
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
            : 0;
    }

    private static double GetDouble(
        JsonElement item,
        string propertyName)
    {
        if (!item.TryGetProperty(
                propertyName,
                out var property))
        {
            return 0;
        }

        if (property.ValueKind ==
            JsonValueKind.Number &&
            property.TryGetDouble(out var value))
        {
            return value;
        }

        return double.TryParse(
            property.ToString(),
            out value)
            ? value
            : 0;
    }

    private static bool GetBool(
        JsonElement item,
        string propertyName)
    {
        if (!item.TryGetProperty(
                propertyName,
                out var property))
        {
            return false;
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
            out var value) &&
            value;
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

        return DateTimeOffset.TryParse(
            value,
            out var parsed)
            ? parsed
            : null;
    }
}