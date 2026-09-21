using ADOps.Core.Entities;

namespace ADOps.Core.Interfaces;

/// <summary>
/// Retrieves knowledge from approved external sources.
/// </summary>
public interface ILiveKnowledgeRetriever
{
    Task<KnowledgeRetrievalResult> RetrieveAsync(
        KnowledgeQuery query,
        CancellationToken cancellationToken = default);
}