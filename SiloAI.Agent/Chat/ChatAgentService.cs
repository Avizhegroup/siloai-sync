using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using SiloAI.Agent.Rag;
using SiloAI.Application.Shared.Features;
using SiloAI.Domains;
using System.ClientModel;
using System.Text.Json;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace SiloAI.Agent.Chat;
public class ChatAgentService(
    IOptions<OpenAIOptions> options,
    RagContextProviderFactory ragContextProviderFactory,
    AiCostCalculator costCalculator,
    AiApiContext context,
    ChatAgentCache agentCache)
{
    private AIAgent writer;

    public async Task InitChatAgent(List<RagDocType>? promptKeys = null, string? modelName = null)
    {
        var instructions = await LoadInstructionsAsync(promptKeys);

        InitChatAgentWithInstructions(instructions, modelName);
    }

    /// <summary>
    /// Builds (or reuses a cached) underlying agent.
    /// </summary>
    /// <param name="includeAutoRagContext">
    /// When true (default), the agent automatically retrieves and injects RAG context before
    /// every model call via <see cref="RagContextProviderFactory"/>. Set this to false when the
    /// caller already performs its own, correctly-filtered retrieval and augments the message
    /// itself (e.g. <c>RagChatSendHandler</c>) — leaving this on in that case causes a second,
    /// unfiltered retrieval pass and duplicate chunk content in the prompt.
    /// </param>
    /// <param name="ragDocType">DocType filter passed through to the auto context provider, if enabled.</param>
    /// <param name="ragKey">Key filter passed through to the auto context provider, if enabled.</param>
    public void InitChatAgentWithInstructions(
        string instructions,
        string? modelName = null,
        bool includeAutoRagContext = true,
        string? ragDocType = null,
        string? ragKey = null)
    {
        var model = modelName ?? options.Value.MainModel;

        var cacheKey = ChatAgentCache.BuildKey(
            model, instructions, includeAutoRagContext, ragDocType, ragKey);

        writer = agentCache.GetOrCreate(cacheKey, () =>
        {
            var chatClient = new ChatClient(model,
                new ApiKeyCredential(options.Value.ApiKey),
                new OpenAIClientOptions { Endpoint = new Uri(options.Value.Endpoint) })
                .AsIChatClient();

            var contextProviders = includeAutoRagContext
                ? new[] { ragContextProviderFactory.Create(docType: ragDocType, key: ragKey) }
                : Array.Empty<AIContextProvider>();

            return new ChatClientAgent(chatClient, new ChatClientAgentOptions
            {
                ChatOptions = new()
                {
                    Instructions = instructions,
                },
                AIContextProviders = contextProviders
            });
        });
    }

    public async Task<CopilotMessageDto> SendRequestAndGetResponse(CopilotMessageRequest query)
    {
        var response = await writer.RunAsync(query.Text);

        return new()
        {
            ResponseText = response?.ToString()
        };
    }

    public async Task<ChatAgentResponse> SendWithAgentSessionAsync(string? sessionJson,CopilotMessageRequest query)
    {
        AgentSession session;

        if (sessionJson.HasValue())
        {
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(sessionJson);

            session = await writer.DeserializeSessionAsync(jsonElement);
        }
        else
        {
            session = await writer.CreateSessionAsync();
        }

        var result = await writer.RunAsync(query.Text, session);

        string responseText = result?.ToString();

        var tokenUsage = new ChatTokenUsageDto
        {
            InputTokenCount = result?.Usage?.InputTokenCount ?? 0,
            OutputTokenCount = result?.Usage?.OutputTokenCount ?? 0,
            CachedInputTokenCount = result?.Usage?.CachedInputTokenCount ?? 0,
            TotalTokenCount = result?.Usage?.TotalTokenCount ?? 0
        };

        var priceUsage = costCalculator.Calculate(tokenUsage);

        var serializedElement = await writer.SerializeSessionAsync(session);

        string serializedSession = serializedElement.GetRawText();

        return new ChatAgentResponse
        {
            Response = new CopilotMessageDto
            {
                ResponseText = responseText
            },
            SerializedSession = serializedSession,
            TokenUsage = tokenUsage,
            PriceUsage = priceUsage
        };
    }

    public async Task<AgentSession> CreateNewSessionAsync()
    {
        return await writer.CreateSessionAsync();
    }

    public async Task<string> SerializeSessionAsync(AgentSession session)
    {
        var serializedElement = await writer.SerializeSessionAsync(session);
        return serializedElement.GetRawText();
    }

    public async Task<string> SendImageAndGetTextAsync(byte[] imageData
        , string imageMediaType
        , RagDocType promptKey)
    {
        if (imageData is null || imageData.Length == 0)
        {
            throw new ArgumentException("Image data cannot be null or empty.", nameof(imageData));
        }

        DataContent? imageContent = new(imageData, imageMediaType);

        List<AIContent>? contents = new()
        {
            imageContent
        };

        string prompt = await LoadInstructionsAsync(new()
        {
            promptKey
        });

        contents.Insert(0, new TextContent(prompt));

        var message = new ChatMessage(ChatRole.User, contents);

        var response = await writer.RunAsync([message]);

        return response?.ToString() ?? string.Empty;
    }

    public async Task<string> SendImageAndGetTextAsync(Stream imageStream
        , string imageMediaType
        , RagDocType promptKey)
    {
        if (imageStream is null)
        {
            throw new ArgumentNullException(nameof(imageStream));
        }

        using var memoryStream = new MemoryStream();

        await imageStream.CopyToAsync(memoryStream);

        var imageData = memoryStream.ToArray();

        return await SendImageAndGetTextAsync(imageData, imageMediaType, promptKey);
    }

    private async Task<string> LoadInstructionsAsync(List<RagDocType>? promptKeys = null)
    {
        var instructions = context.RagInstructions.Where(p => promptKeys.Contains((RagDocType)p.DocType));

        return string.Join(Environment.NewLine, instructions.Select(p=>p.Content));
    }
}
