using System.Text;
using ADOps.Core.Entities;
using ADOps.Core.Enums;

namespace ADOps.Infrastructure.Knowledge;

/// <summary>
/// Converts extracted Microsoft Learn documents into
/// searchable knowledge chunks.
/// </summary>
public sealed class MicrosoftLearnDocumentChunker
{
    private const int MaxChunkLength = 1000;

    public IReadOnlyCollection<KnowledgeChunk> Chunk(
        MicrosoftLearnExtractedDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var sourceId = document.SourceUri.AbsoluteUri;

        var provenance = new KnowledgeSource
        {
            SourceId = sourceId,
            Title = document.Title,
            Publisher = "Microsoft",
            SourceType = KnowledgeSourceType.MicrosoftDocumentation,
            SourceUri = document.SourceUri,
            RetrievedUtc = document.RetrievedUtc
        };

        var chunks = new List<KnowledgeChunk>();

        // Structured blocks are usable only when they represent
        // the complete article without changing its content.
        var blocks = document.Blocks
            .OrderBy(block => block.Sequence)
            .ToArray();

        var structuredText = string.Join(
            "\n",
            blocks.Select(block => block.Content));

        if (blocks.Length == 0 ||
            !string.Equals(
                structuredText,
                document.ArticleText,
                StringComparison.Ordinal))
        {
            AppendTextChunks(
                document.ArticleText,
                sourceId,
                provenance,
                chunks);

            return chunks;
        }

        var pendingText = new StringBuilder();
    for (var index = 0; index < blocks.Length; index++)
    {
        var block = blocks[index];

        // Preserve the separator inserted by the extractor.
        if (index > 0)
        {
            pendingText.Append('\n');
        }

        if (block.Type == MicrosoftLearnContentBlockType.Code)
        {
            var codeContent = block.Content;

            if (pendingText.Length == 1 &&
                pendingText[0] == '\n')
            {
                // Consecutive code blocks: attach the separator
                // to the next code chunk instead of creating
                // a newline-only chunk.
                codeContent = "\n" + codeContent;
            }
            else
            {
                // Flush preceding ordinary text, including
                // its separator, before starting the code.
                AppendTextChunks(
                    pendingText.ToString(),
                    sourceId,
                    provenance,
                    chunks);
            }

            pendingText.Clear();

            AppendTextChunks(
                codeContent,
                sourceId,
                provenance,
                chunks);
        }
        else
        {
            pendingText.Append(block.Content);
        }
    }

        AppendTextChunks(
            pendingText.ToString(),
            sourceId,
            provenance,
            chunks);

        return chunks;
    }

    private static void AppendTextChunks(
        string articleText,
        string sourceId,
        KnowledgeSource provenance,
        List<KnowledgeChunk> chunks)
    {
        var offset = 0;

        while (offset < articleText.Length)
        {
            var length = Math.Min(
                MaxChunkLength,
                articleText.Length - offset);

            // Prefer paragraph boundaries when more content remains.
            // Fall back to whitespace, then the hard character limit.
            if (offset + length < articleText.Length)
            {
                var paragraphBoundary = -1;
                var paragraphDelimiterLength = 0;

                for (var index = offset + length - 2;
                     index >= offset;
                     index--)
                {
                    // Windows paragraph delimiter: \r\n\r\n
                    if (index + 3 < offset + length &&
                        articleText[index] == '\r' &&
                        articleText[index + 1] == '\n' &&
                        articleText[index + 2] == '\r' &&
                        articleText[index + 3] == '\n')
                    {
                        paragraphBoundary = index;
                        paragraphDelimiterLength = 4;
                        break;
                    }

                    // Unix paragraph delimiter: \n\n
                    if (articleText[index] == '\n' &&
                        articleText[index + 1] == '\n')
                    {
                        paragraphBoundary = index;
                        paragraphDelimiterLength = 2;
                        break;
                    }
                }

                if (paragraphBoundary >= offset)
                {
                    // Include the complete paragraph delimiter.
                    length =
                        paragraphBoundary -
                        offset +
                        paragraphDelimiterLength;
                }
                else
                {
                    var lastWhitespace = -1;

                    for (var index = offset + length - 1;
                         index >= offset;
                         index--)
                    {
                        if (char.IsWhiteSpace(articleText[index]))
                        {
                            lastWhitespace = index;
                            break;
                        }
                    }

                    if (lastWhitespace >= offset)
                    {
                        // Preserve whitespace to avoid losing content.
                        length = lastWhitespace - offset + 1;
                    }
                }
            }

            var content = articleText.Substring(
                offset,
                length);

            var sequence = chunks.Count;

            chunks.Add(new KnowledgeChunk
            {
                ChunkId = $"{sourceId}#chunk-{sequence}",
                SourceId = sourceId,
                Content = content,
                Sequence = sequence,
                Provenance = provenance
            });

            offset += length;
        }
    }
}