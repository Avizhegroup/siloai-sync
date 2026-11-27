using Microsoft.Extensions.AI;

namespace SiloAI.Agent.Chat;

/// <summary>
/// Lets a caller show the model a richer version of the current user message (e.g. the question
/// plus retrieved knowledge-base chunks) while the conversation history stores only the plain
/// message. The agent persists exactly the messages it was given, so without this every turn's
/// retrieved chunks pile up in the saved session and the prompt grows by tens of thousands of
/// tokens per turn until the model loses the earlier conversation.
///
/// The substitution is scoped to the current async flow (one agent run), so a cached, shared agent
/// is safe to use from concurrent requests.
/// </summary>
public static class TurnMessageSubstitution
{
    private sealed record Substitution(string StoredText, string ModelText, IReadOnlyList<AIContent> ExtraContents);

    private static readonly AsyncLocal<Substitution?> Current = new();

    /// <summary>
    /// Starts a substitution for the current async flow; dispose to end it.
    /// <paramref name="extraContents"/> (images, files) are shown to the model together with the text
    /// for this turn only; they are never written to the saved conversation.
    /// </summary>
    public static IDisposable Begin(string storedText, string modelText, IReadOnlyList<AIContent>? extraContents = null)
    {
        var previous = Current.Value;
        Current.Value = new Substitution(storedText, modelText, extraContents ?? []);
        return new Scope(previous);
    }

    /// <summary>
    /// Returns <paramref name="messages"/> with the most recent user message whose text equals the
    /// stored text replaced by the model text. Unchanged when no substitution is active.
    /// </summary>
    public static IEnumerable<ChatMessage> Apply(IEnumerable<ChatMessage> messages)
    {
        var substitution = Current.Value;
        if (substitution is null) return messages;

        var list = messages as IList<ChatMessage> ?? messages.ToList();

        for (var i = list.Count - 1; i >= 0; i--)
        {
            var message = list[i];

            if (message.Role == ChatRole.User && string.Equals(message.Text, substitution.StoredText, StringComparison.Ordinal))
            {
                var copy = new List<ChatMessage>(list)
                {
                    [i] = BuildModelMessage(substitution)
                };

                return copy;
            }
        }

        return list;
    }

    private static ChatMessage BuildModelMessage(Substitution substitution)
    {
        if (substitution.ExtraContents.Count == 0)
            return new ChatMessage(ChatRole.User, substitution.ModelText);

        var contents = new List<AIContent> { new TextContent(substitution.ModelText) };
        contents.AddRange(substitution.ExtraContents);

        return new ChatMessage(ChatRole.User, contents);
    }

    private sealed class Scope(Substitution? previous) : IDisposable
    {
        public void Dispose() => Current.Value = previous;
    }
}

/// <summary>Applies <see cref="TurnMessageSubstitution"/> to every model call (including the
/// follow-up calls after a tool invocation, where the current user message is still in the list).</summary>
public sealed class TurnMessageSubstitutingChatClient(IChatClient innerClient) : DelegatingChatClient(innerClient)
{
    public override Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        => base.GetResponseAsync(TurnMessageSubstitution.Apply(messages), options, cancellationToken);

    public override IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        => base.GetStreamingResponseAsync(TurnMessageSubstitution.Apply(messages), options, cancellationToken);
}
