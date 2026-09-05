using Microsoft.Agents.AI;
using SiloAI.Application.Shared.Contracts.Rag;

namespace SiloAI.Agent.Rag;

public class RagContextProviderFactory(IRagSearchService ragSearchService)
{
    public TextSearchProvider Create(int topK = 5, string? docType = null, string? key = null)
    {
        return new TextSearchProvider(
            async (query, cancellationToken) =>
            {
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
