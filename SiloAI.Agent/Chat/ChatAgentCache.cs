using System.Collections.Concurrent;
using Microsoft.Agents.AI;

namespace SiloAI.Agent.Chat;

/// <summary>
/// Process-wide cache of built <see cref="AIAgent"/> instances, keyed by model, instructions,
/// and RAG-context settings. Avoids rebuilding a new ChatClient/ChatClientAgent (and, when
/// enabled, a new TextSearchProvider) on every single chat message when nothing about the
/// agent's configuration has actually changed.
///
/// Agent instances are safe to share across concurrent requests: per-conversation state lives
/// in the <c>AgentSession</c> passed explicitly to <c>RunAsync</c>, not on the agent itself.
///
/// The cache key includes the full instructions text, so it self-invalidates automatically
/// whenever RagInstructions content changes for a DocType — no explicit cache-eviction wiring
/// is required. Note: stale entries for previously-seen instruction text are not proactively
/// evicted, so this is a simple unbounded cache, not an LRU — acceptable given instruction sets
/// are edited rarely and are small in number, but worth revisiting if that assumption changes.
/// </summary>
public sealed class ChatAgentCache
{
    private readonly ConcurrentDictionary<string, AIAgent> _cache = new();

    public AIAgent GetOrCreate(string cacheKey, Func<AIAgent> factory)
        => _cache.GetOrAdd(cacheKey, _ => factory());

    public static string BuildKey(
        string? modelName,
        string instructions,
        bool includeAutoRagContext,
        string? ragDocType,
        string? ragKey)
        => $"{modelName}|{includeAutoRagContext}|{ragDocType}|{ragKey}|{instructions}";
}
