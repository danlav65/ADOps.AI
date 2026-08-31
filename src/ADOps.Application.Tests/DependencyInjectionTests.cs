using ADOps.Application;
using ADOps.Application.Investigation;
using ADOps.Core.Interfaces;
using ADOps.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace ADOps.Application.Tests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddApplicationAndInfrastructure_ResolvesInvestigationService()
    {
        var services =
            new ServiceCollection();

        services.AddApplication();
        services.AddInfrastructure();

        using var provider =
            services.BuildServiceProvider();

        var investigationService =
            provider.GetRequiredService<IInvestigationService>();

        Assert.NotNull(investigationService);
    }
}
