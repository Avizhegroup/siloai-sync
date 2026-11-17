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

public sealed record ImageAnalysisInput(byte[] Data, string MediaType);

public sealed record ImageAnalysisResult(string Text, long InputTokens, long CachedInputTokens, long OutputTokens);

public interface IImageAnalysisService
{
    Task<ImageAnalysisResult> AnalyzeImagesAsync(
        IReadOnlyList<ImageAnalysisInput> images, string? prompt, ResolvedAiModel model,
        RagDocType docType, string? key, CancellationToken cancellationToken = default);
}

/// <summary>
/// Non-chat agent tasks: one file in, one text answer out, no AgentSession and no conversation
/// history. Uses the same <see cref="ChatAgentCache"/> as ChatAgentService (a shared, generic
/// agent/instructions cache — not chat-specific), but builds and validates its own agents
/// independently, so a chat-configured agent and a file-task-configured agent are never mixed up.
/// </summary>
public class AgentFileTaskService(
    IOptions<OpenAIOptions> options,
    AiApiContext context,
    ChatAgentCache agentCache) : IImageAnalysisService
{
    /// <summary>
    /// Image analysis: instructions (DocType + Key) + an optional text request + one or more
    /// images in, a text answer out. Stateless, no RAG. Unlike <see cref="ExtractTextFromFileAsync"/>
    /// it also reports the model's token usage so the caller can bill the real cost.
    /// </summary>
    public async Task<ImageAnalysisResult> AnalyzeImagesAsync(
        IReadOnlyList<ImageAnalysisInput> images,
        string? prompt,
        ResolvedAiModel model,
        RagDocType docType,
        string? key,
        CancellationToken cancellationToken = default)
    {
        if (images is null || images.Count == 0)
            throw new ArgumentException("At least one image is required.", nameof(images));

        ValidateAnalysisModel(model);

        var instructions = await LoadInstructionsAsync(docType, key!, cancellationToken);

        if (string.IsNullOrWhiteSpace(instructions) && string.IsNullOrWhiteSpace(prompt))
            throw new InvalidOperationException(
                "Nothing tells the model what to analyze: send a prompt or add active instructions for this type/key.");

        var agent = GetOrCreateAgent(model.Identifier, instructions);

        var contents = new List<AIContent>();
        if (!string.IsNullOrWhiteSpace(prompt)) contents.Add(new TextContent(prompt.Trim()));
        contents.AddRange(images.Select(i => (AIContent)new DataContent(i.Data, i.MediaType)));

        // Instructions travel once, through ChatOptions.Instructions on the cached agent.
        var response = await agent.RunAsync(
            new List<ChatMessage> { new(ChatRole.User, contents) },
            cancellationToken: cancellationToken);

        var text = response?.ToString()?.Trim() ?? string.Empty;
        if (text.Length == 0)
            throw new InvalidOperationException("The model returned no analysis text.");

        return new ImageAnalysisResult(
            text,
            response!.Usage?.InputTokenCount ?? 0,
            response.Usage?.CachedInputTokenCount ?? 0,
            response.Usage?.OutputTokenCount ?? 0);
    }

    internal static void ValidateAnalysisModel(ResolvedAiModel model)
    {
        if (!model.SupportsImageInput || !model.SupportsTextOutput)
            throw new InvalidOperationException(
                $"Model '{model.Identifier}' does not support image input with text output and cannot be " +
                "used for image analysis. Enable those capabilities on the AI Models page.");
    }

    /// <param name="model">
    /// The model to call, resolved by the caller via <c>IAiModelResolver</c>. Must support file
    /// input and text output.
    /// </param>
    /// <param name="docType">DocType whose active RagInstructions become the task's system instructions.</param>
    public async Task<string> ExtractTextFromFileAsync(
        byte[] fileData,
        string fileMediaType,
        ResolvedAiModel model,
        RagDocType docType,
        string key,
        CancellationToken cancellationToken = default)
    {
        if (fileData is null || fileData.Length == 0)
        {
            throw new ArgumentException("File data cannot be null or empty.", nameof(fileData));
        }

        //if (!model.SupportsFileInput || !model.SupportsTextOutput)
        //    throw new InvalidOperationException(
        //        $"Model '{model.Identifier}' does not support file input with text output and " +
        //        "cannot be used with AgentFileTaskService.");

        var instructions = await LoadInstructionsAsync(docType, key, cancellationToken);

        var agent = GetOrCreateAgent(model.Identifier, instructions);

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, instructions),
            new(ChatRole.User, new List<AIContent> { new DataContent(fileData, fileMediaType) })
        };

        var response = await agent.RunAsync(messages);

        return response?.ToString() ?? string.Empty;
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

    private async Task<string> LoadInstructionsAsync(RagDocType docType, string key, CancellationToken cancellationToken)
    {
        var docTypeValue = (int)docType;

        // Keyed by (DocType, Key): the query below filters by Key, so a DocType-only cache entry
        // would serve the first Key's instructions to every other Key.
        var cachedInstructions = await agentCache.GetOrCreateInstructionsAsync(docTypeValue, key, async () =>
            (IReadOnlyList<CachedRagInstruction>)await context.RagInstructions
                .Where(p => p.DocType == docTypeValue 
                         && p.Key == key
                         && p.IsActive)
                .AsNoTracking()
                .Select(p => new CachedRagInstruction(p.Content, p.IsSystematic, p.CreateDateTime))
                .ToListAsync(cancellationToken));

        return string.Join(Environment.NewLine, cachedInstructions.Select(i => i.Content));
    }
}
