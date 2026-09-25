namespace ADOps.Infrastructure.Knowledge;

/// <summary>
/// Coordinates fetching and extracting a Microsoft Learn document.
/// Does not evaluate the correctness or relevance of the guidance.
/// </summary>
public sealed class MicrosoftLearnDocumentIngestionService
{
    private readonly MicrosoftLearnDocumentFetcher _fetcher;
    private readonly IMicrosoftLearnDocumentExtractor _extractor;

    public MicrosoftLearnDocumentIngestionService(
        MicrosoftLearnDocumentFetcher fetcher,
        IMicrosoftLearnDocumentExtractor extractor)
    {
        _fetcher = fetcher
            ?? throw new ArgumentNullException(nameof(fetcher));

        _extractor = extractor
            ?? throw new ArgumentNullException(nameof(extractor));
    }

    public async Task<MicrosoftLearnExtractedDocument> IngestAsync(
        Uri sourceUri,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sourceUri);

        var document = await _fetcher.FetchAsync(
            sourceUri,
            cancellationToken);

        return _extractor.Extract(document);
    }
}