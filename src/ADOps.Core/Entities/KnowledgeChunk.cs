namespace ADOps.Core.Entities;

/// <summary>
/// Represents a searchable portion of a knowledge document.
/// </summary>
public sealed class KnowledgeChunk
{
    public required string ChunkId { get; init; }

    public required string SourceId { get; init; }

    public required string Content { get; init; }

    public required int Sequence { get; init; }

    public required KnowledgeSource Provenance { get; init; }
}