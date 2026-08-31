using ADOps.Core.Entities;

namespace ADOps.Infrastructure.Collectors.Patch;

public sealed class PatchCommandResult
{
    public required string DomainController { get; init; }

    public required IReadOnlyCollection<PatchRecord>
        Records
    {
        get;
        init;
    }

    public string? StandardError { get; init; }

    public int ExitCode { get; init; }

    public DateTimeOffset ExecutedUtc { get; init; }
}