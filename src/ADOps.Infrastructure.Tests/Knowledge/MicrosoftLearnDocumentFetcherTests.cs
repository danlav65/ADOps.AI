using System.Net;
using System.Net.Http.Headers;
using ADOps.Infrastructure.Knowledge;

namespace ADOps.Infrastructure.Tests.Knowledge;

public sealed class MicrosoftLearnDocumentFetcherTests
{
    [Fact]
    public async Task FetchAsync_ReturnsHtmlDocument()
    {
        using var client = CreateClient(_ =>
            HtmlResponse("<html><body>Test guidance</body></html>"));

        var fetcher = new MicrosoftLearnDocumentFetcher(
            client,
            new MicrosoftLearnSourcePolicy());

        var uri = new Uri(
            "https://learn.microsoft.com/en-us/windows-server/");

        var result = await fetcher.FetchAsync(uri);

        Assert.Equal(uri, result.SourceUri);
        Assert.Contains("Test guidance", result.Content);
        Assert.NotEqual(default, result.RetrievedUtc);
    }

    [Fact]
    public async Task FetchAsync_RejectsUnapprovedInitialUrl()
    {
        using var client = CreateClient(_ =>
            throw new InvalidOperationException(
                "HTTP must not be called."));

        var fetcher = new MicrosoftLearnDocumentFetcher(
            client,
            new MicrosoftLearnSourcePolicy());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => fetcher.FetchAsync(
                new Uri("https://example.com/")));
    }

    [Fact]
    public async Task FetchAsync_RejectsRedirectToUnapprovedHost()
    {
        using var client = CreateClient(_ =>
        {
            var response = new HttpResponseMessage(
                HttpStatusCode.Redirect);

            response.Headers.Location =
                new Uri("https://example.com/document");

            return response;
        });

        var fetcher = new MicrosoftLearnDocumentFetcher(
            client,
            new MicrosoftLearnSourcePolicy());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => fetcher.FetchAsync(
                new Uri("https://learn.microsoft.com/en-us/")));
    }

    [Fact]
    public async Task FetchAsync_FollowsApprovedRedirect()
    {
        var requests = 0;

        using var client = CreateClient(_ =>
        {
            requests++;

            if (requests == 1)
            {
                var response = new HttpResponseMessage(
                    HttpStatusCode.Redirect);

                response.Headers.Location =
                    new Uri("/en-us/windows-server/", UriKind.Relative);

                return response;
            }

            return HtmlResponse("<html>Redirected document</html>");
        });

        var fetcher = new MicrosoftLearnDocumentFetcher(
            client,
            new MicrosoftLearnSourcePolicy());

        var result = await fetcher.FetchAsync(
            new Uri("https://learn.microsoft.com/en-us/"));

        Assert.Equal(2, requests);

        Assert.Equal(
            "https://learn.microsoft.com/en-us/windows-server/",
            result.SourceUri.AbsoluteUri);
    }

    private static HttpClient CreateClient(
        Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        return new HttpClient(new FakeHandler(responder));
    }

    private static HttpResponseMessage HtmlResponse(string html)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(html)
        };

        response.Content.Headers.ContentType =
            new MediaTypeHeaderValue("text/html");

        return response;
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

    [Fact]
    public async Task FetchAsync_RejectsNonHtmlResponse()
    {
        using var client = CreateClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Not HTML")
            });

        var fetcher = new MicrosoftLearnDocumentFetcher(
            client,
            new MicrosoftLearnSourcePolicy());

        await Assert.ThrowsAsync<InvalidOperationException>(
        () => fetcher.FetchAsync(
            new Uri("https://learn.microsoft.com/en-us/")));
    }

    [Fact]
    public async Task FetchAsync_RejectsOversizedResponse()
    {
        using var client = CreateClient(_ =>
            HtmlResponse(new string('X', 2 * 1024 * 1024 + 1)));

        var fetcher = new MicrosoftLearnDocumentFetcher(
            client,
            new MicrosoftLearnSourcePolicy());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => fetcher.FetchAsync(
                new Uri("https://learn.microsoft.com/en-us/")));
    }

    [Fact]
    public async Task FetchAsync_RejectsRedirectLoop()
    {
        using var client = CreateClient(_ =>
        {
            var response = new HttpResponseMessage(
                HttpStatusCode.Redirect);

            response.Headers.Location =
                new Uri("/en-us/", UriKind.Relative);

            return response;
        });

        var fetcher = new MicrosoftLearnDocumentFetcher(
            client,
            new MicrosoftLearnSourcePolicy());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => fetcher.FetchAsync(
                new Uri("https://learn.microsoft.com/en-us/")));
    }

    [Fact]
    public async Task FetchAsync_ThrowsForHttpFailure()
    {
        using var client = CreateClient(_ =>
            new HttpResponseMessage(
                HttpStatusCode.InternalServerError));

        var fetcher = new MicrosoftLearnDocumentFetcher(
            client,
            new MicrosoftLearnSourcePolicy());

        await Assert.ThrowsAsync<HttpRequestException>(
            () => fetcher.FetchAsync(
                new Uri("https://learn.microsoft.com/en-us/")));
    }

    [Fact]
    public async Task FetchAsync_HonorsCancellation()
    {
        using var client = CreateClient(_ =>
            throw new InvalidOperationException(
                "HTTP should not be called."));

        var fetcher = new MicrosoftLearnDocumentFetcher(
            client,
            new MicrosoftLearnSourcePolicy());

        using var cancellation = new CancellationTokenSource();

        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => fetcher.FetchAsync(
                new Uri("https://learn.microsoft.com/en-us/"),
                cancellation.Token));
    }

    [Fact]
    public async Task FetchAsync_DoesNotRequestUnapprovedRedirectDestination()
    {
        // Arrange
        var requestedUris = new List<Uri>();

        using var client = CreateClient(request =>
        {
            requestedUris.Add(request.RequestUri!);

            if (request.RequestUri!.Host == "learn.microsoft.com")
            {
                return new HttpResponseMessage(
                    System.Net.HttpStatusCode.Redirect)
                {
                    Headers =
                    {
                        Location = new Uri(
                            "https://example.com/unapproved")
                    }
                };
            }

            throw new InvalidOperationException(
                "The unapproved destination was requested.");
        });

        var fetcher = new MicrosoftLearnDocumentFetcher(
            client,
            new MicrosoftLearnSourcePolicy());

        var sourceUri = new Uri(
            "https://learn.microsoft.com/en-us/windows-server/");

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => fetcher.FetchAsync(sourceUri));

        // Assert
        Assert.Single(requestedUris);
        Assert.Equal(sourceUri, requestedUris[0]);
    }
}