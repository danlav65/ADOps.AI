using ADOps.Core.Interfaces;
using ADOps.Infrastructure;
using ADOps.Infrastructure.Knowledge;
using Microsoft.Extensions.DependencyInjection;

namespace ADOps.Infrastructure.Tests.Knowledge;

public sealed class MicrosoftLearnDependencyInjectionTests
{
    [Fact]
    public void AddInfrastructure_RegistersMicrosoftLearnFetcher()
    {
        var services = new ServiceCollection();

        services.AddInfrastructure();

        using var provider = services.BuildServiceProvider(
            validateScopes: true);

        var fetcher =
            provider.GetRequiredService<
                MicrosoftLearnDocumentFetcher>();

        var secondResolution =
            provider.GetRequiredService<
                MicrosoftLearnDocumentFetcher>();

        Assert.NotNull(fetcher);
        Assert.Same(fetcher, secondResolution);
    }

    [Fact]
    public void AddInfrastructure_PreservesExistingKnowledgeRetriever()
    {
        var services = new ServiceCollection();

        services.AddInfrastructure();

        using var provider = services.BuildServiceProvider(
            validateScopes: true);

        using var scope = provider.CreateScope();

        var retriever =
            scope.ServiceProvider.GetRequiredService<
                IKnowledgeRetriever>();

        Assert.IsType<InMemoryKnowledgeRetriever>(retriever);
    }
}