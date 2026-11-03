using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Images;
using SiloAI.Agent.Rag;
using SiloAI.Application.Shared.Features;
using System.ClientModel;

namespace SiloAI.Agent.ImageGeneration;

/// <summary>One call to the image model: text-to-image, or edit of one input image.</summary>
public interface IImageBackend
{
    Task<GeneratedImageDto> GenerateAsync(string model, string prompt, CancellationToken cancellationToken);

    Task<GeneratedImageDto> EditAsync(
        string model, ImageGenerationInputImage image, string prompt, CancellationToken cancellationToken);
}

/// <summary>
/// <see cref="IImageBackend"/> on the official OpenAI .NET SDK <see cref="ImageClient"/>, pointed at
/// the configured OpenAI-compatible endpoint (OpenAI:Endpoint / OpenAI:ApiKey).
/// </summary>
public sealed class OpenAiImageBackend(IOptions<OpenAIOptions> options) : IImageBackend
{
    public async Task<GeneratedImageDto> GenerateAsync(string model, string prompt, CancellationToken cancellationToken)
    {
        var result = await CreateClient(model).GenerateImageAsync(prompt, cancellationToken: cancellationToken);

        return ToDto(result.Value);
    }

    public async Task<GeneratedImageDto> EditAsync(
        string model, ImageGenerationInputImage image, string prompt, CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream(image.Data, writable: false);

        var result = await CreateClient(model).GenerateImageEditAsync(
            image: stream,
            imageFilename: "image" + ExtensionFor(image.MediaType),
            prompt: prompt,
            cancellationToken: cancellationToken);

        return ToDto(result.Value);
    }

    private ImageClient CreateClient(string model)
    {
        var o = options.Value;

        if (string.IsNullOrWhiteSpace(o.ApiKey))
            throw new InvalidOperationException("OpenAI:ApiKey is not configured.");

        var clientOptions = new OpenAIClientOptions();
        if (!string.IsNullOrWhiteSpace(o.Endpoint))
            clientOptions.Endpoint = new Uri(o.Endpoint);

        return new ImageClient(model, new ApiKeyCredential(o.ApiKey), clientOptions);
    }

    private static GeneratedImageDto ToDto(GeneratedImage image)
    {
        if (image.ImageBytes is null)
            throw new InvalidOperationException(
                "The image model returned a URL instead of image bytes, which is not supported.");

        var bytes = image.ImageBytes.ToArray();

        return new GeneratedImageDto { DataBase64 = Convert.ToBase64String(bytes), MediaType = DetectMediaType(bytes) };
    }

    internal static string DetectMediaType(byte[] b) =>
        b.Length > 3 && b[0] == 0xFF && b[1] == 0xD8 ? "image/jpeg"
        : b.Length > 11 && b[0] == 'R' && b[1] == 'I' && b[2] == 'F' && b[3] == 'F' && b[8] == 'W' ? "image/webp"
        : "image/png";

    internal static string ExtensionFor(string mediaType) => mediaType switch
    {
        "image/jpeg" => ".jpg",
        "image/webp" => ".webp",
        "image/gif" => ".gif",
        _ => ".png"
    };
}
