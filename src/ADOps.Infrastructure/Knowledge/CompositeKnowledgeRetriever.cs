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
    private readonly InMemoryKnowledgeChunkIndex _chunkIndex;

    public CompositeKnowledgeRetriever(
        InMemoryKnowledgeRetriever fixtureRetriever,
        InMemoryKnowledgeChunkIndex chunkIndex)
    {
        _fixtureRetriever = fixtureRetriever
            ?? throw new ArgumentNullException(
                nameof(fixtureRetriever));

        _chunkIndex = chunkIndex
            ?? throw new ArgumentNullException(
                nameof(chunkIndex));
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
            _chunkIndex.Search(query);

        return fixtureMatches
            .Concat(indexedMatches)
            .Take(query.MaxResults)
            .ToArray();
    }
}