using ADOps.Core.Entities;

namespace ADOps.Core.Interfaces;

/// <summary>
/// Defines the contract for asynchronously searching indexed knowledge.
/// </summary>
public interface IKnowledgeSearcher
{
    /// <summary>
    /// Searches indexed knowledge for matches relevant to the supplied query.
    /// </summary>
    Task<IReadOnlyCollection<KnowledgeMatch>> SearchAsync(
        KnowledgeQuery query,
        CancellationToken cancellationToken = default);
}