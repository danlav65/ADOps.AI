using ADOps.Core.Entities;
using ADOps.Core.Interfaces;

namespace ADOps.Application.Knowledge;

/// <summary>
/// Provides application-level access to knowledge retrieval.
/// </summary>
public sealed class KnowledgeService : IKnowledgeService
{
    private readonly IKnowledgeRetriever _knowledgeRetriever;

    public KnowledgeService(
        IKnowledgeRetriever knowledgeRetriever)
    {
        _knowledgeRetriever =
            knowledgeRetriever ??
            throw new ArgumentNullException(nameof(knowledgeRetriever));
    }

    public async Task<IReadOnlyCollection<KnowledgeMatch>> RetrieveAsync(
        KnowledgeQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        return await _knowledgeRetriever.RetrieveAsync(
            query,
            cancellationToken);
    }

    public async Task<KnowledgeRetrievalResult> RetrieveWithContextAsync(
        KnowledgeQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var matches =
            await _knowledgeRetriever.RetrieveAsync(
                query,
                cancellationToken);

        return new KnowledgeRetrievalResult
        {
            Matches = matches,
            Conflicts = [],
            RetrievedUtc = DateTimeOffset.UtcNow
        };
    }
}
