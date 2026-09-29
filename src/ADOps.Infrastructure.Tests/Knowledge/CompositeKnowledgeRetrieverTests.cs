using ADOps.Core.Entities;
using ADOps.Core.Enums;
using ADOps.Infrastructure.Knowledge;

namespace ADOps.Infrastructure.Tests.Knowledge;

public sealed class CompositeKnowledgeRetrieverTests
{
    [Fact]
    public async Task Retrieve_ReturnsFixtureAndIndexedKnowledge()
    {
        // Arrange
        var index = new InMemoryKnowledgeChunkIndex();

        index.Add(
        [
            new KnowledgeChunk
            {
                ChunkId =
                    "https://learn.microsoft.com/test#chunk-0",

                SourceId =
                    "https://learn.microsoft.com/test",

                Content =
                    "Microsoft replication troubleshooting guidance.",

                Sequence = 0,

                Provenance = new KnowledgeSource
                {
                    SourceId =
                        "https://learn.microsoft.com/test",

                    Title =
                        "Microsoft Learn Replication Guide",

                    Publisher = "Microsoft",

                    SourceType =
                        KnowledgeSourceType.MicrosoftDocumentation,

                    SourceUri =
                        new Uri(
                            "https://learn.microsoft.com/test"),

                    RetrievedUtc =
                        new DateTimeOffset(
                            2026,
                            9,
                            23,
                            12,
                            0,
                            0,
                            TimeSpan.Zero)
                }
            }
        ]);

        var retriever =
            new CompositeKnowledgeRetriever(
                new InMemoryKnowledgeRetriever(),
                index);

        var query = new KnowledgeQuery
        {
            Query = "replication",
            MaxResults = 10
        };

        // Act
        var results =
            await retriever.RetrieveAsync(query);

        // Assert
        Assert.Contains(
            results,
            result =>
                result.Source ==
                "SITA AD Architecture");

        Assert.Contains(
            results,
            result =>
                result.Source ==
                "Microsoft Learn Replication Guide");
    }

    [Fact]
    public async Task Retrieve_RespectsGlobalMaximumResults()
    {
        // Arrange
        var index = new InMemoryKnowledgeChunkIndex();

        index.Add(
        [
            new KnowledgeChunk
            {
                ChunkId =
                    "https://learn.microsoft.com/test#chunk-0",

                SourceId =
                    "https://learn.microsoft.com/test",

                Content =
                    "Microsoft replication troubleshooting guidance.",

                Sequence = 0,

                Provenance = new KnowledgeSource
                {
                    SourceId =
                        "https://learn.microsoft.com/test",

                    Title =
                        "Microsoft Learn Replication Guide",

                    Publisher = "Microsoft",

                    SourceType =
                        KnowledgeSourceType.MicrosoftDocumentation,

                    SourceUri =
                        new Uri(
                            "https://learn.microsoft.com/test"),

                    RetrievedUtc =
                        new DateTimeOffset(
                            2026,
                            9,
                            23,
                            12,
                            0,
                            0,
                            TimeSpan.Zero)
                }
            }
        ]);

        var retriever =
            new CompositeKnowledgeRetriever(
                new InMemoryKnowledgeRetriever(),
                index);

        var query = new KnowledgeQuery
        {
            Query = "replication",
            MaxResults = 1
        };

        // Act
        var results =
            await retriever.RetrieveAsync(query);

        // Assert
        Assert.Single(results);
    }

    [Fact]
    public async Task Retrieve_ReturnsEmptyWhenMaximumResultsIsZero()
    {
        // Arrange
        var index = new InMemoryKnowledgeChunkIndex();

        var retriever =
            new CompositeKnowledgeRetriever(
                new InMemoryKnowledgeRetriever(),
                index);

        var query = new KnowledgeQuery
        {
            Query = "replication",
            MaxResults = 0
        };

        // Act
        var results =
            await retriever.RetrieveAsync(query);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task RetrieveAsync_ReturnsFixtureAndIndexedKnowledge()
    {
        // Arrange
        var index = new InMemoryKnowledgeChunkIndex();

        index.Add(
        [
            new KnowledgeChunk
            {
                ChunkId =
                    "https://learn.microsoft.com/test#chunk-0",

                SourceId =
                    "https://learn.microsoft.com/test",

                Content =
                    "Microsoft replication troubleshooting guidance.",

                Sequence = 0,

                Provenance = new KnowledgeSource
                {
                    SourceId =
                        "https://learn.microsoft.com/test",

                    Title =
                        "Microsoft Learn Replication Guide",

                    Publisher = "Microsoft",

                    SourceType =
                        KnowledgeSourceType.MicrosoftDocumentation,

                    SourceUri =
                        new Uri(
                            "https://learn.microsoft.com/test"),

                    RetrievedUtc =
                        new DateTimeOffset(
                            2026,
                            9,
                            23,
                            12,
                            0,
                            0,
                            TimeSpan.Zero)
                }
            }
        ]);

        var retriever =
            new CompositeKnowledgeRetriever(
                new InMemoryKnowledgeRetriever(),
                index);

        var query = new KnowledgeQuery
        {
            Query = "replication",
            MaxResults = 10
        };

        // Act
        var results =
            await retriever.RetrieveAsync(query);

        // Assert
        Assert.Contains(
            results,
            result =>
                result.Source ==
                "SITA AD Architecture");

        Assert.Contains(
            results,
            result =>
                result.Source ==
                "Microsoft Learn Replication Guide");
    }

    [Fact]
    public async Task RetrieveAsync_CanceledToken_ThrowsOperationCanceledException()
    {
        // Arrange
        var retriever =
            new CompositeKnowledgeRetriever(
                new InMemoryKnowledgeRetriever(),
                new InMemoryKnowledgeChunkIndex());

        var query = new KnowledgeQuery
        {
            Query = "replication",
            MaxResults = 5
        };

        using var cancellationTokenSource =
            new CancellationTokenSource();

        cancellationTokenSource.Cancel();

        // Act
        var exception =
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => retriever.RetrieveAsync(
                    query,
                    cancellationTokenSource.Token));

        // Assert
        Assert.Equal(
            cancellationTokenSource.Token,
            exception.CancellationToken);
    }
}
