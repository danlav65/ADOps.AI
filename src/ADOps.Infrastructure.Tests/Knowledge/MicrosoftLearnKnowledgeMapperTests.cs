using ADOps.Core.Enums;
using ADOps.Infrastructure.Knowledge;

namespace ADOps.Infrastructure.Tests.Knowledge;

public sealed class MicrosoftLearnKnowledgeMapperTests
{
    [Fact]
    public void Map_PreservesArticleContentAndProvenance()
    {
        // Arrange
        var sourceUri = new Uri(
            "https://learn.microsoft.com/en-us/windows-server/");

        var retrievedUtc = new DateTimeOffset(
            2026, 9, 21, 12, 0, 0, TimeSpan.Zero);

        var document = new MicrosoftLearnExtractedDocument
        {
            SourceUri = sourceUri,
            Title = "Windows Server documentation",
            ArticleText = "Troubleshooting Active Directory replication.",
            Headings = ["Troubleshooting"],
            RetrievedUtc = retrievedUtc
        };

        var mapper = new MicrosoftLearnKnowledgeMapper();

        // Act
        var match = mapper.Map(document);

        // Assert
        Assert.Equal(document.Title, match.Source);
        Assert.Equal(document.ArticleText, match.Description);

        Assert.NotNull(match.Provenance);

        Assert.Equal(
            sourceUri.AbsoluteUri,
            match.Provenance.SourceId);

        Assert.Equal(
            document.Title,
            match.Provenance.Title);

        Assert.Equal(
            "Microsoft",
            match.Provenance.Publisher);

        Assert.Equal(
            KnowledgeSourceType.MicrosoftDocumentation,
            match.Provenance.SourceType);

        Assert.Equal(
            sourceUri,
            match.Provenance.SourceUri);

        Assert.Equal(
            retrievedUtc,
            match.Provenance.RetrievedUtc);

        Assert.Null(match.Provenance.PublishedUtc);
        Assert.Null(match.Provenance.ModifiedUtc);

        Assert.Equal(0, match.SimilarityScore);
        Assert.Null(match.RelatedInvestigationId);
    }
}