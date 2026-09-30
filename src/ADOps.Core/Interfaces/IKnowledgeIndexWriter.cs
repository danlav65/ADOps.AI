using ADOps.Core.Entities;

namespace ADOps.Core.Interfaces;

/// <summary>
/// Defines the contract for asynchronously replacing indexed knowledge.
/// </summary>
public interface IKnowledgeIndexWriter
{
    /// <summary>
    /// Replaces all indexed chunks belonging to the specified source.
    /// </summary>
    Task ReplaceSourceAsync(
        string sourceId,
        IEnumerable<KnowledgeChunk> chunks,
        CancellationToken cancellationToken = default);
}