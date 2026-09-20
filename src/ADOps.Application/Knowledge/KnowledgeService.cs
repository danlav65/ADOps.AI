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

    public IReadOnlyCollection<KnowledgeMatch> Retrieve(
        KnowledgeQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return _knowledgeRetriever.Retrieve(query);
    }

    public KnowledgeRetrievalResult RetrieveWithContext(
        KnowledgeQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var matches = _knowledgeRetriever.Retrieve(query);

        return new KnowledgeRetrievalResult
        {
            Matches = matches,
            Conflicts = [],
            RetrievedUtc = DateTimeOffset.UtcNow
        };
    }
}
