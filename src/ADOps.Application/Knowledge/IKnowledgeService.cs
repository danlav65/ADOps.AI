using ADOps.Core.Entities;

namespace ADOps.Application.Knowledge;

/// <summary>
/// Provides application-level access to knowledge retrieval.
/// </summary>
public interface IKnowledgeService
{
    /// <summary>
    /// Retrieves knowledge relevant to the supplied query.
    /// </summary>
    IReadOnlyCollection<KnowledgeMatch> Retrieve(
        KnowledgeQuery query);
}
