namespace ADOps.Infrastructure.Knowledge;

/// <summary>
/// Represents content extracted from a Microsoft Learn HTML document.
/// Extraction does not establish that the guidance is correct or relevant.
/// </summary>
public sealed class MicrosoftLearnExtractedDocument
{
    public required Uri SourceUri { get; init; }

    public required string Title { get; init; }

    public required string ArticleText { get; init; }

    public required IReadOnlyCollection<string> Headings { get; init; }

    public required DateTimeOffset RetrievedUtc { get; init; }
}