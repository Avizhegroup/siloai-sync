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
        // Name the file by what it really is: the endpoint rejects anything but jpg/png/webp.
        var mediaType = ImageFormat.Detect(image.Data)
            ?? throw new InvalidOperationException("The input image must be a PNG, JPEG or WebP file.");

        using var stream = new MemoryStream(image.Data, writable: false);

        var result = await CreateClient(model).GenerateImageEditAsync(
            image: stream,
            imageFilename: "image" + ImageFormat.Extension(mediaType),
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

        return new GeneratedImageDto
        {
            DataBase64 = Convert.ToBase64String(bytes),
            MediaType = ImageFormat.Detect(bytes) ?? "image/png"
        };
    }
}
