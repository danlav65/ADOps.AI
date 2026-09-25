using ADOps.Core.Entities;
using ADOps.Core.Enums;

namespace ADOps.Infrastructure.Knowledge;

/// <summary>
/// Maps an extracted Microsoft Learn article to a knowledge match
/// while preserving source provenance.
/// </summary>
public sealed class MicrosoftLearnKnowledgeMapper
{
    public KnowledgeMatch Map(MicrosoftLearnExtractedDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return new KnowledgeMatch
        {
            Source = document.Title,
            Description = document.ArticleText,

            Provenance = new KnowledgeSource
            {
                SourceId = document.SourceUri.AbsoluteUri,
                Title = document.Title,
                Publisher = "Microsoft",
                SourceType = KnowledgeSourceType.MicrosoftDocumentation,
                SourceUri = document.SourceUri,
                RetrievedUtc = document.RetrievedUtc
            }
        };
    }
}