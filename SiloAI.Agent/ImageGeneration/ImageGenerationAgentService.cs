using Microsoft.Agents.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using SiloAI.Agent.Chat;
using SiloAI.Agent.Rag;
using SiloAI.Application.Shared.Contracts.AiModels;
using SiloAI.Application.Shared.Features;
using SiloAI.Domains;

namespace SiloAI.Agent.ImageGeneration;

public sealed record ImageGenerationAgentResult(
    IReadOnlyList<GeneratedImageDto> Images,
    string? Text,
    long InputTokens,
    long CachedInputTokens,
    long OutputTokens);

public interface IImageGenerationAgentService
{
    Task<ImageGenerationAgentResult> GenerateAsync(
        string prompt, IReadOnlyList<ImageGenerationInputImage> images, ResolvedAiModel model,
        string? key, CancellationToken cancellationToken = default);
}

/// <summary>
/// Stateless task agent: instructions + text request + input images in, generated image(s) out.
/// A MAF <see cref="ChatClientAgent"/> (cached in the shared <see cref="ChatAgentCache"/>, keyed
/// separately from chat/file-task agents) runs on <see cref="OpenAiCompatibleImageChatClient"/>.
/// No RAG, no session, no search tool.
/// </summary>
public class ImageGenerationAgentService(
    IOptions<OpenAIOptions> options,
    AiApiContext context,
    ChatAgentCache agentCache) : IImageGenerationAgentService
{
    private static readonly HttpClient SharedHttp = new() { Timeout = TimeSpan.FromMinutes(5) };

    internal static readonly string[] AllowedImageTypes = ["image/png", "image/jpeg", "image/webp", "image/gif"];

    public async Task<ImageGenerationAgentResult> GenerateAsync(
        string prompt,
        IReadOnlyList<ImageGenerationInputImage> images,
        ResolvedAiModel model,
        string? key,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Prompt cannot be empty.", nameof(prompt));

        ValidateModel(model, images.Count > 0);

        var instructions = await LoadInstructionsAsync(key, cancellationToken);

        var agent = GetOrCreateAgent(model.Identifier, instructions);

        var contents = new List<AIContent> { new TextContent(prompt) };
        contents.AddRange(images.Select(i => (AIContent)new DataContent(i.Data, i.MediaType)));

        var response = await agent.RunAsync(
            new List<Microsoft.Extensions.AI.ChatMessage> { new(ChatRole.User, contents) },
            cancellationToken: cancellationToken);

        return ToResult(response.Messages.SelectMany(m => m.Contents).ToList(),
            response.Usage?.InputTokenCount ?? 0,
            response.Usage?.CachedInputTokenCount ?? 0,
            response.Usage?.OutputTokenCount ?? 0);
    }

    internal static void ValidateModel(ResolvedAiModel model, bool hasInputImages)
    {
        if (!model.SupportsTextInput || !model.SupportsImageOutput)
            throw new InvalidOperationException(
                $"Model '{model.Identifier}' does not support text input with image output and cannot be " +
                "used for image generation. Enable those capabilities on the AI Models page.");

        if (hasInputImages && !model.SupportsImageInput)
            throw new InvalidOperationException(
                $"Model '{model.Identifier}' does not support image input, but input images were supplied.");
    }

    internal static ImageGenerationAgentResult ToResult(
        IReadOnlyList<AIContent> contents, long inputTokens, long cachedTokens, long outputTokens)
    {
        var images = new List<GeneratedImageDto>();

        foreach (var content in contents)
        {
            if (content is DataContent d && d.MediaType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == true)
                images.Add(new GeneratedImageDto { DataBase64 = Convert.ToBase64String(d.Data.ToArray()), MediaType = d.MediaType });
        }

        var text = string.Concat(contents.OfType<TextContent>().Select(t => t.Text)).Trim();

        if (images.Count == 0)
            throw new InvalidOperationException(
                "The model did not return an image." + (text.Length > 0 ? $" It said: {text}" : string.Empty));

        return new ImageGenerationAgentResult(images, text.Length > 0 ? text : null, inputTokens, cachedTokens, outputTokens);
    }

    private AIAgent GetOrCreateAgent(string modelIdentifier, string instructions)
    {
        // Prefixed so this agent never collides with a chat/file-task agent for the same model + text.
        var cacheKey = ChatAgentCache.BuildKey(
            "imagegen:" + modelIdentifier, instructions, includeAutoRagContext: false, ragDocType: null, ragKey: null);

        return agentCache.GetOrCreate(cacheKey, () =>
        {
            IChatClient chatClient = new OpenAiCompatibleImageChatClient(
                SharedHttp, new Uri(options.Value.Endpoint!), options.Value.ApiKey!, modelIdentifier);

            return new ChatClientAgent(chatClient, new ChatClientAgentOptions
            {
                ChatOptions = new() { Instructions = instructions }
            });
        });
    }

    private async Task<string> LoadInstructionsAsync(string? key, CancellationToken cancellationToken)
    {
        var docType = (int)RagDocType.ImageGeneration;
        var hasKey = !string.IsNullOrWhiteSpace(key);

        var items = await context.RagInstructions
            .Where(p => p.DocType == docType && p.IsActive
                        && (hasKey ? p.Key == key : (p.Key == null || p.Key == "")))
            .AsNoTracking()
            .OrderBy(p => p.CreateDateTime)
            .Select(p => p.Content)
            .ToListAsync(cancellationToken);

        return string.Join(Environment.NewLine + "---" + Environment.NewLine, items);
    }
}
