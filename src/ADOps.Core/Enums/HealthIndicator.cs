using ADOps.Core.Enums;

namespace ADOps.Core.Entities;

/// <summary>
/// Represents the evaluated health of a monitored component.
/// This class is independent of how the data was collected.
/// </summary>
public sealed class HealthIndicator : EntityBase
{
    public required string Name { get; init; }

    public required HealthDomain Domain { get; init; }

    /// <summary>
    /// The object this indicator applies to.
    /// Example: SFOFLEX-DC1
    /// </summary>
    public required string Target { get; init; }

    /// <summary>
    /// Human-readable current value.
    /// Examples:
    /// "4% Free"
    /// "RPC Server Unavailable"
    /// "Healthy"
    /// </summary>
    public string CurrentValue { get; private set; } = string.Empty;

    /// <summary>
    /// Human-readable expected value.
    /// Examples:
    /// ">15% Free"
    /// "Successful"
    /// "Running"
    /// </summary>
    public required string ExpectedValue { get; init; }

    /// <summary>
    /// Importance when calculating operational health.
    /// </summary>
    public int Weight { get; init; }

    public HealthStatus Status { get; private set; }

    public SeverityLevel Severity { get; private set; }

    public DateTimeOffset LastEvaluatedUtc { get; private set; }

    public bool RequiresImmediateAttention =>
        Status == HealthStatus.Critical;

    public void Evaluate(
        string currentValue,
        HealthStatus status,
        SeverityLevel severity)
    {
        CurrentValue = currentValue;
        Status = status;
        Severity = severity;
        LastEvaluatedUtc = DateTimeOffset.UtcNow;

        Touch();
    }
}