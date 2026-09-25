using ADOps.Infrastructure.Knowledge;

namespace ADOps.Infrastructure.Tests.Knowledge;

public sealed class MicrosoftLearnDocumentChunkerTests
{
    [Fact]
    public void Chunk_ShortArticle_ProducesSingleChunk()
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

        var chunker = new MicrosoftLearnDocumentChunker();

        // Act
        var chunks = chunker.Chunk(document);

        // Assert
        var chunk = Assert.Single(chunks);

        Assert.Equal(
            document.ArticleText,
            chunk.Content);

        Assert.Equal(0, chunk.Sequence);

        Assert.Equal(
            sourceUri.AbsoluteUri,
            chunk.SourceId);

        Assert.Equal(
            $"{sourceUri.AbsoluteUri}#chunk-0",
            chunk.ChunkId);

        Assert.Equal(
            sourceUri,
            chunk.Provenance.SourceUri);

        Assert.Equal(
            document.Title,
            chunk.Provenance.Title);

        Assert.Equal(
            retrievedUtc,
            chunk.Provenance.RetrievedUtc);
    }

    [Fact]
    public void Chunk_LongArticle_ProducesOrderedChunksWithoutLosingContent()
    {
        // Arrange
        var sourceUri = new Uri(
            "https://learn.microsoft.com/en-us/windows-server/");

        var articleText = new string('A', 2500);

        var document = new MicrosoftLearnExtractedDocument
        {
            SourceUri = sourceUri,
            Title = "Windows Server documentation",
            ArticleText = articleText,
            Headings = ["Troubleshooting"],
            RetrievedUtc = new DateTimeOffset(
                2026, 9, 21, 12, 0, 0, TimeSpan.Zero)
        };

        var chunker = new MicrosoftLearnDocumentChunker();

        // Act
        var chunks = chunker.Chunk(document).ToArray();

        // Assert
        Assert.Equal(3, chunks.Length);

        Assert.Equal(
            new[] { 0, 1, 2 },
            chunks.Select(chunk => chunk.Sequence));

        Assert.All(
            chunks,
            chunk => Assert.InRange(
                chunk.Content.Length,
                1,
                1000));

        Assert.Equal(
            articleText,
            string.Concat(chunks.Select(chunk => chunk.Content)));

        Assert.Equal(
            new[] { 1000, 1000, 500 },
            chunks.Select(chunk => chunk.Content.Length));

        for (var index = 0; index < chunks.Length; index++)
        {
            Assert.Equal(
                $"{sourceUri.AbsoluteUri}#chunk-{index}",
                chunks[index].ChunkId);

            Assert.Equal(
                sourceUri.AbsoluteUri,
                chunks[index].SourceId);

            Assert.Equal(
                sourceUri,
                chunks[index].Provenance.SourceUri);
        }
    }

    [Fact]
    public void Chunk_LongArticle_PrefersWhitespaceBoundary()
    {
        // Arrange
        var sourceUri = new Uri(
            "https://learn.microsoft.com/en-us/windows-server/");

        var articleText =
            new string('A', 995) +
            " " +
            "replication troubleshooting";

        var document = new MicrosoftLearnExtractedDocument
        {
            SourceUri = sourceUri,
            Title = "Windows Server documentation",
            ArticleText = articleText,
            Headings = ["Troubleshooting"],
            RetrievedUtc = new DateTimeOffset(
                2026, 9, 21, 12, 0, 0, TimeSpan.Zero)
        };

        var chunker = new MicrosoftLearnDocumentChunker();

        // Act
        var chunks = chunker.Chunk(document).ToArray();

        // Assert
        Assert.Equal(2, chunks.Length);

        Assert.Equal(
            new string('A', 995) + " ",
            chunks[0].Content);

        Assert.Equal(
            "replication troubleshooting",
            chunks[1].Content);

        Assert.Equal(
            articleText,
            string.Concat(chunks.Select(chunk => chunk.Content)));

        Assert.All(
            chunks,
            chunk => Assert.InRange(
                chunk.Content.Length,
                1,
                1000));
    }

    [Fact]
    public void Chunk_EmptyArticle_ReturnsNoChunks()
    {
        // Arrange
        var document = new MicrosoftLearnExtractedDocument
        {
            SourceUri = new Uri(
                "https://learn.microsoft.com/en-us/windows-server/"),

            Title = "Windows Server documentation",
            ArticleText = string.Empty,
            Headings = [],

            RetrievedUtc = new DateTimeOffset(
                2026, 9, 21, 12, 0, 0, TimeSpan.Zero)
        };

        var chunker = new MicrosoftLearnDocumentChunker();

        // Act
        var chunks = chunker.Chunk(document);

        // Assert
        Assert.Empty(chunks);
    }

    [Fact]
    public void Chunk_LongArticle_PrefersParagraphBoundary()
    {
        // Arrange
        var sourceUri = new Uri(
            "https://learn.microsoft.com/en-us/windows-server/");

        var firstParagraph = new string('A', 800);

        var secondParagraph =
            string.Join(" ", Enumerable.Repeat("replication", 30));

        var articleText =
            firstParagraph + "\n\n" + secondParagraph;

        var document = new MicrosoftLearnExtractedDocument
        {
            SourceUri = sourceUri,
            Title = "Active Directory replication troubleshooting",
            ArticleText = articleText,
            Headings = ["Troubleshooting"],
            RetrievedUtc = new DateTimeOffset(
                2026, 9, 21, 12, 0, 0, TimeSpan.Zero)
        };

        var chunker = new MicrosoftLearnDocumentChunker();

        // Act
        var chunks = chunker.Chunk(document).ToArray();

        // Assert
        Assert.Equal(2, chunks.Length);

        Assert.Equal(
            firstParagraph + "\n\n",
            chunks[0].Content);

        Assert.Equal(
            secondParagraph,
            chunks[1].Content);

        Assert.Equal(
            articleText,
            string.Concat(chunks.Select(chunk => chunk.Content)));

        Assert.All(
            chunks,
            chunk => Assert.InRange(
                chunk.Content.Length,
                1,
                1000));

        Assert.Equal(
            new[] { 0, 1 },
            chunks.Select(chunk => chunk.Sequence));
    }

    [Fact]
    public void Chunk_LongArticle_PrefersWindowsParagraphBoundary()
    {
        // Arrange
        var sourceUri = new Uri(
            "https://learn.microsoft.com/en-us/windows-server/");

        var firstParagraph = new string('A', 800);

        var secondParagraph =
            string.Join(" ", Enumerable.Repeat("replication", 30));

        var articleText =
            firstParagraph + "\r\n\r\n" + secondParagraph;

        var document = new MicrosoftLearnExtractedDocument
        {
            SourceUri = sourceUri,
            Title = "Active Directory replication troubleshooting",
            ArticleText = articleText,
            Headings = ["Troubleshooting"],
            RetrievedUtc = new DateTimeOffset(
                2026, 9, 21, 12, 0, 0, TimeSpan.Zero)
        };

        var chunker = new MicrosoftLearnDocumentChunker();

        // Act
        var chunks = chunker.Chunk(document).ToArray();

        // Assert
        Assert.Equal(2, chunks.Length);

        Assert.Equal(
            firstParagraph + "\r\n\r\n",
            chunks[0].Content);

        Assert.Equal(
            secondParagraph,
            chunks[1].Content);

        Assert.Equal(
            articleText,
            string.Concat(chunks.Select(chunk => chunk.Content)));

        Assert.All(
            chunks,
            chunk => Assert.InRange(chunk.Content.Length, 1, 1000));
    }

    [Fact]
    public void Chunk_StructuredArticle_KeepsCodeBlockTogether()
    {
        // Arrange
        var sourceUri = new Uri(
            "https://learn.microsoft.com/en-us/windows-server/");

        var introduction = new string('A', 900);

        var code =
            "repadmin /replsummary\n" +
            "repadmin /showrepl\n" +
            "dcdiag /test:replications";

        var articleText = introduction + "\n" + code;

        var document = new MicrosoftLearnExtractedDocument
        {
            SourceUri = sourceUri,
            Title = "Replication troubleshooting",
            ArticleText = articleText,
            Headings = ["Replication troubleshooting"],

            RetrievedUtc = new DateTimeOffset(
                2026, 9, 21, 12, 0, 0, TimeSpan.Zero),

            Blocks =
            [
                new MicrosoftLearnContentBlock
                {
                    Type = MicrosoftLearnContentBlockType.Paragraph,
                    Content = introduction,
                    Sequence = 0
                },

                new MicrosoftLearnContentBlock
                {
                    Type = MicrosoftLearnContentBlockType.Code,
                    Content = code,
                    Sequence = 1
                }
            ]
        };

        // Act
        var chunks = new MicrosoftLearnDocumentChunker()
            .Chunk(document)
            .OrderBy(chunk => chunk.Sequence)
            .ToArray();

        // Assert
        Assert.Equal(2, chunks.Length);

        Assert.Equal(
            introduction + "\n",
            chunks[0].Content);

        Assert.Equal(
            code,
            chunks[1].Content);

        Assert.Equal(
            articleText,
            string.Concat(chunks.Select(chunk => chunk.Content)));

        Assert.All(
            chunks,
            chunk => Assert.True(chunk.Content.Length <= 1000));
    }

    [Fact]
    public void Chunk_ConsecutiveCodeBlocks_PreservesSeparatorAndContent()
    {
        // Arrange
        var sourceUri = new Uri(
            "https://learn.microsoft.com/en-us/windows-server/");

        var firstCode = "repadmin /replsummary";
        var secondCode = "repadmin /showrepl";

        var articleText = firstCode + "\n" + secondCode;

        var document = new MicrosoftLearnExtractedDocument
        {
            SourceUri = sourceUri,
            Title = "Replication troubleshooting",
            ArticleText = articleText,
            Headings = ["Replication troubleshooting"],

            RetrievedUtc = new DateTimeOffset(
                2026, 9, 21, 12, 0, 0, TimeSpan.Zero),

            Blocks =
            [
                new MicrosoftLearnContentBlock
                {
                    Type = MicrosoftLearnContentBlockType.Code,
                    Content = firstCode,
                    Sequence = 0
                },

                new MicrosoftLearnContentBlock
                {
                    Type = MicrosoftLearnContentBlockType.Code,
                    Content = secondCode,
                    Sequence = 1
                }
            ]
        };

        // Act
        var chunks = new MicrosoftLearnDocumentChunker()
            .Chunk(document)
            .OrderBy(chunk => chunk.Sequence)
            .ToArray();

        // Assert
        Assert.Equal(2, chunks.Length);

        Assert.Equal(
            firstCode,
            chunks[0].Content);

        Assert.Equal(
            "\n" + secondCode,
            chunks[1].Content);

        Assert.Equal(
            articleText,
            string.Concat(chunks.Select(chunk => chunk.Content)));

        Assert.All(
            chunks,
            chunk => Assert.True(chunk.Content.Length <= 1000));
    }

    [Fact]
    public void Chunk_OversizedCodeBlock_PrefersLineBoundary()
    {
        // Arrange
        var sourceUri = new Uri(
            "https://learn.microsoft.com/en-us/windows-server/");

        var firstLine = new string('A', 800);
        var secondLine = new string('B', 400);

        var code = firstLine + "\n" + secondLine;

        var document = new MicrosoftLearnExtractedDocument
        {
            SourceUri = sourceUri,
            Title = "Replication troubleshooting",
            ArticleText = code,
            Headings = ["Replication troubleshooting"],

            RetrievedUtc = new DateTimeOffset(
                2026, 9, 21, 12, 0, 0, TimeSpan.Zero),

            Blocks =
            [
                new MicrosoftLearnContentBlock
                {
                    Type = MicrosoftLearnContentBlockType.Code,
                    Content = code,
                    Sequence = 0
                }
            ]
        };

        // Act
        var chunks = new MicrosoftLearnDocumentChunker()
            .Chunk(document)
            .OrderBy(chunk => chunk.Sequence)
            .ToArray();

        // Assert
        Assert.Equal(2, chunks.Length);

        Assert.Equal(
            firstLine + "\n",
            chunks[0].Content);

        Assert.Equal(
            secondLine,
            chunks[1].Content);

        Assert.Equal(
            code,
            string.Concat(chunks.Select(chunk => chunk.Content)));

        Assert.All(
            chunks,
            chunk => Assert.True(chunk.Content.Length <= 1000));
    }

    [Fact]
    public void Chunk_OversizedSingleCodeLine_PreservesAllCharacters()
    {
        // Arrange
        var sourceUri = new Uri(
            "https://learn.microsoft.com/en-us/windows-server/");

        var code = new string('A', 1500);

        var document = new MicrosoftLearnExtractedDocument
        {
            SourceUri = sourceUri,
            Title = "Replication troubleshooting",
            ArticleText = code,
            Headings = ["Replication troubleshooting"],

            RetrievedUtc = new DateTimeOffset(
                2026, 9, 21, 12, 0, 0, TimeSpan.Zero),

            Blocks =
            [
                new MicrosoftLearnContentBlock
                {
                    Type = MicrosoftLearnContentBlockType.Code,
                    Content = code,
                    Sequence = 0
                }
            ]
        };

        // Act
        var chunks = new MicrosoftLearnDocumentChunker()
            .Chunk(document)
            .OrderBy(chunk => chunk.Sequence)
            .ToArray();

        // Assert
        Assert.Equal(2, chunks.Length);

        Assert.Equal(1000, chunks[0].Content.Length);
        Assert.Equal(500, chunks[1].Content.Length);

        Assert.Equal(
            code,
            string.Concat(chunks.Select(chunk => chunk.Content)));

        Assert.All(
            chunks,
            chunk => Assert.True(chunk.Content.Length <= 1000));

        Assert.Equal(0, chunks[0].Sequence);
        Assert.Equal(1, chunks[1].Sequence);
    }
}