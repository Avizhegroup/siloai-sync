using Microsoft.Extensions.AI;
using SiloAI.Agent.Chat;
using Xunit;

namespace SiloAI.Tests;

/// <summary>
/// Regression guard for "the chat forgets earlier messages": retrieved chunks must reach the model
/// for the current turn only, never be stored in the conversation history.
/// </summary>
public class TurnMessageSubstitutionTests
{
    private static List<ChatMessage> History() =>
    [
        new(ChatRole.User, "first question"),
        new(ChatRole.Assistant, "first answer"),
        new(ChatRole.User, "second question")
    ];

    [Fact]
    public void Without_a_substitution_messages_are_unchanged()
    {
        var messages = History();

        var result = TurnMessageSubstitution.Apply(messages).ToList();

        Assert.Equal(new[] { "first question", "first answer", "second question" }, result.Select(m => m.Text).ToArray());
    }

    [Fact]
    public void Only_the_current_user_message_is_replaced_and_the_input_list_is_not_mutated()
    {
        var messages = History();

        using (TurnMessageSubstitution.Begin("second question", "CHUNKS + second question"))
        {
            var result = TurnMessageSubstitution.Apply(messages).ToList();

            Assert.Equal(new[] { "first question", "first answer", "CHUNKS + second question" }, result.Select(m => m.Text).ToArray());
            Assert.Equal(ChatRole.User, result[2].Role);
        }

        // The list the agent stores from is untouched.
        Assert.Equal("second question", messages[2].Text);
    }

    [Fact]
    public void The_most_recent_matching_message_wins_when_the_same_text_was_asked_before()
    {
        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, "same"),
            new(ChatRole.Assistant, "a"),
            new(ChatRole.User, "same")
        };

        using (TurnMessageSubstitution.Begin("same", "augmented"))
        {
            var result = TurnMessageSubstitution.Apply(messages).ToList();

            Assert.Equal("same", result[0].Text);
            Assert.Equal("augmented", result[2].Text);
        }
    }

    [Fact]
    public void Disposing_the_scope_ends_the_substitution()
    {
        using (TurnMessageSubstitution.Begin("second question", "augmented")) { }

        var result = TurnMessageSubstitution.Apply(History()).ToList();

        Assert.Equal("second question", result[2].Text);
    }

    [Fact]
    public async Task The_chat_client_sends_the_augmented_text_to_the_model()
    {
        var inner = new CapturingClient();
        var client = new TurnMessageSubstitutingChatClient(inner);

        using (TurnMessageSubstitution.Begin("second question", "CHUNKS + second question"))
        {
            await client.GetResponseAsync(History());
        }

        Assert.Equal(
            new[] { "first question", "first answer", "CHUNKS + second question" },
            inner.Received.Select(m => m.Text).ToArray());
    }

    private sealed class CapturingClient : IChatClient
    {
        public List<ChatMessage> Received = [];

        public Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        {
            Received = messages.ToList();
            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "ok")));
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose() { }
    }
}
