using System.Diagnostics;

namespace ADOps.Infrastructure.Collectors.Replication;

public sealed class ReplicationCommandRunner
    : IReplicationCommandRunner
{
    public async Task<string> RunAsync(
        string domainController,
        CancellationToken cancellationToken = default)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "repadmin.exe",
            Arguments = $"/showrepl {domainController}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process
        {
            StartInfo = startInfo
        };

        process.Start();

        var outputTask =
            process.StandardOutput.ReadToEndAsync(
                cancellationToken);

        var errorTask =
            process.StandardError.ReadToEndAsync(
                cancellationToken);

        await process.WaitForExitAsync(
            cancellationToken);

        var output =
            await outputTask;

        var error =
            await errorTask;

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"repadmin failed for {domainController}: {error}");
        }

        return output;
    }
}