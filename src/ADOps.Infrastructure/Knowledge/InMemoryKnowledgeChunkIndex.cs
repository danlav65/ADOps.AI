using ADOps.Core.Entities;

namespace ADOps.Infrastructure.Knowledge;

/// <summary>
/// Provides deterministic, in-memory indexing and keyword search
/// over knowledge document chunks.
/// </summary>
public sealed class InMemoryKnowledgeChunkIndex
{
    private readonly List<KnowledgeChunk> _chunks = [];

    private readonly HashSet<string> _chunkIds =
        new(StringComparer.Ordinal);

    /// <summary>
    /// Adds knowledge chunks to the index.
    /// Existing chunk IDs are ignored to prevent duplicate entries.
    /// </summary>
    public void Add(IEnumerable<KnowledgeChunk> chunks)
    {
        ArgumentNullException.ThrowIfNull(chunks);

        foreach (var chunk in chunks)
        {
            if (_chunkIds.Add(chunk.ChunkId))
            {
                _chunks.Add(chunk);
            }
        }
    }

    /// <summary>
    /// Replaces all indexed chunks belonging to a source.
    /// Validates replacements before modifying the index.
    /// </summary>
    public void ReplaceSource(
        string sourceId,
        IEnumerable<KnowledgeChunk> chunks)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceId);
        ArgumentNullException.ThrowIfNull(chunks);

        // Materialize the replacement before modifying existing content.
        var replacements = chunks.ToArray();

        // Every replacement must belong to the requested source.
        if (replacements.Any(chunk =>
            !string.Equals(
                chunk.SourceId,
                sourceId,
                StringComparison.Ordinal)))
        {
            throw new ArgumentException(
                "All replacement chunks must belong to the specified source.",
                nameof(chunks));
        }

        // Reject duplicate IDs within the replacement batch.
        // Otherwise Add() would silently discard later chunks.
        var replacementIds =
            new HashSet<string>(StringComparer.Ordinal);

        foreach (var chunk in replacements)
        {
            if (!replacementIds.Add(chunk.ChunkId))
            {
                throw new ArgumentException(
                    "Replacement chunks must have unique chunk IDs.",
                    nameof(chunks));
            }
        }

        // Reject IDs already owned by a different source.
        // Reusing IDs belonging to this source is allowed.
        var otherSourceChunkIds = _chunks
            .Where(chunk =>
                !string.Equals(
                    chunk.SourceId,
                    sourceId,
                    StringComparison.Ordinal))
            .Select(chunk => chunk.ChunkId)
            .ToHashSet(StringComparer.Ordinal);

        if (replacements.Any(chunk =>
            otherSourceChunkIds.Contains(chunk.ChunkId)))
        {
            throw new ArgumentException(
                "A replacement chunk ID is already owned by another source.",
                nameof(chunks));
        }

        // All validation has passed. Remove the old document.
        var existingChunks = _chunks
            .Where(chunk =>
                string.Equals(
                    chunk.SourceId,
                    sourceId,
                    StringComparison.Ordinal))
            .ToArray();

        foreach (var chunk in existingChunks)
        {
            _chunkIds.Remove(chunk.ChunkId);
        }

        _chunks.RemoveAll(chunk =>
            string.Equals(
                chunk.SourceId,
                sourceId,
                StringComparison.Ordinal));

        // Add the validated replacement chunks.
        Add(replacements);
    }

    /// <summary>
    /// Searches indexed chunks using case-insensitive keyword matching.
    /// Results are ranked by the number of distinct matching query terms.
    /// </summary>
    public IReadOnlyCollection<KnowledgeMatch> Search(
        KnowledgeQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (string.IsNullOrWhiteSpace(query.Query) ||
            query.MaxResults <= 0)
        {
            return [];
        }

        var terms = query.Query
            .Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return _chunks
            .Select(chunk => new
            {
                Chunk = chunk,

                MatchingTerms = terms.Count(term =>
                    chunk.Content.Contains(
                        term,
                        StringComparison.OrdinalIgnoreCase) ||
                    chunk.Provenance.Title.Contains(
                        term,
                        StringComparison.OrdinalIgnoreCase))
            })
            .Where(result => result.MatchingTerms > 0)
            .OrderByDescending(result => result.MatchingTerms)
            .ThenBy(
                result => result.Chunk.SourceId,
                StringComparer.Ordinal)
            .ThenBy(result => result.Chunk.Sequence)
            .ThenBy(
                result => result.Chunk.ChunkId,
                StringComparer.Ordinal)
            .Take(query.MaxResults)
            .Select(result => new KnowledgeMatch
            {
                Source = result.Chunk.Provenance.Title,
                Description = result.Chunk.Content,
                Provenance = result.Chunk.Provenance
            })
            .ToArray();
    }
}