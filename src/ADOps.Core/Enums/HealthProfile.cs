using ADOps.Core.Enums;

namespace ADOps.Core.Entities;

/// <summary>
/// Represents the operational health profile of a monitored target.
/// </summary>
public sealed class HealthProfile : EntityBase
{
    private readonly List<HealthIndicator> _indicators = [];

    public required string Target { get; init; }

    public required string Environment { get; init; }

    public DateTimeOffset LastUpdatedUtc { get; private set; }

    public IReadOnlyCollection<HealthIndicator> Indicators => _indicators;

    public void AddIndicator(HealthIndicator indicator)
    {
        ArgumentNullException.ThrowIfNull(indicator);

        _indicators.RemoveAll(i =>
            i.Name == indicator.Name &&
            i.Domain == indicator.Domain);

        _indicators.Add(indicator);

        LastUpdatedUtc = DateTimeOffset.UtcNow;

        Touch();
    }

    public IEnumerable<HealthIndicator> GetCriticalIndicators() =>
        _indicators.Where(i => i.Status == HealthStatus.Critical);

    public IEnumerable<HealthIndicator> GetIndicatorsByDomain(HealthDomain domain) =>
        _indicators.Where(i => i.Domain == domain);
}