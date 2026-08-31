namespace ADOps.Core.Entities;

public sealed class PatchEvidence
{
    public required string EvidenceId { get; init; }

    public required string InvestigationId { get; init; }

    public required string DomainController { get; init; }

    public required string Site { get; init; }

    public required string OperatingSystem { get; init; }

    public required string OsBuild { get; init; }

    public required string KnowledgeBaseArticle { get; init; }

    public required string PatchVersion { get; init; }

    public DateTimeOffset? InstalledUtc { get; init; }

    public bool Installed { get; init; }

    public required DateTimeOffset CollectedUtc { get; init; }
}