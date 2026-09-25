using System.Net;
using System.Net.Http.Headers;
using ADOps.Core.Entities;
using ADOps.Infrastructure.Knowledge;

namespace ADOps.Infrastructure.Tests.Knowledge;

public sealed class MicrosoftLearnKnowledgeIndexingServiceTests
{
    [Fact]
    public async Task IndexAsync_SuccessfulRefreshReplacesStaleContent()
    {
        // Arrange
        var sourceUri = new Uri(
            "https://learn.microsoft.com/en-us/windows-server/");

        const string originalHtml = """
            <main>
                <article>
                    <h1>Directory troubleshooting</h1>
                    <p>Legacy replication procedure.</p>
                </article>
            </main>
            """;

        const string updatedHtml = """
            <main>
                <article>
                    <h1>Directory troubleshooting</h1>
                    <p>Updated connectivity procedure.</p>
                </article>
            </main>
            """;

        var requestCount = 0;

        using var client = new HttpClient(
            new FakeHandler(() =>
            {
                requestCount++;

                var html = requestCount == 1
                    ? originalHtml
                    : updatedHtml;

                var response = new HttpResponseMessage(
                    HttpStatusCode.OK)
                {
                    Content = new StringContent(html)
                };

                response.Content.Headers.ContentType =
                    new MediaTypeHeaderValue("text/html");

                return response;
            }));

        var fetcher = new MicrosoftLearnDocumentFetcher(
            client,
            new MicrosoftLearnSourcePolicy());

        var ingestionService =
            new MicrosoftLearnDocumentIngestionService(
                fetcher,
                new MicrosoftLearnDocumentExtractor());

        var index = new InMemoryKnowledgeChunkIndex();

        var service = new MicrosoftLearnKnowledgeIndexingService(
            ingestionService,
            new MicrosoftLearnDocumentChunker(),
            index);

        // Establish the original indexed version.
        await service.IndexAsync(sourceUri);

        var originalMatches = index.Search(
            new KnowledgeQuery
            {
                Query = "replication",
                MaxResults = 5
            });

        Assert.Single(originalMatches);

        // Act: refresh the same source with updated content.
        await service.IndexAsync(sourceUri);

        // Assert: both HTTP requests completed.
        Assert.Equal(2, requestCount);

        // Updated guidance is searchable.
        var updatedMatches = index.Search(
            new KnowledgeQuery
            {
                Query = "connectivity",
                MaxResults = 5
            });

        var updatedMatch = Assert.Single(updatedMatches);

        Assert.Contains(
            "Updated connectivity procedure.",
            updatedMatch.Description);

        Assert.Equal(
            sourceUri.AbsoluteUri,
            updatedMatch.Provenance!.SourceId);

        // Obsolete guidance has been removed.
        var staleMatches = index.Search(
            new KnowledgeQuery
            {
                Query = "replication",
                MaxResults = 5
            });

        Assert.Empty(staleMatches);

        // Only one version of this single-chunk article remains.
        var allMatches = index.Search(
            new KnowledgeQuery
            {
                Query = "Directory",
                MaxResults = 10
            });

        Assert.Single(allMatches);
    }
    
    [Fact]
    public async Task IndexAsync_HttpFailurePreservesPreviouslyIndexedDocument()
    {
        // Arrange
        var sourceUri = new Uri(
            "https://learn.microsoft.com/en-us/windows-server/");

        const string originalHtml = """
            <main>
                <article>
                    <h1>Replication troubleshooting</h1>
                    <p>Original replication guidance.</p>
                </article>
            </main>
            """;

        var requestCount = 0;

        using var client = new HttpClient(
            new FakeHandler(() =>
            {
                requestCount++;

                if (requestCount == 1)
                {
                    var response = new HttpResponseMessage(
                        HttpStatusCode.OK)
                    {
                        Content = new StringContent(originalHtml)
                    };

                    response.Content.Headers.ContentType =
                        new MediaTypeHeaderValue("text/html");

                    return response;
                }

                return new HttpResponseMessage(
                    HttpStatusCode.InternalServerError);
            }));

        var fetcher = new MicrosoftLearnDocumentFetcher(
            client,
            new MicrosoftLearnSourcePolicy());

        var ingestionService =
            new MicrosoftLearnDocumentIngestionService(
                fetcher,
                new MicrosoftLearnDocumentExtractor());

        var index = new InMemoryKnowledgeChunkIndex();

        var service = new MicrosoftLearnKnowledgeIndexingService(
            ingestionService,
            new MicrosoftLearnDocumentChunker(),
            index);

        // First ingestion succeeds and populates the index.
        await service.IndexAsync(sourceUri);

        // Act: refreshing the same source fails.
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => service.IndexAsync(sourceUri));

        // Assert: the failure was observed.
        Assert.Equal(
            HttpStatusCode.InternalServerError,
            exception.StatusCode);

        Assert.Equal(2, requestCount);

        // The previously indexed article must remain searchable.
        var matches = index.Search(
            new KnowledgeQuery
            {
                Query = "replication",
                MaxResults = 5
            });

        var match = Assert.Single(matches);

        Assert.Contains(
            "Original replication guidance.",
            match.Description);

        Assert.Equal(
            sourceUri.AbsoluteUri,
            match.Provenance!.SourceId);
    }
    
    [Fact]
    public async Task IndexAsync_IngestsDocumentAndMakesChunksSearchable()
    {
        // Arrange
        var sourceUri = new Uri(
            "https://learn.microsoft.com/en-us/windows-server/");

        const string html = """
            <main>
              <article>
                <h1>Replication troubleshooting</h1>
                <p>Validate replication health.</p>
              </article>
            </main>
            """;

        using var client = new HttpClient(
            new FakeHandler(() =>
            {
                var response = new HttpResponseMessage(
                    HttpStatusCode.OK)
                {
                    Content = new StringContent(html)
                };

                response.Content.Headers.ContentType =
                    new MediaTypeHeaderValue("text/html");

                return response;
            }));

        var fetcher = new MicrosoftLearnDocumentFetcher(
            client,
            new MicrosoftLearnSourcePolicy());

        var ingestionService =
            new MicrosoftLearnDocumentIngestionService(
                fetcher,
                new MicrosoftLearnDocumentExtractor());

        var index = new InMemoryKnowledgeChunkIndex();

        var service = new MicrosoftLearnKnowledgeIndexingService(
            ingestionService,
            new MicrosoftLearnDocumentChunker(),
            index);

        // Act
        await service.IndexAsync(sourceUri);

        var matches = index.Search(
            new KnowledgeQuery
            {
                Query = "replication",
                MaxResults = 5
            });

        // Assert
        var match = Assert.Single(matches);

        Assert.Contains(
            "Validate replication health.",
            match.Description);

        Assert.NotNull(match.Provenance);

        Assert.Equal(
            sourceUri.AbsoluteUri,
            match.Provenance.SourceId);

        Assert.Equal(
            sourceUri,
            match.Provenance.SourceUri);
    }

    private sealed class FakeHandler(
        Func<HttpResponseMessage> responder)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var response = responder();
            response.RequestMessage = request;

            return Task.FromResult(response);
        }
    }
}