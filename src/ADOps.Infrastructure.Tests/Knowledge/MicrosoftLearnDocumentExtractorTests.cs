using ADOps.Infrastructure.Knowledge;

namespace ADOps.Infrastructure.Tests.Knowledge;

public sealed class MicrosoftLearnDocumentExtractorTests
{
    private readonly MicrosoftLearnDocumentExtractor _extractor = new();

    [Fact]
    public void Extract_PreservesNestedTroubleshootingProcedure()
    {
        var document = CreateDocument("""
    <main>
      <article>
        <h1>Replication troubleshooting</h1>
        <ol>
          <li>
            <p>Validate connectivity.</p>
            <ul>
              <li>Check DNS resolution.</li>
              <li>Check RPC connectivity.</li>
            </ul>
            <pre><code>repadmin /replsummary
    repadmin /showrepl</code></pre>
          </li>
          <li>
            <p>Review replication status.</p>
          </li>
        </ol>
      </article>
    </main>
    """);

        var result = _extractor.Extract(document);

        var text = result.ArticleText.Replace("\r\n", "\n");

        Assert.Contains(
            "1. Validate connectivity.\n" +
            "  - Check DNS resolution.\n" +
            "  - Check RPC connectivity.\n" +
            "  repadmin /replsummary\n" +
            "  repadmin /showrepl\n" +
            "2. Review replication status.",
            text);
        }
    
    [Fact]
    public void Extract_PreservesTroubleshootingTableRelationships()
    {
        var document = CreateDocument("""
            <main>
                <article>
                    <h1>Replication errors</h1>
                    <table>
                        <thead>
                            <tr>
                                <th>Error</th>
                                <th>Meaning</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr>
                                <td>1722</td>
                                <td>RPC server unavailable</td>
                            </tr>
                            <tr>
                                <td>8453</td>
                                <td>Replication access denied</td>
                            </tr>
                    </tbody>
                </table>
            </article>
        </main>
        """);

        var result = _extractor.Extract(document);

        Assert.Contains(
            "Error | Meaning",
            result.ArticleText);

        Assert.Contains(
            "1722 | RPC server unavailable",
            result.ArticleText);

        Assert.Contains(
            "8453 | Replication access denied",
            result.ArticleText);
    }
    
    [Fact]
    public void Extract_PreservesPowerShellCodeBlockFormatting()
    {
        var document = CreateDocument("""
            <main>
                <article>
                    <h1>Replication troubleshooting</h1>
                    <p>Run these commands:</p>
                    <pre><code>repadmin /replsummary
            repadmin /showrepl
                dcdiag /test:replications</code></pre>
                </article>
            </main>
            """);

        var result = _extractor.Extract(document);

        Assert.Contains(
            "repadmin /replsummary\nrepadmin /showrepl\n" +
            "    dcdiag /test:replications",
            result.ArticleText.Replace("\r\n", "\n"));
    }

    [Fact]
    public void Extract_PreservesParagraphAndHeadingBoundaries()
    {
        var document = CreateDocument("""
            <main>
            <article>
                <h1>Replication troubleshooting</h1>
                <p>First paragraph.</p>
                <h2>Validation</h2>
                <p>Second paragraph.</p>
            </article>
            </main>
        """);

    var result = _extractor.Extract(document);

    Assert.Contains(
        "First paragraph.\nValidation\nSecond paragraph.",
        result.ArticleText.Replace("\r\n", "\n"));
    }

    [Fact]
    public void Extract_PreservesListItemBoundaries()
    {
        var document = CreateDocument("""
            <main>
                <article>
                    <h1>Validation steps</h1>
                    <ol>
                    <li>Check DNS resolution.</li>
                    <li>Check RPC connectivity.</li>
                    <li>Review replication status.</li>
                    </ol>
                </article>
            </main>
            """);

        var result = _extractor.Extract(document);

        Assert.Contains(
            "1. Check DNS resolution.\n" +
            "2. Check RPC connectivity.\n" +
            "3. Review replication status.",
            result.ArticleText.Replace("\r\n", "\n"));
    }
    
    [Fact]
    public void Extract_ReturnsArticleTitleTextAndHeadings()
    {
        var document = CreateDocument("""
            <html>
              <head>
                <title>Browser title</title>
              </head>
              <body>
                <nav>Navigation menu</nav>
                <main>
                  <article>
                    <h1>Active Directory replication</h1>
                    <p>Review replication health.</p>
                    <h2>Troubleshooting</h2>
                    <p>Inspect replication errors.</p>
                  </article>
                </main>
                <footer>Website footer</footer>
              </body>
            </html>
            """);

        var result = _extractor.Extract(document);

        Assert.Equal(
            "Active Directory replication",
            result.Title);

        Assert.Equal(
            [
                "Active Directory replication",
                "Troubleshooting"
            ],
            result.Headings);

        Assert.Contains(
            "Review replication health.",
            result.ArticleText);

        Assert.Contains(
            "Inspect replication errors.",
            result.ArticleText);

        Assert.DoesNotContain(
            "Navigation menu",
            result.ArticleText);

        Assert.DoesNotContain(
            "Website footer",
            result.ArticleText);

        Assert.Equal(document.SourceUri, result.SourceUri);
        Assert.Equal(document.RetrievedUtc, result.RetrievedUtc);
    }

    [Fact]
    public void Extract_RemovesNonArticleElements()
    {
        var document = CreateDocument("""
            <html>
              <body>
                <main>
                  <article>
                    <h1>Replication guidance</h1>
                    <p>Useful guidance.</p>
                    <script>Unwanted script</script>
                    <style>Unwanted styles</style>
                    <aside>Unwanted sidebar</aside>
                    <button>Unwanted button</button>
                    <div aria-hidden="true">Hidden content</div>
                  </article>
                </main>
              </body>
            </html>
            """);

        var result = _extractor.Extract(document);

        Assert.Contains(
            "Useful guidance.",
            result.ArticleText);

        Assert.DoesNotContain(
            "Unwanted",
            result.ArticleText);

        Assert.DoesNotContain(
            "Hidden content",
            result.ArticleText);
    }

    [Fact]
    public void Extract_DecodesHtmlEntities()
    {
        var document = CreateDocument("""
            <main>
              <article>
                <h1>DNS &amp; replication</h1>
                <p>Check DNS &amp; RPC connectivity.</p>
              </article>
            </main>
            """);

        var result = _extractor.Extract(document);

        Assert.Equal(
            "DNS & replication",
            result.Title);

        Assert.Contains(
            "Check DNS & RPC connectivity.",
            result.ArticleText);
    }

    [Fact]
    public void Extract_ThrowsWhenArticleIsMissing()
    {
        var document = CreateDocument("""
            <html>
              <body>
                <nav>Navigation only</nav>
              </body>
            </html>
            """);

        Assert.Throws<InvalidOperationException>(
            () => _extractor.Extract(document));
    }

    [Fact]
    public void Extract_ThrowsWhenArticleIsEmpty()
    {
        var document = CreateDocument("""
            <main>
              <article>
                <script>Script only</script>
              </article>
            </main>
            """);

        Assert.Throws<InvalidOperationException>(
            () => _extractor.Extract(document));
    }

    private static MicrosoftLearnDocument CreateDocument(
        string html)
    {
        return new MicrosoftLearnDocument
        {
            SourceUri = new Uri(
                "https://learn.microsoft.com/en-us/windows-server/"),

            Content = html,

            RetrievedUtc = new DateTimeOffset(
                2026, 9, 21, 12, 0, 0, TimeSpan.Zero)
        };
    }
}