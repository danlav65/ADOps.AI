namespace ADOps.Infrastructure.Knowledge;

/// <summary>
/// Extracts article content from fetched Microsoft Learn HTML.
/// </summary>
public interface IMicrosoftLearnDocumentExtractor
{
    MicrosoftLearnExtractedDocument Extract(
        MicrosoftLearnDocument document);
}