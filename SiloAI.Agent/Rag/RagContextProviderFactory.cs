using Microsoft.Agents.AI;
using Microsoft.Extensions.DependencyInjection;
using SiloAI.Application.Shared.Contracts.Rag;
using SiloAI.Application.Shared.Features;

namespace SiloAI.Agent.Rag;

public class RagContextProviderFactory(IServiceScopeFactory scopeFactory)
{
    public TextSearchProvider Create(int topK = 5, RagDocType? docType = null, string? key = null)
    {
        var serviceScopeFactory = scopeFactory;

        return new TextSearchProvider(
            async (query, cancellationToken) =>
            {
                using var scope = serviceScopeFactory.CreateScope();
                var ragSearchService = scope.ServiceProvider.GetRequiredService<IRagSearchService>();
                var hits = await ragSearchService.SearchAsync(
                    query, topK, docType, key, cancellationToken);

                return hits.Select(h => new TextSearchProvider.TextSearchResult
                {
                    SourceName = h.FileName,
                    Text = h.Content
                });
            },
            new TextSearchProviderOptions
            {
                SearchTime = TextSearchProviderOptions.TextSearchBehavior.BeforeAIInvoke,
                ContextFormatter = static results =>
                {
                    if (results.Count == 0)
                    {
                        return string.Empty;
                    }

                    return string.Join(
                        Environment.NewLine + "---" + Environment.NewLine,
                        results.Select(r => r.Text));
                }
            });
    }
}
