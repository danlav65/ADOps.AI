using Microsoft.Extensions.DependencyInjection;
using ADOps.Application.Investigation;
using ADOps.Application.Presentation;

namespace ADOps.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<
            IInvestigationService,
            InvestigationService>();

        services.AddScoped<
            InvestigationPresenter>();

        return services;
    }
}