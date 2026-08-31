using System.Diagnostics;

namespace ADOps.Infrastructure.Collectors.Rpc;

public sealed class RpcCommandRunner : IRpcCommandRunner
{
    public async Task<string> RunAsync(
        string domainController,
        string target,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            domainController);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            target);

        var script =
            $@"
            Invoke-Command `
                -ComputerName ""{domainController}"" `
                -ScriptBlock {{
                    Test-NetConnection `
                        -ComputerName ""{target}"" `
                        -Port 135 |
                    Select-Object `
                        ComputerName,
                        RemoteAddress,
                        RemotePort,
                        InterfaceAlias,
                        @{{Name='SourceAddress';Expression={{
                            if ($_.SourceAddress -is [string])
                            {{
                                $_.SourceAddress
                            }}
                            else
                            {{
                                $_.SourceAddress.IPAddress
                            }}
                        }} }},
                        TcpTestSucceeded
                }} |
            ConvertTo-Json -Compress
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
                $"RPC connectivity test failed for " +
                $"{domainController} -> {target}: {error}");
        }

        return output;
    }
}