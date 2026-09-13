using System.Collections.Concurrent;
using Microsoft.Agents.AI;
using SiloAI.Application.Shared.Features;

namespace SiloAI.Agent.Chat;

/// <summary>
/// Snapshot of the RagInstruction fields consumers actually need, cached exactly as read from the
/// database (no joining, trimming or other transformation applied).
/// </summary>
public sealed record CachedRagInstruction(string Content, bool IsSystematic, DateTime CreateDateTime);

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

    /// <summary>
    /// Active RagInstruction rows per DocType, cached exactly as read from the database — no
    /// joining, trimming or any other transformation — so callers stay free to compose them
    /// however they need. Invalidated explicitly whenever a RagInstruction is created, updated
    /// or deleted.
    /// </summary>
    private readonly ConcurrentDictionary<int, IReadOnlyList<CachedRagInstruction>> _instructionsCache = new();

    public AIAgent GetOrCreate(string cacheKey, Func<AIAgent> factory)
        => _cache.GetOrAdd(cacheKey, _ => factory());

    /// <summary>
    /// Returns the cached rows for <paramref name="docType"/>, loading them through
    /// <paramref name="factory"/> on a cache miss.
    /// </summary>
    public async Task<IReadOnlyList<CachedRagInstruction>> GetOrCreateInstructionsAsync(
        int docType,
        Func<Task<IReadOnlyList<CachedRagInstruction>>> factory)
    {
        if (_instructionsCache.TryGetValue(docType, out var cached))
        {
            return cached;
        }

        var contents = await factory();

        _instructionsCache[docType] = contents;

        return contents;
    }

    /// <summary>
    /// Drops the cached instruction contents for a single DocType. Call this after any
    /// create/update/delete of a RagInstruction so the next read re-loads from the database.
    /// </summary>
    public void InvalidateInstructions(int docType)
        => _instructionsCache.TryRemove(docType, out _);

    /// <summary>
    /// Drops all cached instruction contents.
    /// </summary>
    public void InvalidateAllInstructions()
        => _instructionsCache.Clear();

    public static string BuildKey(
        string? modelName,
        string instructions,
        bool includeAutoRagContext,
        RagDocType? ragDocType,
        string? ragKey)
        => $"{modelName}|{includeAutoRagContext}|{ragDocType}|{ragKey}|{instructions}";
}
