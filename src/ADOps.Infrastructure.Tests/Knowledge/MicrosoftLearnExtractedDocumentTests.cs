using ADOps.Infrastructure.Knowledge;

namespace ADOps.Infrastructure.Tests.Knowledge;

public sealed class MicrosoftLearnExtractedDocumentTests
{
    [Fact]
    public void ExtractedDocument_PreservesSourceAndContent()
    {
        // Arrange
        var sourceUri = new Uri(
            "https://learn.microsoft.com/en-us/windows-server/");

        var retrievedUtc = new DateTimeOffset(
            2026, 9, 20, 12, 0, 0, TimeSpan.Zero);

        // Act
        var document = new MicrosoftLearnExtractedDocument
        {
            SourceUri = sourceUri,
            Title = "Windows Server documentation",
            ArticleText = "Example article content.",
            Headings = ["Overview", "Configuration"],
            RetrievedUtc = retrievedUtc
        };

        // Assert
        Assert.Equal(sourceUri, document.SourceUri);
        Assert.Equal(
            "Windows Server documentation",
            document.Title);

        Assert.Equal(
            "Example article content.",
            document.ArticleText);

        Assert.Equal(
            ["Overview", "Configuration"],
            document.Headings);

        Assert.Equal(retrievedUtc, document.RetrievedUtc);
    }
}