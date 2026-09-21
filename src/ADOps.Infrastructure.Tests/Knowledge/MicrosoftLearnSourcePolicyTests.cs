using ADOps.Infrastructure.Knowledge;

namespace ADOps.Infrastructure.Tests.Knowledge;

public sealed class MicrosoftLearnSourcePolicyTests
{
    private readonly MicrosoftLearnSourcePolicy _policy = new();

    [Theory]
    [InlineData("https://learn.microsoft.com/en-us/windows-server/identity/")]
    [InlineData("https://LEARN.MICROSOFT.COM/en-us/")]
    public void IsAllowed_AcceptsMicrosoftLearnHttpsUrls(string url)
    {
        Assert.True(_policy.IsAllowed(new Uri(url)));
    }

    [Theory]
    [InlineData("http://learn.microsoft.com/en-us/")]
    [InlineData("https://learn.microsoft.com.evil.example/en-us/")]
    [InlineData("https://evil.example/")]
    [InlineData("https://sub.learn.microsoft.com/")]
    [InlineData("https://learn.microsoft.com:8443/en-us/")]
    [InlineData("https://user:password@learn.microsoft.com/en-us/")]
    public void IsAllowed_RejectsUnapprovedUrls(string url)
    {
        Assert.False(_policy.IsAllowed(new Uri(url)));
    }

    [Fact]
    public void IsAllowed_RejectsNull()
    {
        Assert.False(_policy.IsAllowed(null));
    }

    [Fact]
    public void IsAllowed_RejectsRelativeUri()
    {
        Assert.False(
            _policy.IsAllowed(
                new Uri("/en-us/windows-server/", UriKind.Relative)));
    }

    [Fact]
    public void IsAllowed_RejectsUrlFragment()
    {
        var policy = new MicrosoftLearnSourcePolicy();

        var uri = new Uri(
            "https://learn.microsoft.com/en-us/windows-server/#section");

        Assert.False(policy.IsAllowed(uri));
    }
}