namespace ADOps.Infrastructure.Knowledge;

/// <summary>
/// Creates HTTP clients configured for Microsoft Learn document retrieval.
/// </summary>
public static class MicrosoftLearnHttpClientFactory
{
    public static HttpClientHandler CreateHandler()
    {
        return new HttpClientHandler
        {
            AllowAutoRedirect = false,
            UseCookies = false
        };
    }

    public static HttpClient CreateClient()
    {
        return new HttpClient(CreateHandler())
        {
            Timeout = TimeSpan.FromSeconds(15)
        };
    }
}