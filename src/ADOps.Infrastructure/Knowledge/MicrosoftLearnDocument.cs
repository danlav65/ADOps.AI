namespace ADOps.Infrastructure.Knowledge;

/// <summary>
/// Represents a successfully fetched Microsoft Learn document.
/// Content is raw HTML, not verified or extracted guidance.
/// </summary>
public sealed class MicrosoftLearnDocument
{
    public required Uri SourceUri { get; init; }

    public required string Content { get; init; }

    public required DateTimeOffset RetrievedUtc { get; init; }
}