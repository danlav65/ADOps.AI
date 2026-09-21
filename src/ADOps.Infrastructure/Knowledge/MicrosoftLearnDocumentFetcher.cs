using System.Net;

namespace ADOps.Infrastructure.Knowledge;

/// <summary>
/// Fetches HTML from Microsoft Learn with explicit redirect validation.
/// The supplied HttpClient MUST have automatic redirects disabled.
/// </summary>
public sealed class MicrosoftLearnDocumentFetcher
{
    private const int MaxRedirects = 5;
    private const int MaxContentBytes = 2 * 1024 * 1024;

    private readonly HttpClient _httpClient;
    private readonly MicrosoftLearnSourcePolicy _sourcePolicy;

    public MicrosoftLearnDocumentFetcher(
        HttpClient httpClient,
        MicrosoftLearnSourcePolicy sourcePolicy)
    {
        _httpClient = httpClient
            ?? throw new ArgumentNullException(nameof(httpClient));

        _sourcePolicy = sourcePolicy
            ?? throw new ArgumentNullException(nameof(sourcePolicy));
    }

    public async Task<MicrosoftLearnDocument> FetchAsync(
        Uri sourceUri,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sourceUri);

        if (!_sourcePolicy.IsAllowed(sourceUri))
        {
            throw new InvalidOperationException(
                "The requested URL is not an approved Microsoft Learn URL.");
        }

        var currentUri = sourceUri;

        for (var redirectCount = 0;
             redirectCount <= MaxRedirects;
             redirectCount++)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                currentUri);

            using var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            // Fail closed if a misconfigured HttpClient followed a redirect.
            // This detects the mistake, but does not replace disabling
            // automatic redirects at HttpClient construction time.
            if (response.RequestMessage?.RequestUri != currentUri)
            {
                throw new InvalidOperationException(
                    "Unexpected HTTP redirect. Automatic redirects must be disabled.");
            }

            if (IsRedirect(response.StatusCode))
            {
                if (redirectCount == MaxRedirects)
                {
                    throw new InvalidOperationException(
                        "Microsoft Learn redirect limit exceeded.");
                }

                var location = response.Headers.Location
                    ?? throw new InvalidOperationException(
                        "Redirect response did not contain a Location header.");

                var nextUri = location.IsAbsoluteUri
                    ? location
                    : new Uri(currentUri, location);

                if (!_sourcePolicy.IsAllowed(nextUri))
                {
                    throw new InvalidOperationException(
                        "Redirect target is not an approved Microsoft Learn URL.");
                }

                currentUri = nextUri;
                continue;
            }

            response.EnsureSuccessStatusCode();

            var mediaType = response.Content.Headers.ContentType?.MediaType;

            if (!string.Equals(
                    mediaType,
                    "text/html",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Microsoft Learn response is not HTML.");
            }

            if (response.Content.Headers.ContentLength > MaxContentBytes)
            {
                throw new InvalidOperationException(
                    "Microsoft Learn response exceeds the size limit.");
            }

            await using var stream =
                await response.Content.ReadAsStreamAsync(cancellationToken);

            using var buffer = new MemoryStream();

            var chunk = new byte[8192];
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(
                       chunk,
                       cancellationToken)) != 0)
            {
                if (buffer.Length + bytesRead > MaxContentBytes)
                {
                    throw new InvalidOperationException(
                        "Microsoft Learn response exceeds the size limit.");
                }

                buffer.Write(chunk, 0, bytesRead);
            }

            buffer.Position = 0;

            using var reader = new StreamReader(
                buffer,
                System.Text.Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true);

            var content = await reader.ReadToEndAsync(cancellationToken);

            return new MicrosoftLearnDocument
            {
                SourceUri = currentUri,
                Content = content,
                RetrievedUtc = DateTimeOffset.UtcNow
            };
        }

        throw new InvalidOperationException(
            "Microsoft Learn redirect limit exceeded.");
    }

    private static bool IsRedirect(HttpStatusCode statusCode)
    {
        return statusCode is
            HttpStatusCode.MovedPermanently or
            HttpStatusCode.Redirect or
            HttpStatusCode.RedirectMethod or
            HttpStatusCode.TemporaryRedirect or
            HttpStatusCode.PermanentRedirect;
    }
}