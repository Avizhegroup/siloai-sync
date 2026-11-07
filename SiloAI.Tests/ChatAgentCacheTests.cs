using SiloAI.Agent.Chat;
using Xunit;

namespace SiloAI.Tests;

public class ChatAgentCacheTests
{
    private static Func<Task<IReadOnlyList<CachedRagInstruction>>> Rows(string content, Action? onLoad = null) => () =>
    {
        onLoad?.Invoke();
        return Task.FromResult<IReadOnlyList<CachedRagInstruction>>([new CachedRagInstruction(content, false, DateTime.UnixEpoch)]);
    };

    [Fact]
    public async Task Different_keys_of_the_same_DocType_get_their_own_instructions()
    {
        var cache = new ChatAgentCache();

        var a = await cache.GetOrCreateInstructionsAsync(2, "ocr", Rows("extract text"));
        var b = await cache.GetOrCreateInstructionsAsync(2, "analyze", Rows("analyze the image"));

        Assert.Equal("extract text", Assert.Single(a).Content);
        Assert.Equal("analyze the image", Assert.Single(b).Content);
    }

    [Fact]
    public async Task A_key_is_loaded_once_until_invalidated()
    {
        var cache = new ChatAgentCache();
        var loads = 0;

        await cache.GetOrCreateInstructionsAsync(6, "k", Rows("v1", () => loads++));
        await cache.GetOrCreateInstructionsAsync(6, "k", Rows("v1", () => loads++));
        Assert.Equal(1, loads);

        cache.InvalidateInstructions(6);

        var reloaded = await cache.GetOrCreateInstructionsAsync(6, "k", Rows("v2", () => loads++));
        Assert.Equal(2, loads);
        Assert.Equal("v2", Assert.Single(reloaded).Content);
    }

    [Fact]
    public async Task Invalidating_one_DocType_leaves_other_DocTypes_cached()
    {
        var cache = new ChatAgentCache();
        var loads = 0;

        await cache.GetOrCreateInstructionsAsync(2, "k", Rows("ocr", () => loads++));
        await cache.GetOrCreateInstructionsAsync(6, "k", Rows("analysis", () => loads++));

        cache.InvalidateInstructions(6);

        await cache.GetOrCreateInstructionsAsync(2, "k", Rows("ocr", () => loads++));
        Assert.Equal(2, loads); // DocType 2 was still cached
    }

    [Fact]
    public async Task Null_and_empty_key_share_one_scope()
    {
        var cache = new ChatAgentCache();
        var loads = 0;

        await cache.GetOrCreateInstructionsAsync(6, null, Rows("x", () => loads++));
        await cache.GetOrCreateInstructionsAsync(6, "", Rows("x", () => loads++));

        Assert.Equal(1, loads);
    }

    [Fact]
    public async Task InvalidateAll_clears_keyed_entries()
    {
        var cache = new ChatAgentCache();
        var loads = 0;

        await cache.GetOrCreateInstructionsAsync(6, "k", Rows("x", () => loads++));
        cache.InvalidateAllInstructions();
        await cache.GetOrCreateInstructionsAsync(6, "k", Rows("x", () => loads++));

        Assert.Equal(2, loads);
    }

    [Fact]
    public async Task The_DocType_only_cache_is_unchanged()
    {
        var cache = new ChatAgentCache();
        var loads = 0;

        await cache.GetOrCreateInstructionsAsync(0, Rows("chat", () => loads++));
        await cache.GetOrCreateInstructionsAsync(0, Rows("chat", () => loads++));

        Assert.Equal(1, loads);
    }
}
