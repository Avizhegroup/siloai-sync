using CommunityToolkit.VectorData.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SiloAI.Domains;

namespace SiloAI.Agent.Rag;

internal static class RagDocumentChunkCollectionServiceCollectionExtensions
{
    public static IServiceCollection AddRagDocumentChunkCollection(this IServiceCollection services)
    {
        services.AddScoped<RagDocumentChunkCollectionOptionsFactory>();

        services.AddSqlServerCollection<Guid, RagDocumentChunk>(
            name: "tbl_RagDocumentChunks",
            connectionStringProvider: static serviceProvider =>
                serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString("AiDb")!,
            optionsProvider: static serviceProvider =>
                serviceProvider.GetRequiredService<RagDocumentChunkCollectionOptionsFactory>().Create(),
            ServiceLifetime.Scoped);

        return services;
    }
}
