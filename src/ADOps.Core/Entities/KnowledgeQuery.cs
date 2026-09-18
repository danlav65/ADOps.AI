namespace ADOps.Core.Entities;

/// <summary>
/// Represents a knowledge retrieval query.
/// </summary>
public sealed class KnowledgeQuery
{
    /// <summary>
    /// The text to search for.
    /// </summary>
    public required string Query { get; init; }

    /// <summary>
    /// Optional Active Directory site context for the query.
    /// </summary>
    public string? Site { get; init; }

    /// <summary>
    /// Maximum number of knowledge matches to return.
    /// </summary>
    public int MaxResults { get; init; } = 5;
}
