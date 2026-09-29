using ADOps.Core.Entities;

namespace ADOps.Application.Knowledge;

/// <summary>
/// Provides application-level access to knowledge retrieval.
/// </summary>
public interface IKnowledgeService
{
    /// <summary>
    /// Asynchronously retrieves knowledge relevant to the supplied query.
    /// </summary>
    Task<IReadOnlyCollection<KnowledgeMatch>> RetrieveAsync(
        KnowledgeQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves knowledge together with conflict-analysis status.
    /// </summary>
    Task<KnowledgeRetrievalResult> RetrieveWithContextAsync(
        KnowledgeQuery query,
        CancellationToken cancellationToken = default);
}