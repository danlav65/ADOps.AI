using ADOps.Core.Enums;
using ADOps.Core.Entities;
using ADOps.Core.Interfaces;

namespace ADOps.Infrastructure.Knowledge;

/// <summary>
/// Provides deterministic in-memory knowledge retrieval for development and testing.
/// </summary>
public sealed class InMemoryKnowledgeRetriever : IKnowledgeRetriever
{
    private static readonly DateTimeOffset FixtureRetrievedUtc =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static readonly IReadOnlyCollection<KnowledgeMatch> Knowledge =
    [
        new KnowledgeMatch
        {
            Source = "SITA AD Architecture",
            Description = "Active Directory replication failures can affect DNS, SYSVOL, GPO processing, and domain authentication.",
            Provenance = new KnowledgeSource
            {
                SourceId = "fixture-sita-ad-architecture",
                Title = "SITA AD Architecture",
                Publisher = "ADOps AI Test Fixture",
                SourceType = KnowledgeSourceType.Unknown,
                RetrievedUtc = FixtureRetrievedUtc
            }
        },
        new KnowledgeMatch
        {
            Source = "SITA AD Operations",
            Description = "Kerberos authentication issues should be investigated alongside domain controller replication health and patch baseline consistency.",
            Provenance = new KnowledgeSource
            {
                SourceId = "fixture-sita-ad-operations",
                Title = "SITA AD Operations",
                Publisher = "ADOps AI Test Fixture",
                SourceType = KnowledgeSourceType.Unknown,
                RetrievedUtc = FixtureRetrievedUtc
            }
        },
        new KnowledgeMatch
        {
            Source = "SITA Incident RCA",
            Description = "Patch baseline drift between domain controllers can be a contributing factor when investigating replication and authentication incidents.",
            Provenance = new KnowledgeSource
            {
                SourceId = "fixture-sita-incident-rca",
                Title = "SITA Incident RCA",
                Publisher = "ADOps AI Test Fixture",
                SourceType = KnowledgeSourceType.Unknown,
                RetrievedUtc = FixtureRetrievedUtc
            }
        }
    ];

    public IReadOnlyCollection<KnowledgeMatch> Retrieve(KnowledgeQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (string.IsNullOrWhiteSpace(query.Query))
        {
            return [];
        }

        var terms = query.Query
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var matches = Knowledge
            .Where(match =>
                terms.Any(term =>
                    match.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    match.Source.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .Take(Math.Max(0, query.MaxResults))
            .ToList();

        return matches;
    }
}
