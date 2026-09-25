using ADOps.Core.Entities;
using ADOps.Core.Enums;
using ADOps.Infrastructure.Knowledge;

namespace ADOps.Infrastructure.Tests.Knowledge;

public sealed class CompositeKnowledgeRetrieverTests
{
    [Fact]
    public void Retrieve_ReturnsFixtureAndIndexedKnowledge()
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
        var results = retriever.Retrieve(query);

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
    public void Retrieve_RespectsGlobalMaximumResults()
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
        var results = retriever.Retrieve(query);

        // Assert
        Assert.Single(results);
    }

    [Fact]
    public void Retrieve_ReturnsEmptyWhenMaximumResultsIsZero()
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
        var results = retriever.Retrieve(query);

        // Assert
        Assert.Empty(results);
    }
}