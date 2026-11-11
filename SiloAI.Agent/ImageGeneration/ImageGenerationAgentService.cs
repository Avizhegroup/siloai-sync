using Microsoft.EntityFrameworkCore;
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
/// Stateless task: instructions + text request + optional input image in, generated image out.
/// No RAG, no session. The instructions are the active <see cref="RagDocType.ImageGeneration"/>
/// RagInstructions (scoped by <c>key</c>); the image model call goes through <see cref="IImageBackend"/>.
/// </summary>
public class ImageGenerationAgentService(
    IImageBackend backend,
    AiApiContext context) : IImageGenerationAgentService
{
    public const int MaxInputImages = 8;

    public async Task<ImageGenerationAgentResult> GenerateAsync(
        string prompt,
        IReadOnlyList<ImageGenerationInputImage> images,
        ResolvedAiModel model,
        string? key,
        CancellationToken cancellationToken = default)
    {
        var instructions = await LoadInstructionsAsync(key, cancellationToken);

        return await ExecuteAsync(BuildPrompt(instructions, prompt), images, model, cancellationToken);
    }

    internal async Task<ImageGenerationAgentResult> ExecuteAsync(
        string composedPrompt,
        IReadOnlyList<ImageGenerationInputImage> images,
        ResolvedAiModel model,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(composedPrompt))
            throw new ArgumentException("Prompt cannot be empty.", nameof(composedPrompt));

        if (images.Count > MaxInputImages)
            throw new InvalidOperationException($"At most {MaxInputImages} input images are supported.");

        ValidateModel(model);

        var result = images.Count > 0
            ? await backend.EditAsync(model.Identifier, images, composedPrompt, cancellationToken)
            : await backend.GenerateAsync(model.Identifier, composedPrompt, cancellationToken);

        // Token usage (when the gateway reports it) feeds the pricing engine; with none reported
        // billing falls to the feature's floor (per-image price) on the Financial page.
        return new ImageGenerationAgentResult(
            result.Images, null, result.InputTokens, result.CachedInputTokens, result.OutputTokens);
    }

    /// <summary>Only models registered with Kind = ImageGeneration can serve this feature.</summary>
    internal static void ValidateModel(ResolvedAiModel model)
    {
        if (model.Kind != AiModelKind.ImageGeneration)
            throw new InvalidOperationException(
                $"Model '{model.Identifier}' is not an image-generation model. Register it with the " +
                "image-generation kind on the AI Models page and assign it to the image-generation feature.");
    }

    /// <summary>The image API has no system role, so instructions are folded into the prompt.</summary>
    internal static string BuildPrompt(string instructions, string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Prompt cannot be empty.", nameof(prompt));

        return string.IsNullOrWhiteSpace(instructions)
            ? prompt.Trim()
            : $"{instructions.Trim()}{Environment.NewLine}{Environment.NewLine}---{Environment.NewLine}{Environment.NewLine}{prompt.Trim()}";
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
