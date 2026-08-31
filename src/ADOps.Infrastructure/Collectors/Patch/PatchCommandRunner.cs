using System.Diagnostics;

namespace ADOps.Infrastructure.Collectors.Patch;

public sealed class PatchCommandRunner
    : IPatchCommandRunner
{
    private readonly IPatchOutputParser
        _outputParser;

    public PatchCommandRunner(
        IPatchOutputParser outputParser)
    {
        _outputParser =
            outputParser;
    }

    public async Task<PatchCommandResult> RunAsync(
        string domainController,
        CancellationToken cancellationToken = default)
    {
        var executedUtc =
            DateTimeOffset.UtcNow;

        var script =
            $@"
            Invoke-Command `
                -ComputerName ""{domainController}"" `
                -ScriptBlock {{
                    Get-HotFix |
                    Select-Object `
                        PSComputerName,
                        HotFixID,
                        InstalledOn,
                        Description
                }} |
            ConvertTo-Json -Depth 5
            ";

        var startInfo =
            new ProcessStartInfo
            {
                FileName =
                    "powershell.exe",

                Arguments =
                    $"-NoProfile -NonInteractive -Command \"{script}\"",

                RedirectStandardOutput =
                    true,

                RedirectStandardError =
                    true,

                UseShellExecute =
                    false,

                CreateNoWindow =
                    true
            };

        using var process =
            new Process
            {
                StartInfo =
                    startInfo
            };

        process.Start();

        var standardOutputTask =
            process.StandardOutput
                .ReadToEndAsync(
                    cancellationToken);

        var standardErrorTask =
            process.StandardError
                .ReadToEndAsync(
                    cancellationToken);

        await process.WaitForExitAsync(
            cancellationToken);

        var standardOutput =
            await standardOutputTask;

        var standardError =
            await standardErrorTask;

        if (process.ExitCode != 0)
        {
            return new PatchCommandResult
            {
                DomainController =
                    domainController,

                Records =
                    [],

                StandardError =
                    standardError,

                ExitCode =
                    process.ExitCode,

                ExecutedUtc =
                    executedUtc
            };
        }

        var records =
            _outputParser.Parse(
                domainController,
                standardOutput,
                executedUtc);

        return new PatchCommandResult
        {
            DomainController =
                domainController,

            Records =
                records,

            StandardError =
                standardError,

            ExitCode =
                process.ExitCode,

            ExecutedUtc =
                executedUtc
        };
    }
}
