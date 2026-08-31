namespace ADOps.Core.ValueObjects;

/// <summary>
/// Represents a summarized view of operational health.
/// </summary>
public sealed record HealthSummary
{
    public required int OverallScore { get; init; }

    public required int HealthyCount { get; init; }

    public required int WarningCount { get; init; }

    public required int CriticalCount { get; init; }

    public required DateTimeOffset EvaluatedUtc { get; init; }

    public bool RequiresImmediateAttention =>
        CriticalCount > 0;

    public bool IsHealthy =>
        CriticalCount == 0 &&
        WarningCount == 0;
}