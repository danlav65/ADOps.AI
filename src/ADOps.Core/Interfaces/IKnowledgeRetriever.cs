using ADOps.Core.Entities;

namespace ADOps.Core.Interfaces;

/// <summary>
/// Defines the contract for retrieving relevant knowledge for an investigation.
/// </summary>
public interface IKnowledgeRetriever
{
    /// <summary>
    /// Retrieves knowledge matches for the supplied query.
    /// </summary>
    IReadOnlyCollection<KnowledgeMatch> Retrieve(KnowledgeQuery query);
}
