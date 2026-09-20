using ADOps.Core.Entities;
using ADOps.Core.Enums;

namespace ADOps.Core.Tests;

public sealed class KnowledgeSourceTests
{
    [Fact]
    public void KnowledgeSource_PreservesProvenanceMetadata()
    {
        var publishedUtc =
            new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero);

        var retrievedUtc =
            new DateTimeOffset(2026, 9, 19, 12, 0, 0, TimeSpan.Zero);

        var source = new KnowledgeSource
        {
            SourceId = "MS-AD-001",
            Title = "Active Directory Replication Guidance",
            Publisher = "Microsoft",
            SourceType = KnowledgeSourceType.MicrosoftDocumentation,
            SourceUri = new Uri(
                "https://learn.microsoft.com/"),
            PublishedUtc = publishedUtc,
            RetrievedUtc = retrievedUtc
        };

        Assert.Equal("MS-AD-001", source.SourceId);
        Assert.Equal("Microsoft", source.Publisher);
        Assert.Equal(
            KnowledgeSourceType.MicrosoftDocumentation,
            source.SourceType);
        Assert.Equal(publishedUtc, source.PublishedUtc);
        Assert.Equal(retrievedUtc, source.RetrievedUtc);
    }

    [Fact]
    public void KnowledgeMatch_PreservesSourceProvenance()
    {
        var source = new KnowledgeSource
        {
            SourceId = "MS-SUPPORT-001",
            Title = "AD Replication Support Guidance",
            Publisher = "Microsoft",
            SourceType = KnowledgeSourceType.MicrosoftSupportCase,
            RetrievedUtc = DateTimeOffset.UtcNow
        };

        var match = new KnowledgeMatch
        {
            Source = "Microsoft Support",
            Description = "Case-specific replication guidance.",
            Provenance = source
        };

        Assert.Same(source, match.Provenance);
        Assert.Equal(
            "MS-SUPPORT-001",
            match.Provenance!.SourceId);
    }
}