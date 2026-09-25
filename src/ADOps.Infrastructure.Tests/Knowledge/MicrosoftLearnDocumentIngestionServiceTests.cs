using System.Net;
using System.Net.Http.Headers;
using ADOps.Infrastructure.Knowledge;

namespace ADOps.Infrastructure.Tests.Knowledge;

public sealed class MicrosoftLearnDocumentIngestionServiceTests
{
    [Fact]
    public async Task IngestAsync_FetchesAndExtractsArticle()
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

        var requestCount = 0;

        using var client = new HttpClient(
            new FakeHandler(request =>
            {
                requestCount++;

                Assert.Equal(sourceUri, request.RequestUri);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
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

        var service = new MicrosoftLearnDocumentIngestionService(
            fetcher,
            new MicrosoftLearnDocumentExtractor());

        // Act
        var result = await service.IngestAsync(sourceUri);

        // Assert
        Assert.Equal(1, requestCount);

        Assert.Equal(sourceUri, result.SourceUri);

        Assert.Equal(
            "Replication troubleshooting",
            result.Title);

        Assert.Contains(
            "Validate replication health.",
            result.ArticleText);

        Assert.NotEqual(
            default,
            result.RetrievedUtc);
    }

    private sealed class FakeHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responder)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var response = responder(request);

            response.RequestMessage = request;

            return Task.FromResult(response);
            
        }
    }

    private sealed class AsyncFakeHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>
            responder)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return responder(request, cancellationToken);
        }
    }

    [Fact]
    public async Task IngestAsync_ThrowsWhenSourceUriIsNull()
    {
        // Arrange
        var requestCount = 0;

        using var client = new HttpClient(
            new FakeHandler(_ =>
            {
                requestCount++;

                throw new InvalidOperationException(
                    "HTTP must not be called.");
            }));

        var fetcher = new MicrosoftLearnDocumentFetcher(
            client,
            new MicrosoftLearnSourcePolicy());

        var service = new MicrosoftLearnDocumentIngestionService(
            fetcher,
            new MicrosoftLearnDocumentExtractor());

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.IngestAsync(null!));

        // Assert
        Assert.Equal("sourceUri", exception.ParamName);
        Assert.Equal(0, requestCount);
    }

    [Fact]
    public async Task IngestAsync_PropagatesCancellation()
    {
        // Arrange
        var sourceUri = new Uri(
            "https://learn.microsoft.com/en-us/windows-server/");

        var requestCount = 0;
        var cancellationObserved = false;

        using var client = new HttpClient(
            new FakeHandler(_ =>
            {
                requestCount++;

                throw new InvalidOperationException(
                    "HTTP response must not be created.");
            }));

        var fetcher = new MicrosoftLearnDocumentFetcher(
            client,
            new MicrosoftLearnSourcePolicy());

        var service = new MicrosoftLearnDocumentIngestionService(
            fetcher,
            new MicrosoftLearnDocumentExtractor());

        using var cancellation = new CancellationTokenSource();

        cancellation.Cancel();

        // Act
        try
        {
            await service.IngestAsync(
                sourceUri,
                cancellation.Token);
        }
        catch (OperationCanceledException)
        {
            cancellationObserved = true;
        }

        // Assert
        Assert.True(cancellationObserved);
        Assert.Equal(0, requestCount);
    }

    [Fact]
    public async Task IngestAsync_PropagatesHttpFailure()
    {
        // Arrange
        var sourceUri = new Uri(
            "https://learn.microsoft.com/en-us/windows-server/");

        var requestCount = 0;

        using var client = new HttpClient(
            new FakeHandler(_ =>
            {
                requestCount++;

                return new HttpResponseMessage(
                    HttpStatusCode.InternalServerError);
            }));

        var fetcher = new MicrosoftLearnDocumentFetcher(
            client,
            new MicrosoftLearnSourcePolicy());

        var service = new MicrosoftLearnDocumentIngestionService(
            fetcher,
            new MicrosoftLearnDocumentExtractor());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => service.IngestAsync(sourceUri));

        Assert.Equal(
            HttpStatusCode.InternalServerError,
            exception.StatusCode);

        Assert.Equal(1, requestCount);
    }

    [Fact]
    public async Task IngestAsync_CancelsInFlightHttpRequest()
    {
        // Arrange
        var sourceUri = new Uri(
            "https://learn.microsoft.com/en-us/windows-server/");

        var requestStarted =
            new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

        var requestCancelled =
            new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

        using var client = new HttpClient(
            new AsyncFakeHandler(async (_, cancellationToken) =>
            {
                requestStarted.TrySetResult(true);

                try
                {
                    await Task.Delay(
                        Timeout.InfiniteTimeSpan,
                        cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    requestCancelled.TrySetResult(true);
                    throw;
                }

                throw new InvalidOperationException(
                    "The request should have been cancelled.");
            }));

        var fetcher = new MicrosoftLearnDocumentFetcher(
            client,
            new MicrosoftLearnSourcePolicy());

        var service = new MicrosoftLearnDocumentIngestionService(
            fetcher,
            new MicrosoftLearnDocumentExtractor());

        using var cancellation = new CancellationTokenSource();

        // Act
        var ingestionTask = service.IngestAsync(
            sourceUri,
            cancellation.Token);

        await requestStarted.Task.WaitAsync(
            TimeSpan.FromSeconds(5));

        cancellation.Cancel();

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => ingestionTask.WaitAsync(TimeSpan.FromSeconds(5)));

        Assert.True(
            await requestCancelled.Task.WaitAsync(
                TimeSpan.FromSeconds(5)));
    }
}