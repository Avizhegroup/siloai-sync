using Microsoft.Agents.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using SiloAI.Agent.Chat;
using SiloAI.Agent.Rag;
using SiloAI.Application.Shared.Contracts.AiModels;
using SiloAI.Application.Shared.Features;
using SiloAI.Domains;
using System.ClientModel;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace SiloAI.Agent.Tasks;

/// <summary>
/// Non-chat agent tasks: one file in, one text answer out, no AgentSession and no conversation
/// history. Uses the same <see cref="ChatAgentCache"/> as ChatAgentService (a shared, generic
/// agent/instructions cache — not chat-specific), but builds and validates its own agents
/// independently, so a chat-configured agent and a file-task-configured agent are never mixed up.
/// </summary>
public class AgentFileTaskService(
    IOptions<OpenAIOptions> options,
    AiApiContext context,
    ChatAgentCache agentCache)
{
    /// <param name="model">
    /// The model to call, resolved by the caller via <c>IAiModelResolver</c>. Must support file
    /// input and text output.
    /// </param>
    /// <param name="promptKey">DocType whose active RagInstructions become the task's system instructions.</param>
    public async Task<string> ExtractTextFromFileAsync(
        byte[] fileData,
        string fileMediaType,
        ResolvedAiModel model,
        RagDocType promptKey,
        CancellationToken cancellationToken = default)
    {
        if (fileData is null || fileData.Length == 0)
        {
            throw new ArgumentException("File data cannot be null or empty.", nameof(fileData));
        }

        if (!model.SupportsFileInput || !model.SupportsTextOutput)
            throw new InvalidOperationException(
                $"Model '{model.Identifier}' does not support file input with text output and " +
                "cannot be used with AgentFileTaskService.");

        var instructions = await LoadInstructionsAsync(promptKey, cancellationToken);

        var agent = GetOrCreateAgent(model.Identifier, instructions);

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, instructions),
            new(ChatRole.User, new List<AIContent> { new DataContent(fileData, fileMediaType) })
        };

        var response = await agent.RunAsync(messages);

        return response?.ToString() ?? string.Empty;
    }

    public async Task<string> ExtractTextFromFileAsync(
        Stream fileStream,
        string fileMediaType,
        ResolvedAiModel model,
        RagDocType promptKey,
        CancellationToken cancellationToken = default)
    {
        if (fileStream is null)
        {
            throw new ArgumentNullException(nameof(fileStream));
        }

        using var memoryStream = new MemoryStream();

        await fileStream.CopyToAsync(memoryStream, cancellationToken);

        var fileData = memoryStream.ToArray();

        return await ExtractTextFromFileAsync(fileData, fileMediaType, model, promptKey, cancellationToken);
    }

    private AIAgent GetOrCreateAgent(string modelIdentifier, string instructions)
    {
        var cacheKey = ChatAgentCache.BuildKey(
            modelIdentifier, instructions, includeAutoRagContext: false, ragDocType: null, ragKey: null);

        return agentCache.GetOrCreate(cacheKey, () =>
        {
            var chatClient = new ChatClient(modelIdentifier,
                new ApiKeyCredential(options.Value.ApiKey),
                new OpenAIClientOptions { Endpoint = new Uri(options.Value.Endpoint) })
                .AsIChatClient();

            return new ChatClientAgent(chatClient, new ChatClientAgentOptions
            {
                ChatOptions = new()
                {
                    Instructions = instructions,
                }
            });
        });
    }

    private async Task<string> LoadInstructionsAsync(RagDocType promptKey, CancellationToken cancellationToken)
    {
        var docTypeValue = (int)promptKey;

        var cachedInstructions = await agentCache.GetOrCreateInstructionsAsync(docTypeValue, async () =>
            (IReadOnlyList<CachedRagInstruction>)await context.RagInstructions
                .Where(p => p.DocType == docTypeValue && p.IsActive)
                .AsNoTracking()
                .Select(p => new CachedRagInstruction(p.Content, p.IsSystematic, p.CreateDateTime))
                .ToListAsync(cancellationToken));

        return string.Join(Environment.NewLine, cachedInstructions.Select(i => i.Content));
    }
}
