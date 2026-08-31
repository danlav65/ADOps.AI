using ADOps.Core.Entities;

namespace ADOps.Infrastructure.Collectors.SystemInfo;

public sealed class SystemInfoCommandResult
{
    public required string DomainController { get; init; }

    public required IReadOnlyCollection<SystemInfoRecord> Records
    {
        get;
        init;
    }

    public string? StandardError { get; init; }

    public int ExitCode { get; init; }

    public DateTimeOffset ExecutedUtc { get; init; }
}