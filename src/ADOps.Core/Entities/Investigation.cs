using ADOps.Core.Enums;

namespace ADOps.Core.Entities;

/// <summary>
/// Represents an operational investigation.
/// This is the aggregate root for incident analysis.
/// </summary>
public sealed class Investigation : EntityBase
{
    private readonly List<HealthProfile> _healthProfiles = [];

    public required string InvestigationNumber { get; init; }

    public required Incident Incident { get; init; }

    public InvestigationStatus Status { get; private set; }

    public DateTimeOffset StartedUtc { get; init; }

    public DateTimeOffset? CompletedUtc { get; private set; }

    public IReadOnlyCollection<HealthProfile> HealthProfiles =>
        _healthProfiles;

    public void AddHealthProfile(HealthProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        _healthProfiles.Add(profile);

        Touch();
    }


    public void ChangeStatus(InvestigationStatus status)
    {
        Status = status;

        if (status == InvestigationStatus.Closed)
        {
            CompletedUtc = DateTimeOffset.UtcNow;
        }

        Touch();
    }
}