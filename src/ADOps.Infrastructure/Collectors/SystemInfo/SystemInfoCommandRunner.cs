using System.Diagnostics;
using System.Text.Json;
using ADOps.Core.Entities;

namespace ADOps.Infrastructure.Collectors.SystemInfo;

public sealed class SystemInfoCommandRunner
    : ISystemInfoCommandRunner
{
    public async Task<SystemInfoCommandResult> RunAsync(
        string domainController,
        CancellationToken cancellationToken = default)
    {
        var executedUtc = DateTimeOffset.UtcNow;

        var script =
            $@"
            Invoke-Command `
                -ComputerName ""{domainController}"" `
                -ScriptBlock {{
                    Get-CimInstance Win32_OperatingSystem |
                    Select-Object `
                        CSName,
                        Caption,
                        Version,
                        BuildNumber,
                        OSArchitecture,
                        InstallDate,
                        LastBootUpTime,
                        WindowsDirectory
                }} |
            ConvertTo-Json -Depth 5
            ";

        var startInfo =
            new ProcessStartInfo
            {
                FileName = "powershell.exe",

                Arguments =
                    $"-NoProfile -NonInteractive -Command \"{script}\"",

                RedirectStandardOutput = true,

                RedirectStandardError = true,

                UseShellExecute = false,

                CreateNoWindow = true
            };

        using var process =
            new Process
            {
                StartInfo = startInfo
            };

        process.Start();

        var standardOutputTask =
            process.StandardOutput.ReadToEndAsync(
                cancellationToken);

        var standardErrorTask =
            process.StandardError.ReadToEndAsync(
                cancellationToken);

        await process.WaitForExitAsync(
            cancellationToken);

        var standardOutput =
            await standardOutputTask;

        var standardError =
            await standardErrorTask;

        if (process.ExitCode != 0)
        {
            return new SystemInfoCommandResult
            {
                DomainController = domainController,
                Records = [],
                StandardError = standardError,
                ExitCode = process.ExitCode,
                ExecutedUtc = executedUtc
            };
        }

        var records =
            ParseSystemInfoOutput(
                standardOutput,
                domainController,
                executedUtc);

        return new SystemInfoCommandResult
        {
            DomainController = domainController,
            Records = records,
            StandardError = standardError,
            ExitCode = process.ExitCode,
            ExecutedUtc = executedUtc
        };
    }

    private static IReadOnlyCollection<SystemInfoRecord>
        ParseSystemInfoOutput(
            string output,
            string domainController,
            DateTimeOffset collectedUtc)
    {
        if (string.IsNullOrWhiteSpace(output))
        {
            return [];
        }

        using var document =
            JsonDocument.Parse(output);

        var items =
            document.RootElement.ValueKind ==
            JsonValueKind.Array
                ? document.RootElement.EnumerateArray().ToList()
                : new List<JsonElement>
                {
                    document.RootElement
                };

        var records =
            new List<SystemInfoRecord>();

        foreach (var item in items)
        {
            var computerName =
                GetString(item, "CSName")
                ?? domainController;

            var operatingSystem =
                GetString(item, "Caption")
                ?? string.Empty;

            var osVersion =
                GetString(item, "Version")
                ?? string.Empty;

            var buildNumber =
                GetString(item, "BuildNumber")
                ?? string.Empty;

            var architecture =
                GetString(item, "OSArchitecture")
                ?? string.Empty;

            DateTimeOffset? installDate =
                ParseDate(item, "InstallDate");

            DateTimeOffset? lastBootTime =
                ParseDate(item, "LastBootUpTime");

            records.Add(
                new SystemInfoRecord
                {
                    DomainController = domainController,

                    Site = string.Empty,

                    ComputerName = computerName,

                    OperatingSystem = operatingSystem,

                    OsVersion = osVersion,

                    BuildNumber = buildNumber,

                    Edition = operatingSystem,

                    Architecture = architecture,

                    InstallDate = installDate,

                    LastBootTime = lastBootTime,

                    TimeZone = string.Empty,

                    LogicalProcessors = 0,

                    PhysicalMemoryGb = 0,

                    SystemDriveFreeSpaceGb = 0,

                    VirtualMachine = false,

                    Hypervisor = null,

                    PowerShellVersion = string.Empty,

                    DotNetVersion = string.Empty,

                    CollectedUtc = collectedUtc
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

        return DateTimeOffset.TryParse(
            value,
            out var parsed)
            ? parsed
            : null;
    }
}
