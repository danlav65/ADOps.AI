using ADOps.Core.Entities;
using ADOps.Core.Enums;
using ADOps.Core.Interfaces;
using ADOps.Infrastructure;
using ADOps.Infrastructure.Knowledge;
using Microsoft.Extensions.DependencyInjection;

namespace ADOps.Infrastructure.Tests.Knowledge;

public sealed class MicrosoftLearnDependencyInjectionTests
{
    [Fact]
    public void AddInfrastructure_RegistersMicrosoftLearnFetcher()
    {
        var services = new ServiceCollection();

        services.AddInfrastructure();

        using var provider = services.BuildServiceProvider(
            validateScopes: true);

        var fetcher =
            provider.GetRequiredService<
                MicrosoftLearnDocumentFetcher>();

        var secondResolution =
            provider.GetRequiredService<
                MicrosoftLearnDocumentFetcher>();

        Assert.NotNull(fetcher);
        Assert.Same(fetcher, secondResolution);
    }

    [Fact]
    public void AddInfrastructure_RegistersCompositeKnowledgeRetriever()
    {
        var services = new ServiceCollection();

        services.AddInfrastructure();

        using var provider = services.BuildServiceProvider(
            validateScopes: true);

        using var scope = provider.CreateScope();

        var retriever =
            scope.ServiceProvider.GetRequiredService<
                IKnowledgeRetriever>();

        Assert.IsType<CompositeKnowledgeRetriever>(retriever);

        var fixtureRetriever =
            scope.ServiceProvider.GetRequiredService<
                InMemoryKnowledgeRetriever>();

        Assert.NotNull(fixtureRetriever);
    }

    [Fact]
    public void AddInfrastructure_RegistersMicrosoftLearnIndexingPipeline()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddInfrastructure();

        using var provider = services.BuildServiceProvider(
            validateScopes: true);

        // Act
        using var firstScope = provider.CreateScope();

        var extractor =
            firstScope.ServiceProvider.GetRequiredService<
                IMicrosoftLearnDocumentExtractor>();

        var ingestionService =
            firstScope.ServiceProvider.GetRequiredService<
                MicrosoftLearnDocumentIngestionService>();

        var chunker =
            firstScope.ServiceProvider.GetRequiredService<
                MicrosoftLearnDocumentChunker>();

        var indexingService =
            firstScope.ServiceProvider.GetRequiredService<
                MicrosoftLearnKnowledgeIndexingService>();

        var firstIndex =
            firstScope.ServiceProvider.GetRequiredService<
                InMemoryKnowledgeChunkIndex>();

        using var secondScope = provider.CreateScope();

        var secondIndex =
            secondScope.ServiceProvider.GetRequiredService<
                InMemoryKnowledgeChunkIndex>();

        // Assert
        Assert.NotNull(extractor);
        Assert.NotNull(ingestionService);
        Assert.NotNull(chunker);
        Assert.NotNull(indexingService);

        // Indexed knowledge must be shared across scopes.
        Assert.Same(firstIndex, secondIndex);
    }

    [Fact]
    public void AddInfrastructure_IndexedKnowledgeIsRetrievableThroughInterface()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddInfrastructure();

        using var provider = services.BuildServiceProvider(
            validateScopes: true);

        var index =
            provider.GetRequiredService<
                InMemoryKnowledgeChunkIndex>();

        index.Add(
        [
            new KnowledgeChunk
            {
                ChunkId =
                    "https://learn.microsoft.com/test#chunk-0",

                SourceId =
                    "https://learn.microsoft.com/test",

                Content =
                    "Microsoft connectivity troubleshooting guidance.",

                Sequence = 0,

                Provenance = new KnowledgeSource
                {
                    SourceId =
                        "https://learn.microsoft.com/test",

                    Title =
                        "Microsoft Learn Connectivity Guide",

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

        using var scope = provider.CreateScope();

        var retriever =
            scope.ServiceProvider.GetRequiredService<
                IKnowledgeRetriever>();

        var query = new KnowledgeQuery
        {
            Query = "connectivity",
            MaxResults = 10
        };

        // Act
        var results = retriever.Retrieve(query);

        // Assert
        Assert.Contains(
            results,
            result =>
                result.Source ==
                "Microsoft Learn Connectivity Guide" &&
                result.Description.Contains(
                    "connectivity troubleshooting",
                    StringComparison.OrdinalIgnoreCase) &&
                result.Provenance?.SourceId ==
                    "https://learn.microsoft.com/test");
    }
}