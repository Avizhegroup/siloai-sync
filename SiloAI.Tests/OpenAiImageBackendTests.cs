using Microsoft.Extensions.Options;
using SiloAI.Agent.ImageGeneration;
using SiloAI.Agent.Rag;
using SiloAI.Application.Shared.Features;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Xunit;

namespace SiloAI.Tests;

public class ImageFormatTests
{
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0];

    [Fact]
    public void PNG_is_detected() => Assert.Equal("image/png", ImageFormat.Detect(Png));

    [Fact]
    public void JPEG_is_detected() => Assert.Equal("image/jpeg", ImageFormat.Detect([0xFF, 0xD8, 0xFF, 0xE0]));

    [Fact]
    public void WebP_is_detected() => Assert.Equal("image/webp", ImageFormat.Detect("RIFF\0\0\0\0WEBPVP8 "u8));

    [Fact]
    public void GIF_is_not_accepted() => Assert.Null(ImageFormat.Detect("GIF89a\0\0\0\0\0\0"u8));

    [Fact]
    public void Non_images_and_empty_input_are_not_accepted()
    {
        Assert.Null(ImageFormat.Detect("hello world, not an image"u8));
        Assert.Null(ImageFormat.Detect([]));
    }

    [Theory]
    [InlineData("image/jpeg", ".jpg")]
    [InlineData("image/webp", ".webp")]
    [InlineData("image/png", ".png")]
    public void Extension_matches_media_type(string mediaType, string ext)
        => Assert.Equal(ext, ImageFormat.Extension(mediaType));
}

public class HttpImageBackendTests
{
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3];
    private static readonly string PngJson = $$"""{"data":[{"b64_json":"{{Convert.ToBase64String(Png)}}"}]}""";

    private static HttpImageBackend Create(StubHandler handler) => new(
        Options.Create(new OpenAIOptions { ApiKey = "secret", Endpoint = "https://gw.example/api/v1/" }),
        new HttpClient(handler));

    [Fact]
    public async Task Generate_posts_json_to_images_generations_with_bearer_key()
    {
        var handler = new StubHandler(HttpStatusCode.OK, PngJson);

        var result = await Create(handler).GenerateAsync("openai/gpt-image-2", "A cute baby sea otter", default);

        Assert.Equal("https://gw.example/api/v1/images/generations", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal("Bearer", handler.LastRequest.Headers.Authorization!.Scheme);
        Assert.Equal("secret", handler.LastRequest.Headers.Authorization.Parameter);

        var body = JsonNode.Parse(handler.LastBody!)!;
        Assert.Equal("openai/gpt-image-2", body["model"]!.GetValue<string>());
        Assert.Equal("A cute baby sea otter", body["prompt"]!.GetValue<string>());
        Assert.Equal(1, body["n"]!.GetValue<int>());
        Assert.Equal("1024x1024", body["size"]!.GetValue<string>());
        Assert.Equal("high", body["quality"]!.GetValue<string>());

        var image = Assert.Single(result.Images);
        Assert.Equal("image/png", image.MediaType);
        Assert.Equal(Convert.ToBase64String(Png), image.DataBase64);
    }

    [Fact]
    public async Task Edit_posts_multipart_with_one_image_part_per_input()
    {
        var handler = new StubHandler(HttpStatusCode.OK, PngJson);
        ImageGenerationInputImage Input() => new() { Data = Png, MediaType = "image/png" };

        await Create(handler).EditAsync("openai/gpt-image-2", [Input(), Input(), Input()], "make them happy", default);

        Assert.Equal("https://gw.example/api/v1/images/edits", handler.LastRequest!.RequestUri!.ToString());
        Assert.StartsWith("multipart/form-data", handler.LastRequest.Content!.Headers.ContentType!.MediaType);

        var body = handler.LastBody!;
        Assert.Equal(3, System.Text.RegularExpressions.Regex.Matches(body, "name=image;").Count);
        Assert.Contains("name=model", body);
        Assert.Contains("openai/gpt-image-2", body);
        Assert.Contains("name=prompt", body);
        Assert.Contains("make them happy", body);
        Assert.Contains("filename=image1.png", body);
        Assert.Contains("filename=image3.png", body);
        Assert.Contains("image/png", body);
    }

    [Fact]
    public async Task Edit_rejects_a_non_jpg_png_webp_input_without_calling_the_endpoint()
    {
        var handler = new StubHandler(HttpStatusCode.OK, PngJson);

        await Assert.ThrowsAsync<InvalidOperationException>(() => Create(handler).EditAsync(
            "m", [new ImageGenerationInputImage { Data = "GIF89a....."u8.ToArray(), MediaType = "image/gif" }], "p", default));

        Assert.Null(handler.LastRequest);
    }

    [Fact]
    public void Response_usage_is_parsed()
    {
        var json = $$"""{"data":[{"b64_json":"{{Convert.ToBase64String(Png)}}"}],"usage":{"input_tokens":120,"output_tokens":30,"input_tokens_details":{"cached_tokens":20}}}""";

        var result = HttpImageBackend.ParseResponse(json);

        Assert.Equal((120L, 20L, 30L), (result.InputTokens, result.CachedInputTokens, result.OutputTokens));
    }

    [Fact]
    public void Response_without_images_throws()
        => Assert.Throws<InvalidOperationException>(() => HttpImageBackend.ParseResponse("""{"data":[]}"""));

    [Fact]
    public async Task Http_errors_surface_status_and_body()
    {
        var handler = new StubHandler(HttpStatusCode.BadRequest, "files should be in jpg, png, webp format.");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => Create(handler).GenerateAsync("m", "p", default));

        Assert.Contains("400", ex.Message);
        Assert.Contains("files should be in jpg", ex.Message);
    }

    [Fact]
    public async Task Missing_configuration_fails_fast()
    {
        var backend = new HttpImageBackend(Options.Create(new OpenAIOptions()), new HttpClient(new StubHandler(HttpStatusCode.OK, "{}")));

        await Assert.ThrowsAsync<InvalidOperationException>(() => backend.GenerateAsync("m", "p", default));
    }

    private sealed class StubHandler(HttpStatusCode status, string body) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }
        public string? LastBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            LastRequest = request;
            LastBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(ct);
            return new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
        }
    }
}
