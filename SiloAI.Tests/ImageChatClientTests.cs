using Microsoft.Extensions.AI;
using SiloAI.Agent.ImageGeneration;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Xunit;

namespace SiloAI.Tests;

public class ImageChatClientTests
{
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 1, 2, 3];

    [Fact]
    public void Request_carries_instructions_text_images_and_image_modality()
    {
        var messages = new[]
        {
            new ChatMessage(ChatRole.User, new List<AIContent>
            {
                new TextContent("make it blue"),
                new DataContent(Png, "image/png")
            })
        };

        var body = OpenAiCompatibleImageChatClient.BuildRequestBody(
            messages, new ChatOptions { Instructions = "be an artist" }, "vendor/image-model");

        Assert.Equal("vendor/image-model", body["model"]!.GetValue<string>());
        Assert.Equal(new[] { "image", "text" }, body["modalities"]!.AsArray().Select(n => n!.GetValue<string>()).ToArray());

        var wire = body["messages"]!.AsArray();
        Assert.Equal(2, wire.Count);
        Assert.Equal("system", wire[0]!["role"]!.GetValue<string>());
        Assert.Equal("be an artist", wire[0]!["content"]!.GetValue<string>());

        var parts = wire[1]!["content"]!.AsArray();
        Assert.Equal("text", parts[0]!["type"]!.GetValue<string>());
        Assert.Equal("make it blue", parts[0]!["text"]!.GetValue<string>());
        Assert.Equal("image_url", parts[1]!["type"]!.GetValue<string>());
        Assert.StartsWith("data:image/png;base64,", parts[1]!["image_url"]!["url"]!.GetValue<string>());
    }

    [Fact]
    public void Request_without_instructions_has_no_system_message()
    {
        var body = OpenAiCompatibleImageChatClient.BuildRequestBody(
            [new ChatMessage(ChatRole.User, "hi")], new ChatOptions(), "m");

        Assert.Single(body["messages"]!.AsArray());
    }

    [Fact]
    public void Response_images_and_usage_are_parsed()
    {
        var json = $$$$"""
        base64,{{{{Convert.ToBase64String(Png)}}}}"}}]}}],
         "usage":{"prompt_tokens":120,"completion_tokens":30,"total_tokens":150,"prompt_tokens_details":{"cached_tokens":20}}}
        """;

        var response = OpenAiCompatibleImageChatClient.ParseResponse(json, "m");

        var image = Assert.Single(response.Messages.SelectMany(m => m.Contents).OfType<DataContent>());
        Assert.Equal("image/png", image.MediaType);
        Assert.Equal(Png, image.Data.ToArray());
        Assert.Equal("done", response.Text);
        Assert.Equal(120, response.Usage!.InputTokenCount);
        Assert.Equal(30, response.Usage.OutputTokenCount);
        Assert.Equal(20, response.Usage.CachedInputTokenCount);
    }

    [Fact]
    public void Response_http_image_url_becomes_UriContent()
    {
        var json = """{"choices":[{"message":{"content":"","images":[{"image_url":{"url":"https://cdn.example/x.png"}}]}}]}""";

        var response = OpenAiCompatibleImageChatClient.ParseResponse(json, "m");

        Assert.Single(response.Messages.SelectMany(m => m.Contents).OfType<UriContent>());
    }

    [Fact]
    public void Response_without_choices_throws()
        => Assert.Throws<InvalidOperationException>(() => OpenAiCompatibleImageChatClient.ParseResponse("{}", "m"));

    [Fact]
    public async Task GetResponseAsync_posts_to_chat_completions_with_bearer_key()
    {
        var handler = new StubHandler(HttpStatusCode.OK,
            $$$"""{"choices":[{"message":{"images":[{"image_url":{"url":"data:image/png;base64,{{{Convert.ToBase64String(Png)}}}"}}]}}]}""");

        using var client = new OpenAiCompatibleImageChatClient(
            new HttpClient(handler), new Uri("https://gw.example/api/v1/"), "secret", "img-model");

        var response = await client.GetResponseAsync([new ChatMessage(ChatRole.User, "draw")]);

        Assert.Equal("https://gw.example/api/v1/chat/completions", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal("Bearer", handler.LastRequest.Headers.Authorization!.Scheme);
        Assert.Equal("secret", handler.LastRequest.Headers.Authorization.Parameter);
        Assert.Single(response.Messages.SelectMany(m => m.Contents).OfType<DataContent>());
        Assert.Equal("img-model", JsonNode.Parse(handler.LastBody!)!["model"]!.GetValue<string>());
    }

    [Fact]
    public async Task GetResponseAsync_surfaces_http_errors()
    {
        using var client = new OpenAiCompatibleImageChatClient(
            new HttpClient(new StubHandler(HttpStatusCode.BadRequest, "{\"error\":\"nope\"}")),
            new Uri("https://gw.example/v1"), "k", "m");

        var ex = await Assert.ThrowsAsync<HttpRequestException>(
            () => client.GetResponseAsync([new ChatMessage(ChatRole.User, "x")]));

        Assert.Contains("400", ex.Message);
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
