namespace ADOps.Portal.Services;

public sealed class SfoP1DemoContext
{
    public required string IncidentNumber { get; init; }

    public required string Site { get; init; }

    public required string Domain { get; init; }

    public required string Environment { get; init; }

    public required string Severity { get; init; }

    public required DateTimeOffset DetectedUtc { get; init; }

    public required string BusinessImpact { get; init; }

    public required string TechnicalImpact { get; init; }

    public required IReadOnlyCollection<string> AffectedDomainControllers
    {
        get;
        init;
    }

    public required IReadOnlyCollection<string> KeySymptoms
    {
        get;
        init;
    }
}