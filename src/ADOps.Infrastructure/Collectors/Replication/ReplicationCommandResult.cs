namespace ADOps.Infrastructure.Collectors.Replication;

public sealed class ReplicationCommandResult
{
    public required string TargetDomainController { get; init; }

    public required string StandardOutput { get; init; }

    public required string StandardError { get; init; }

    public int ExitCode { get; init; }

    public TimeSpan Duration { get; init; }

    public DateTimeOffset ExecutedUtc { get; init; }
}