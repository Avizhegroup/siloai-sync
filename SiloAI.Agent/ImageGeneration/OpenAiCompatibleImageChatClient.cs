using Microsoft.Extensions.AI;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SiloAI.Agent.ImageGeneration;

/// <summary>
/// An <see cref="IChatClient"/> for OpenAI-compatible gateways that return generated images from
/// <c>/chat/completions</c> (request <c>modalities: ["image","text"]</c>; response
/// <c>choices[0].message.images[].image_url.url</c> as a data: or http(s) URL). The stock OpenAI
/// .NET chat client drops that non-standard field, so this client reads the raw JSON itself and
/// surfaces images as <see cref="DataContent"/>/<see cref="UriContent"/> in the ChatResponse,
/// which lets a regular MAF <c>ChatClientAgent</c> sit on top of it (instructions, middleware,
/// usage reporting all work as for any other IChatClient).
/// </summary>
public sealed class OpenAiCompatibleImageChatClient(
    HttpClient http, Uri endpoint, string apiKey, string modelId) : IChatClient
{
    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        var body = BuildRequestBody(messages, options, modelId);

        var url = new Uri(endpoint.AbsoluteUri.TrimEnd('/') + "/chat/completions");
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        using var response = await http.SendAsync(request, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"Image model call failed ({(int)response.StatusCode}): {Truncate(json, 500)}");

        return ParseResponse(json, modelId);
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Image generation is not incremental; surface the finished result as one update.
        var result = await GetResponseAsync(messages, options, cancellationToken);
        yield return new ChatResponseUpdate(ChatRole.Assistant, result.Messages.SelectMany(m => m.Contents).ToList());
    }

    public object? GetService(Type serviceType, object? serviceKey = null) =>
        serviceKey is null && serviceType.IsInstanceOfType(this) ? this : null;

    public void Dispose() { }

    // ---- wire format (internal for unit tests) ------------------------------------------------

    internal static JsonObject BuildRequestBody(IEnumerable<ChatMessage> messages, ChatOptions? options, string modelId)
    {
        var wire = new JsonArray();

        if (!string.IsNullOrWhiteSpace(options?.Instructions))
            wire.Add(new JsonObject { ["role"] = "system", ["content"] = options!.Instructions });

        foreach (var message in messages)
        {
            var role = message.Role == ChatRole.System ? "system"
                     : message.Role == ChatRole.Assistant ? "assistant" : "user";

            var parts = new JsonArray();
            foreach (var content in message.Contents)
            {
                switch (content)
                {
                    case TextContent t when !string.IsNullOrEmpty(t.Text):
                        parts.Add(new JsonObject { ["type"] = "text", ["text"] = t.Text });
                        break;
                    case DataContent d when d.MediaType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == true:
                        parts.Add(new JsonObject
                        {
                            ["type"] = "image_url",
                            ["image_url"] = new JsonObject { ["url"] = d.Uri.ToString() }
                        });
                        break;
                    case UriContent u when u.MediaType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == true:
                        parts.Add(new JsonObject
                        {
                            ["type"] = "image_url",
                            ["image_url"] = new JsonObject { ["url"] = u.Uri.ToString() }
                        });
                        break;
                }
            }

            if (parts.Count == 0) continue;

            // System messages are plain strings on the wire.
            wire.Add(role == "system"
                ? new JsonObject { ["role"] = role, ["content"] = string.Concat(message.Contents.OfType<TextContent>().Select(t => t.Text)) }
                : new JsonObject { ["role"] = role, ["content"] = parts });
        }

        return new JsonObject
        {
            ["model"] = modelId,
            ["modalities"] = new JsonArray("image", "text"),
            ["messages"] = wire
        };
    }

    internal static ChatResponse ParseResponse(string json, string modelId)
    {
        var root = JsonNode.Parse(json) ?? throw new InvalidOperationException("Empty response from image model.");
        var message = root["choices"]?[0]?["message"]
            ?? throw new InvalidOperationException("Image model response has no choices[0].message.");

        var contents = new List<AIContent>();

        var text = message["content"] switch
        {
            JsonValue v when v.TryGetValue<string>(out var s) => s,
            JsonArray a => string.Concat(a.OfType<JsonObject>()
                .Where(p => p["type"]?.GetValue<string>() == "text").Select(p => p["text"]?.GetValue<string>())),
            _ => null
        };
        if (!string.IsNullOrWhiteSpace(text)) contents.Add(new TextContent(text));

        void AddImage(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            if (url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                var comma = url.IndexOf(',');
                var header = url[5..comma];                       // e.g. image/png;base64
                var mediaType = header.Split(';')[0];
                if (header.Contains("base64", StringComparison.OrdinalIgnoreCase))
                    contents.Add(new DataContent(Convert.FromBase64String(url[(comma + 1)..]), mediaType));
            }
            else
            {
                contents.Add(new UriContent(new Uri(url), "image/*"));
            }
        }

        if (message["images"] is JsonArray images)
            foreach (var img in images)
                AddImage(img?["image_url"]?["url"]?.GetValue<string>());

        if (message["content"] is JsonArray parts)
            foreach (var p in parts.OfType<JsonObject>().Where(p => p["type"]?.GetValue<string>() == "image_url"))
                AddImage(p["image_url"]?["url"]?.GetValue<string>());

        var result = new ChatResponse(new ChatMessage(ChatRole.Assistant, contents)) { ModelId = modelId };

        if (root["usage"] is JsonObject u)
        {
            result.Usage = new UsageDetails
            {
                InputTokenCount = u["prompt_tokens"]?.GetValue<long>(),
                OutputTokenCount = u["completion_tokens"]?.GetValue<long>(),
                TotalTokenCount = u["total_tokens"]?.GetValue<long>(),
                CachedInputTokenCount = u["prompt_tokens_details"]?["cached_tokens"]?.GetValue<long>()
            };
        }

        return result;
    }

    private static string Truncate(string s, int n) => s.Length <= n ? s : s[..n] + "…";
}
