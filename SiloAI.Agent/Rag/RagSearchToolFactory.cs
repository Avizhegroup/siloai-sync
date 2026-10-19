using Microsoft.Extensions.AI;
using SiloAI.Application.Shared.Contracts.Rag;
using SiloAI.Application.Shared.Features;

namespace SiloAI.Agent.Rag;

/// <summary>
/// Snapshot of a RAG search hit shaped for the model to consume as a tool result — only what it
/// needs to answer (file name, position, text, similarity), not the internal chunk/document ids
/// RagSearchHit also carries.
/// </summary>
public sealed record RagSearchToolResult(string FileName, int ChunkIndex, string Content, double Similarity);

/// <summary>
/// Builds an <see cref="AIFunction"/> that lets a chat agent call <see cref="IRagSearchService"/>
/// itself, on demand, as a tool call — instead of (or alongside) a single fixed retrieval pass a
/// caller performs before the model ever runs.
///
/// This exists for the case the agent's existing context (e.g. the chunks RagChatSendHandler
/// already augments the user's message with) turns out not to answer the question, or the
/// conversation has moved on since the turn where retrieval last ran — the model can decide, on
/// its own, to search again with a refined query rather than being stuck with whatever context
/// was gathered up front.
///
/// docType and key are bound here, from the caller's own request, at the point the tool is
/// created — never exposed as parameters the model fills in. Letting the model choose which
/// document scope to search would let it reach outside the conversation's intended scope (e.g. a
/// customer's own Report-scoped conversation pulling from another customer's indexed documents).
/// Only the search query itself, and optionally how many results to ask for, are left to the
/// model.
/// </summary>
public class RagSearchToolFactory(IRagSearchService ragSearchService)
{
    public const string ToolName = "search_knowledge_base";

    public AIFunction Create(RagDocType? docType, string? key, int defaultTopK = 5)
    {
        async Task<IReadOnlyList<RagSearchToolResult>> SearchKnowledgeBase(
            string query,
            int? resultCount,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return [];
            }

            var top = Math.Clamp(resultCount ?? defaultTopK, 1, 20);

            var hits = await ragSearchService.SearchAsync(query, top, docType, key, cancellationToken);

            return hits
                .Select(h => new RagSearchToolResult(h.FileName, h.ChunkIndex, h.Content, h.Similarity))
                .ToList();
        }

        return AIFunctionFactory.Create(
            SearchKnowledgeBase,
            name: ToolName,
            description:
                "Searches the knowledge base for information relevant to a query, within the " +
                "current conversation's own document scope. Call this when the context you " +
                "already have does not answer the user's question, or when the conversation has " +
                "moved on to something the earlier context does not cover. Returns the most " +
                "relevant snippets found, each with its source file name. Prefer a focused, " +
                "specific query over the user's raw message when the two differ.");
    }
}
