using System.Net;
using System.Net.Http.Headers;
using ADOps.Infrastructure.Knowledge;

namespace ADOps.Infrastructure.Tests.Knowledge;

public sealed class MicrosoftLearnPipelineTests
{
    [Fact]
    public async Task FetchAndExtract_PreservesArticleAndProvenance()
    {
        // Arrange
        var sourceUri = new Uri(
            "https://learn.microsoft.com/en-us/windows-server/");

        const string html = """
            <html>
              <body>
                <nav>Site navigation</nav>
                <main>
                  <article>
                    <h1>Replication troubleshooting</h1>
                    <p>Check replication health.</p>
                    <pre><code>repadmin /replsummary
            repadmin /showrepl</code></pre>
                  </article>
                </main>
              </body>
            </html>
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

        var extractor = new MicrosoftLearnDocumentExtractor();

        var beforeFetch = DateTimeOffset.UtcNow;

        // Act
        var fetched = await fetcher.FetchAsync(sourceUri);

        var extracted = extractor.Extract(fetched);

        var afterFetch = DateTimeOffset.UtcNow;

        // Assert
        Assert.Equal(1, requestCount);

        Assert.Equal(sourceUri, fetched.SourceUri);
        Assert.Equal(sourceUri, extracted.SourceUri);

        Assert.InRange(
            fetched.RetrievedUtc,
            beforeFetch,
            afterFetch);

        Assert.Equal(
            fetched.RetrievedUtc,
            extracted.RetrievedUtc);

        Assert.Equal(
            "Replication troubleshooting",
            extracted.Title);

        Assert.Contains(
            "Replication troubleshooting",
            extracted.Headings);

        Assert.Contains(
            "Check replication health.",
            extracted.ArticleText);

        Assert.Contains(
            "repadmin /replsummary\nrepadmin /showrepl",
            extracted.ArticleText.Replace("\r\n", "\n"));

        Assert.DoesNotContain(
            "Site navigation",
            extracted.ArticleText);
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
}