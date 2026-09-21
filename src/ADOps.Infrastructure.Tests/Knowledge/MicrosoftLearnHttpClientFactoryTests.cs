using ADOps.Infrastructure.Knowledge;

namespace ADOps.Infrastructure.Tests.Knowledge;

public sealed class MicrosoftLearnHttpClientFactoryTests
{
    [Fact]
    public void CreateHandler_DisablesAutomaticRedirects()
    {
        using var handler =
            MicrosoftLearnHttpClientFactory.CreateHandler();

        Assert.False(handler.AllowAutoRedirect);
    }

    [Fact]
    public void CreateHandler_DisablesCookies()
    {
        using var handler =
            MicrosoftLearnHttpClientFactory.CreateHandler();

        Assert.False(handler.UseCookies);
    }

    [Fact]
    public void CreateClient_ConfiguresFiniteTimeout()
    {
        using var client =
            MicrosoftLearnHttpClientFactory.CreateClient();

        Assert.Equal(
            TimeSpan.FromSeconds(15),
            client.Timeout);
    }
}