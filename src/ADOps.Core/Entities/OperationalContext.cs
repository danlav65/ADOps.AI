namespace ADOps.Core.Entities;

/// <summary>
/// Represents the operational and architectural context of a monitored target.
/// Used to determine business impact and operational risk.
/// </summary>
public sealed class OperationalContext : EntityBase
{
    private readonly List<string> _hostedServices = [];

    public required string Target { get; init; }

    public required string Site { get; init; }

    public required string Environment { get; init; }

    public bool IsProduction { get; init; }

    public bool IsCriticalSite { get; init; }

    public int WritableDomainControllers { get; init; }

    public bool IsGlobalCatalog { get; init; }

    public IReadOnlyCollection<string> HostedServices =>
        _hostedServices;

    public void AddHostedService(string service)
    {
        if (!_hostedServices.Contains(service))
        {
            _hostedServices.Add(service);
            Touch();
        }
    }
}