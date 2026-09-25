using ADOps.Core.Entities;
using ADOps.Core.Enums;
using ADOps.Infrastructure.Knowledge;

namespace ADOps.Infrastructure.Tests.Knowledge;

public sealed class InMemoryKnowledgeChunkIndexTests
{
    [Fact]
    public void Search_FindsIndexedChunk_CaseInsensitiveAndPreservesProvenance()
    {
        // Arrange
        var sourceUri = new Uri(
            "https://learn.microsoft.com/en-us/windows-server/");

        var retrievedUtc = new DateTimeOffset(
            2026, 9, 21, 12, 0, 0, TimeSpan.Zero);

        var provenance = new KnowledgeSource
        {
            SourceId = "learn-replication",
            Title = "Active Directory replication troubleshooting",
            Publisher = "Microsoft",
            SourceType = KnowledgeSourceType.MicrosoftDocumentation,
            SourceUri = sourceUri,
            RetrievedUtc = retrievedUtc
        };

        var chunk = new KnowledgeChunk
        {
            ChunkId = "learn-replication#chunk-0",
            SourceId = "learn-replication",
            Content = "Use repadmin to investigate Active Directory replication failures.",
            Sequence = 0,
            Provenance = provenance
        };

        var index = new InMemoryKnowledgeChunkIndex();

        index.Add([chunk]);

        // Act
        var matches = index.Search(
            new KnowledgeQuery
            {
                Query = "REPLICATION",
                MaxResults = 5
            });

        // Assert
        var match = Assert.Single(matches);

        Assert.Equal(
            "Active Directory replication troubleshooting",
            match.Source);

        Assert.Equal(
            chunk.Content,
            match.Description);

        Assert.Same(
            provenance,
            match.Provenance);

        Assert.Equal(
            sourceUri,
            match.Provenance!.SourceUri);
    }

    [Fact]
    public void Search_RanksChunksByNumberOfMatchingQueryTerms()
    {
        // Arrange
        var provenance = new KnowledgeSource
        {
            SourceId = "learn-replication",
            Title = "Active Directory troubleshooting",
            Publisher = "Microsoft",
            SourceType = KnowledgeSourceType.MicrosoftDocumentation,
            SourceUri = new Uri(
                "https://learn.microsoft.com/en-us/windows-server/"),
            RetrievedUtc = new DateTimeOffset(
                2026, 9, 21, 12, 0, 0, TimeSpan.Zero)
        };

        var lowRelevance = new KnowledgeChunk
        {
            ChunkId = "learn-replication#chunk-0",
            SourceId = provenance.SourceId,
            Content = "Investigate replication health.",
            Sequence = 0,
            Provenance = provenance
        };

        var highRelevance = new KnowledgeChunk
        {
            ChunkId = "learn-replication#chunk-1",
            SourceId = provenance.SourceId,
            Content = "Investigate replication RPC failure.",
            Sequence = 1,
            Provenance = provenance
        };

        var index = new InMemoryKnowledgeChunkIndex();

        // Intentionally index the less relevant chunk first.
        index.Add([lowRelevance, highRelevance]);

        // Act
        var matches = index.Search(
            new KnowledgeQuery
            {
                Query = "replication RPC failure",
                MaxResults = 2
            }).ToArray();

        // Assert
        Assert.Equal(2, matches.Length);

        Assert.Equal(
            highRelevance.Content,
            matches[0].Description);

        Assert.Equal(
            lowRelevance.Content,
            matches[1].Description);

        Assert.Same(
            provenance,
            matches[0].Provenance);
    }

    [Fact]
    public void Search_AppliesMaxResultsAfterRelevanceRanking()
    {
        // Arrange
        var provenance = new KnowledgeSource
        {
            SourceId = "learn-replication",
            Title = "Active Directory troubleshooting",
            Publisher = "Microsoft",
            SourceType = KnowledgeSourceType.MicrosoftDocumentation,
            RetrievedUtc = new DateTimeOffset(
                2026, 9, 21, 12, 0, 0, TimeSpan.Zero)
        };

        var index = new InMemoryKnowledgeChunkIndex();

        index.Add(
        [
            new KnowledgeChunk
            {
                ChunkId = "chunk-0",
                SourceId = provenance.SourceId,
                Content = "Investigate replication health.",
                Sequence = 0,
                Provenance = provenance
            },
            new KnowledgeChunk
            {
                ChunkId = "chunk-1",
                SourceId = provenance.SourceId,
                Content = "Investigate replication and RPC failures.",
                Sequence = 1,
                Provenance = provenance
            },
            new KnowledgeChunk
            {
                ChunkId = "chunk-2",
                SourceId = provenance.SourceId,
                Content = "Investigate replication RPC failure and patch drift.",
                Sequence = 2,
                Provenance = provenance
            }
        ]);

        // Act
        var matches = index.Search(
            new KnowledgeQuery
            {
                Query = "replication RPC patch",
                MaxResults = 1
            });

        // Assert
        var match = Assert.Single(matches);

        Assert.Equal(
            "Investigate replication RPC failure and patch drift.",
            match.Description);
    }

    [Fact]
    public void Search_EqualRelevance_UsesDeterministicOrdering()
    {
        // Arrange
        var provenance = new KnowledgeSource
        {
            SourceId = "learn-replication",
            Title = "Active Directory troubleshooting",
            Publisher = "Microsoft",
            SourceType = KnowledgeSourceType.MicrosoftDocumentation,
            RetrievedUtc = new DateTimeOffset(
                2026, 9, 21, 12, 0, 0, TimeSpan.Zero)
        };

        var index = new InMemoryKnowledgeChunkIndex();

        // Intentionally insert chunks out of sequence.
        index.Add(
        [
            new KnowledgeChunk
            {
                ChunkId = "chunk-2",
                SourceId = provenance.SourceId,
                Content = "Replication guidance: third section.",
                Sequence = 2,
                Provenance = provenance
            },
            new KnowledgeChunk
            {
                ChunkId = "chunk-0",
                SourceId = provenance.SourceId,
                Content = "Replication guidance: first section.",
                Sequence = 0,
                Provenance = provenance
            },
            new KnowledgeChunk
            {
                ChunkId = "chunk-1",
                SourceId = provenance.SourceId,
                Content = "Replication guidance: second section.",
                Sequence = 1,
                Provenance = provenance
            }
        ]);

        // Act
        var matches = index.Search(
            new KnowledgeQuery
            {
                Query = "replication",
                MaxResults = 3
            }).ToArray();

        // Assert
        Assert.Equal(3, matches.Length);

        Assert.Equal(
            "Replication guidance: first section.",
            matches[0].Description);

        Assert.Equal(
            "Replication guidance: second section.",
            matches[1].Description);

        Assert.Equal(
            "Replication guidance: third section.",
            matches[2].Description);
    }

    [Fact]
    public void Add_SameChunkTwice_DoesNotReturnDuplicateMatches()
    {
        // Arrange
        var provenance = new KnowledgeSource
        {
            SourceId = "learn-replication",
            Title = "Active Directory troubleshooting",
            Publisher = "Microsoft",
            SourceType = KnowledgeSourceType.MicrosoftDocumentation,
            RetrievedUtc = new DateTimeOffset(
                2026, 9, 21, 12, 0, 0, TimeSpan.Zero)
        };

        var chunk = new KnowledgeChunk
        {
            ChunkId = "learn-replication#chunk-0",
            SourceId = provenance.SourceId,
            Content = "Investigate Active Directory replication failures.",
            Sequence = 0,
            Provenance = provenance
        };

        var index = new InMemoryKnowledgeChunkIndex();

        // Simulate ingesting the same document twice.
        index.Add([chunk]);
        index.Add([chunk]);

        // Act
        var matches = index.Search(
            new KnowledgeQuery
            {
                Query = "replication",
                MaxResults = 5
            });

        // Assert
        var match = Assert.Single(matches);

        Assert.Equal(chunk.Content, match.Description);
        Assert.Same(provenance, match.Provenance);
    }

    [Fact]
    public void ReplaceSource_ReplacesExistingChunksAndRemovesStaleChunks()
    {
        // Arrange
        var provenance = new KnowledgeSource
        {
            SourceId = "learn-replication",
            Title = "Active Directory troubleshooting",
            Publisher = "Microsoft",
            SourceType = KnowledgeSourceType.MicrosoftDocumentation,
            RetrievedUtc = new DateTimeOffset(
                2026, 9, 21, 12, 0, 0, TimeSpan.Zero)
        };

        var index = new InMemoryKnowledgeChunkIndex();

        index.Add(
        [
            new KnowledgeChunk
            {
                ChunkId = "learn-replication#chunk-0",
                SourceId = provenance.SourceId,
                Content = "Old replication guidance.",
                Sequence = 0,
                Provenance = provenance
            },
            new KnowledgeChunk
            {
                ChunkId = "learn-replication#chunk-1",
                SourceId = provenance.SourceId,
                Content = "Obsolete replication instructions.",
                Sequence = 1,
                Provenance = provenance
            }
        ]);

        // Act: the refreshed document contains only one updated chunk.
        index.ReplaceSource(
            provenance.SourceId,
            [
                new KnowledgeChunk
                {
                    ChunkId = "learn-replication#chunk-0",
                    SourceId = provenance.SourceId,
                    Content = "Updated replication guidance.",
                    Sequence = 0,
                    Provenance = provenance
                }
            ]);

        var matches = index.Search(
            new KnowledgeQuery
            {
                Query = "replication",
                MaxResults = 5
            });

        // Assert
        var match = Assert.Single(matches);

        Assert.Equal(
            "Updated replication guidance.",
            match.Description);

        Assert.Same(provenance, match.Provenance);
    }

    [Fact]
    public void ReplaceSource_WrongSourceId_PreservesExistingChunks()
    {
        // Arrange
        var provenance = new KnowledgeSource
        {
            SourceId = "learn-replication",
            Title = "Active Directory troubleshooting",
            Publisher = "Microsoft",
            SourceType = KnowledgeSourceType.MicrosoftDocumentation,
            RetrievedUtc = new DateTimeOffset(
                2026, 9, 21, 12, 0, 0, TimeSpan.Zero)
        };

        var originalChunk = new KnowledgeChunk
        {
            ChunkId = "learn-replication#chunk-0",
            SourceId = provenance.SourceId,
            Content = "Original replication guidance.",
            Sequence = 0,
            Provenance = provenance
        };

        var invalidReplacement = new KnowledgeChunk
        {
            ChunkId = "other-source#chunk-0",
            SourceId = "other-source",
            Content = "Replacement replication guidance.",
            Sequence = 0,
            Provenance = provenance
        };

        var index = new InMemoryKnowledgeChunkIndex();

        index.Add([originalChunk]);

        // Act: attempt to replace one source with another source's chunk.
        Assert.Throws<ArgumentException>(() =>
            index.ReplaceSource(
                provenance.SourceId,
                [invalidReplacement]));

        // Assert: the original indexed content must remain intact.
        var matches = index.Search(
            new KnowledgeQuery
            {
                Query = "replication",
                MaxResults = 5
            });

        var match = Assert.Single(matches);

        Assert.Equal(
            originalChunk.Content,
            match.Description);

        Assert.Same(
            provenance,
            match.Provenance);
    }

    [Fact]
    public void ReplaceSource_ChunkIdOwnedByAnotherSource_PreservesExistingChunks()
    {
        // Arrange
        var retrievedUtc = new DateTimeOffset(
            2026, 9, 21, 12, 0, 0, TimeSpan.Zero);

        var sourceA = new KnowledgeSource
        {
            SourceId = "source-a",
            Title = "Source A",
            Publisher = "Microsoft",
            RetrievedUtc = retrievedUtc
        };

        var sourceB = new KnowledgeSource
        {
            SourceId = "source-b",
            Title = "Source B",
            Publisher = "Microsoft",
            RetrievedUtc = retrievedUtc
        };

        var index = new InMemoryKnowledgeChunkIndex();

        index.Add(
        [
            new KnowledgeChunk
            {
                ChunkId = "shared-chunk-id",
                SourceId = sourceA.SourceId,
                Content = "Original authentication guidance.",
                Sequence = 0,
                Provenance = sourceA
            },
            new KnowledgeChunk
            {
                ChunkId = "source-b#chunk-0",
                SourceId = sourceB.SourceId,
                Content = "Original replication guidance.",
                Sequence = 0,
                Provenance = sourceB
            }
        ]);

        var conflictingReplacement = new KnowledgeChunk
        {
            ChunkId = "shared-chunk-id",
            SourceId = sourceB.SourceId,
            Content = "Updated replication guidance.",
            Sequence = 0,
            Provenance = sourceB
        };

        // Act
        Assert.Throws<ArgumentException>(() =>
            index.ReplaceSource(
                sourceB.SourceId,
                [conflictingReplacement]));

        // Assert: both original sources remain searchable.
        var authenticationMatches = index.Search(
            new KnowledgeQuery
            {
                Query = "authentication",
                MaxResults = 5
            });

        var replicationMatches = index.Search(
            new KnowledgeQuery
            {
                Query = "replication",
                MaxResults = 5
            });

        Assert.Equal(
            "Original authentication guidance.",
            Assert.Single(authenticationMatches).Description);

        Assert.Equal(
            "Original replication guidance.",
            Assert.Single(replicationMatches).Description);
    }

    [Fact]
    public void ReplaceSource_DuplicateChunkIds_PreservesExistingChunks()
    {
        // Arrange
        var provenance = new KnowledgeSource
        {
            SourceId = "learn-replication",
            Title = "Active Directory troubleshooting",
            Publisher = "Microsoft",
            SourceType = KnowledgeSourceType.MicrosoftDocumentation,
            RetrievedUtc = new DateTimeOffset(
                2026, 9, 21, 12, 0, 0, TimeSpan.Zero)
        };

        var originalChunk = new KnowledgeChunk
        {
            ChunkId = "learn-replication#chunk-0",
            SourceId = provenance.SourceId,
            Content = "Original replication guidance.",
            Sequence = 0,
            Provenance = provenance
        };

        var index = new InMemoryKnowledgeChunkIndex();

        index.Add([originalChunk]);

        // Both replacement chunks incorrectly use the same ID.
        var replacements = new[]
        {
            new KnowledgeChunk
            {
                ChunkId = "learn-replication#chunk-0",
                SourceId = provenance.SourceId,
                Content = "Updated replication guidance.",
                Sequence = 0,
                Provenance = provenance
            },
            new KnowledgeChunk
            {
                ChunkId = "learn-replication#chunk-0",
                SourceId = provenance.SourceId,
                Content = "Conflicting replication guidance.",
                Sequence = 1,
                Provenance = provenance
            }
        };

        // Act: reject the entire invalid replacement.
        Assert.Throws<ArgumentException>(() =>
            index.ReplaceSource(provenance.SourceId, replacements));

        // Assert: the original content remains unchanged.
        var matches = index.Search(
            new KnowledgeQuery
            {
                Query = "replication",
                MaxResults = 5
            });

        var match = Assert.Single(matches);

        Assert.Equal(originalChunk.Content, match.Description);
        Assert.Same(provenance, match.Provenance);
    }
}