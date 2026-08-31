namespace ADOps.Core.Entities;

public sealed class RpcRecord
{
    public required string DomainController { get; init; }

    public required string Target { get; init; }

    public string? SourceAddress { get; init; }

    public string? RemoteAddress { get; init; }

    public int? RemotePort { get; init; }

    public string? InterfaceAlias { get; init; }

    public bool Success { get; init; }

    public int? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public DateTimeOffset CollectedUtc { get; init; }

    public string? SourceCommand { get; init; }
}