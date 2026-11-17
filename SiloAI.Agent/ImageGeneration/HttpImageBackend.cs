using Microsoft.Extensions.Options;
using SiloAI.Agent.Rag;
using SiloAI.Application.Shared.Features;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;

namespace SiloAI.Agent.ImageGeneration;

public sealed record ImageBackendResult(
    IReadOnlyList<GeneratedImageDto> Images, long InputTokens, long CachedInputTokens, long OutputTokens);

/// <summary>One call to the image model: text-to-image, or edit of one or more input images.</summary>
public interface IImageBackend
{
    Task<ImageBackendResult> GenerateAsync(string model, string prompt, CancellationToken cancellationToken);

    Task<ImageBackendResult> EditAsync(
        string model, IReadOnlyList<ImageGenerationInputImage> images, string prompt, CancellationToken cancellationToken);
}

/// <summary>
/// TEMPORARY: calls the gateway's OpenAI-style REST endpoints directly (<c>/images/generations</c>
/// and multipart <c>/images/edits</c>) because the OpenAI .NET SDK's image-edit call fails against
/// it. Swap back to the SDK <c>ImageClient</c> behind <see cref="IImageBackend"/> once that works.
/// </summary>
public sealed class HttpImageBackend(IOptions<OpenAIOptions> options, HttpClient? http = null) : IImageBackend
{
    private static readonly HttpClient SharedHttp = new() { Timeout = TimeSpan.FromMinutes(5) };

    private readonly HttpClient _http = http ?? SharedHttp;

    public async Task<ImageBackendResult> GenerateAsync(string model, string prompt, CancellationToken cancellationToken)
    {
        var body = new JsonObject
        {
            ["model"] = model,
            ["prompt"] = prompt,
            ["n"] = 1,
            ["size"] = "1024x1024",
            ["quality"] = "high"
        };

        using var request = CreateRequest("images/generations");
        request.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");

        return await SendAsync(request, cancellationToken);
    }

    public async Task<ImageBackendResult> EditAsync(
        string model, IReadOnlyList<ImageGenerationInputImage> images, string prompt, CancellationToken cancellationToken)
    {
        using var form = new MultipartFormDataContent
        {
            { new StringContent(model), "model" },
            { new StringContent(prompt), "prompt" }
        };

        var index = 0;
        foreach (var image in images)
        {
            // Name/type the file by what it really is: the endpoint accepts only jpg/png/webp.
            var mediaType = ImageFormat.Detect(image.Data)
                ?? throw new InvalidOperationException("Input images must be PNG, JPEG or WebP files.");

            var part = new ByteArrayContent(image.Data);
            part.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
            form.Add(part, "image", $"image{++index}{ImageFormat.Extension(mediaType)}");
        }

        using var request = CreateRequest("images/edits");
        request.Content = form;

        return await SendAsync(request, cancellationToken);
    }

    private HttpRequestMessage CreateRequest(string path)
    {
        var o = options.Value;

        if (string.IsNullOrWhiteSpace(o.ApiKey) || string.IsNullOrWhiteSpace(o.Endpoint))
            throw new InvalidOperationException("OpenAI:ApiKey and OpenAI:Endpoint must be configured.");

        var request = new HttpRequestMessage(HttpMethod.Post, o.Endpoint.TrimEnd('/') + "/" + path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", o.ApiKey);
        return request;
    }

    private async Task<ImageBackendResult> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var response = await _http.SendAsync(request, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"Image model call failed ({(int)response.StatusCode}): {Truncate(json, 500)}");

        var result = ParseResponse(json);

        // URL-style results are fetched here so callers always get bytes.
        var images = new List<GeneratedImageDto>();
        foreach (var item in result.Images)
        {
            if (item.DataBase64.Length > 0) { images.Add(item); continue; }

            var bytes = await _http.GetByteArrayAsync(item.MediaType, cancellationToken); // MediaType carries the URL here
            images.Add(ToDto(bytes));
        }

        return result with { Images = images };
    }

    /// <summary>Parses <c>{ "data": [{ "b64_json" | "url" }], "usage": {...} }</c>.</summary>
    internal static ImageBackendResult ParseResponse(string json)
    {
        var root = JsonNode.Parse(json) ?? throw new InvalidOperationException("Empty response from image model.");

        var images = new List<GeneratedImageDto>();

        foreach (var item in root["data"]?.AsArray() ?? [])
        {
            var b64 = item?["b64_json"]?.GetValue<string>();
            if (!string.IsNullOrEmpty(b64))
            {
                images.Add(ToDto(Convert.FromBase64String(b64)));
                continue;
            }

            var url = item?["url"]?.GetValue<string>();
            if (!string.IsNullOrEmpty(url))
                images.Add(new GeneratedImageDto { DataBase64 = string.Empty, MediaType = url }); // resolved in SendAsync
        }

        if (images.Count == 0)
            throw new InvalidOperationException("The image model returned no image: " + Truncate(json, 300));

        var usage = root["usage"];
        return new ImageBackendResult(
            images,
            usage?["input_tokens"]?.GetValue<long>() ?? 0,
            usage?["input_tokens_details"]?["cached_tokens"]?.GetValue<long>() ?? 0,
            usage?["output_tokens"]?.GetValue<long>() ?? 0);
    }

    private static GeneratedImageDto ToDto(byte[] bytes) => new()
    {
        DataBase64 = Convert.ToBase64String(bytes),
        MediaType = ImageFormat.Detect(bytes) ?? "image/png"
    };

    private static string Truncate(string s, int n) => s.Length <= n ? s : s[..n] + "…";
}
