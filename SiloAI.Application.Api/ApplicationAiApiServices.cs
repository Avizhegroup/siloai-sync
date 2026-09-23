using Microsoft.Extensions.DependencyInjection;
using SiloAI.Application.Api.Services;

namespace SiloAI.Application.Api;

public static class ApplicationAiApiServices
{
    public static IServiceCollection AddApplicationAiApiServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationAiApiServices).Assembly));

        services.AddScoped<IPricingEngine, PricingEngine>();

        services.AddScoped<ICreditLedgerService, CreditLedgerService>();

        return services;
    }
}
