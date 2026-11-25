using Microsoft.Extensions.DependencyInjection;
using SiloAI.Application.Api.Services;

namespace SiloAI.Application.Api;

public static class ApplicationAiApiServices
{
    public static IServiceCollection AddApplicationAiApiServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationAiApiServices).Assembly));

        // Fills CustomerId / OwnerId / Username from the authenticated caller (ICustomerContext).
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Behaviors.CallerBindingBehavior<,>));

        services.AddScoped<IPricingEngine, PricingEngine>();

        services.AddScoped<ICreditLedgerService, CreditLedgerService>();

        services.AddScoped<IAiModelResolver, AiModelResolver>();

        services.AddScoped<ISpeechModelService, SpeechModelService>();

        services.AddScoped<ISpeechToTextService, OpenAISpeechToTextService>();

        services.AddScoped<ITextToSpeechService, OpenAITextToSpeechService>();

        services.AddScoped<ISpeechBillingService, SpeechBillingService>();

        services.AddScoped<IRagVoiceChatService, RagVoiceChatService>();

        return services;
    }
}
