using ADOps.Core.Entities;
using ADOps.Core.Interfaces;

namespace ADOps.Infrastructure.Knowledge;

/// <summary>
/// Combines deterministic fixture knowledge with dynamically
/// indexed knowledge.
/// </summary>
public sealed class CompositeKnowledgeRetriever : IKnowledgeRetriever
{
    private readonly InMemoryKnowledgeRetriever _fixtureRetriever;
    private readonly IKnowledgeSearcher _knowledgeSearcher;

    public CompositeKnowledgeRetriever(
        InMemoryKnowledgeRetriever fixtureRetriever,
        IKnowledgeSearcher knowledgeSearcher)
    {
        _fixtureRetriever = fixtureRetriever
            ?? throw new ArgumentNullException(
                nameof(fixtureRetriever));

        _knowledgeSearcher = knowledgeSearcher
            ?? throw new ArgumentNullException(
                nameof(knowledgeSearcher));
    }

    public async Task<IReadOnlyCollection<KnowledgeMatch>> RetrieveAsync(
        KnowledgeQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(query.Query) ||
            query.MaxResults <= 0)
        {
            return [];
        }

        var fixtureMatches =
            await _fixtureRetriever.RetrieveAsync(
                query,
                cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        var indexedMatches =
            await _knowledgeSearcher.SearchAsync(
                query,
                cancellationToken);

        return fixtureMatches
            .Concat(indexedMatches)
            .Take(query.MaxResults)
            .ToArray();
    }
}