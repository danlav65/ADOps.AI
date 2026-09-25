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

    [Fact]
    public void ExtractedDocument_CanPreserveStructuredContentBlocks()
    {
        // Arrange
        var document = new MicrosoftLearnExtractedDocument
        {
            SourceUri = new Uri(
                "https://learn.microsoft.com/en-us/windows-server/"),

            Title = "Replication troubleshooting",

            ArticleText =
                "Run these commands:\n" +
                "repadmin /replsummary\n" +
                "repadmin /showrepl",

            Headings = ["Replication troubleshooting"],

            RetrievedUtc = new DateTimeOffset(
                2026, 9, 21, 12, 0, 0, TimeSpan.Zero),

            Blocks =
            [
                new MicrosoftLearnContentBlock
                {
                    Type = MicrosoftLearnContentBlockType.Paragraph,
                    Content = "Run these commands:",
                    Sequence = 0
                },

                new MicrosoftLearnContentBlock
                {
                    Type = MicrosoftLearnContentBlockType.Code,
                    Content =
                        "repadmin /replsummary\n" +
                        "repadmin /showrepl",
                    Sequence = 1
                }
            ]
        };

        // Assert
        Assert.Equal(2, document.Blocks.Count);

        var blocks = document.Blocks.ToArray();

        Assert.Equal(
            MicrosoftLearnContentBlockType.Paragraph,
            blocks[0].Type);

        Assert.Equal(
            MicrosoftLearnContentBlockType.Code,
            blocks[1].Type);

        Assert.Equal(
            "repadmin /replsummary\nrepadmin /showrepl",
            blocks[1].Content);

        Assert.Equal(0, blocks[0].Sequence);
        Assert.Equal(1, blocks[1].Sequence);
    }
}