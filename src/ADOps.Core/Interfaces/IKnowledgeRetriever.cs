using ADOps.Core.Entities;

namespace ADOps.Core.Interfaces;

/// <summary>
/// Defines the contract for retrieving relevant knowledge for an investigation.
/// </summary>
public interface IKnowledgeRetriever
{
    /// <summary>
    /// Asynchronously retrieves knowledge matches for the supplied query.
    /// </summary>
    Task<IReadOnlyCollection<KnowledgeMatch>> RetrieveAsync(
        KnowledgeQuery query,
        CancellationToken cancellationToken = default);
}