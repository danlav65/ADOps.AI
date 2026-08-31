namespace ADOps.Infrastructure.Collectors;

public sealed class CollectorResult<T>
{
    public required string CollectorName { get; init; }

    public bool Success { get; init; }

    public DateTimeOffset CollectedUtc { get; init; }

    public IReadOnlyCollection<T> Data { get; init; }
        = [];

    public IReadOnlyCollection<string> Errors { get; init; }
        = [];
}
