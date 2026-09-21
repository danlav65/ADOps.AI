namespace ADOps.Infrastructure.Knowledge;

/// <summary>
/// Restricts live knowledge retrieval to Microsoft Learn HTTPS URLs.
/// </summary>
public sealed class MicrosoftLearnSourcePolicy
{
    public bool IsAllowed(Uri? uri)
    {
        if (uri is null || !uri.IsAbsoluteUri)
        {
            return false;
        }

        return uri.Scheme == Uri.UriSchemeHttps
            && uri.Host.Equals(
                "learn.microsoft.com",
                StringComparison.OrdinalIgnoreCase)
            && uri.IsDefaultPort
            && string.IsNullOrEmpty(uri.UserInfo)
            && string.IsNullOrEmpty(uri.Fragment);
    }
}