using System.ClientModel;
using System.Text.Json;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Agents.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using SiloAI.Agent.Rag;
using SiloAI.Application.Shared.Contracts.AiModels;
using SiloAI.Application.Shared.Features;
using SiloAI.Domains;
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

    /// <summary>
    /// Identifier of the model actually in use for the current <see cref="writer"/> agent, set by
    /// <see cref="InitChatAgentWithInstructions"/>. Used by <see cref="AiCostCalculator"/> so the
    /// legacy informational PriceUsage figure prices the model that was really called, instead of
    /// a fixed config value.
    /// </summary>
    private string? activeModelIdentifier;

    public async Task InitChatAgent(ResolvedAiModel model, List<RagDocType>? promptKeys = null)
    {
        var instructions = await LoadInstructionsAsync(promptKeys);

        InitChatAgentWithInstructions(instructions, model);
    }

    /// <summary>
    /// Builds (or reuses a cached) underlying agent.
    /// </summary>
    /// <param name="model">
    /// The model to call, resolved by the caller via <c>IAiModelResolver</c> for the relevant
    /// customer + feature. ChatAgentService is chat-only: the model must support plain text
    /// input and text output. Use <c>SiloAI.Agent.Tasks.AgentFileTaskService</c> instead for
    /// file-input tasks (OCR, document extraction, etc.).
    /// </param>
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
        ResolvedAiModel model,
        bool includeAutoRagContext = true,
        RagDocType? ragDocType = null,
        string? ragKey = null)
    {
        if (!model.SupportsTextInput || !model.SupportsTextOutput)
            throw new InvalidOperationException(
                $"Model '{model.Identifier}' does not support text input/output and cannot be " +
                "used with ChatAgentService. Use SiloAI.Agent.Tasks.AgentFileTaskService for " +
                "file-input tasks instead.");

        activeModelIdentifier = model.Identifier;

        var cacheKey = ChatAgentCache.BuildKey(
            model.Identifier, instructions, includeAutoRagContext, ragDocType, ragKey);

        writer = agentCache.GetOrCreate(cacheKey, () =>
        {
            var chatClient = new ChatClient(model.Identifier,
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

    public async Task<ChatAgentResponse> SendWithAgentSessionAsync(
        string? sessionJson, CopilotMessageRequest query, CancellationToken cancellationToken = default)
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

        var priceUsage = await costCalculator.CalculateAsync(tokenUsage, activeModelIdentifier, cancellationToken);

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

    private async Task<string> LoadInstructionsAsync(List<RagDocType>? promptKeys = null)
    {
        if (promptKeys is null || promptKeys.Count == 0)
        {
            return string.Empty;
        }

        var contents = new List<string>();

        foreach (var docType in promptKeys.Distinct())
        {
            var docTypeValue = (int)docType;

            var cachedInstructions = await agentCache.GetOrCreateInstructionsAsync(docTypeValue, async () =>
                (IReadOnlyList<CachedRagInstruction>)await context.RagInstructions
                    .Where(p => p.DocType == docTypeValue && p.IsActive)
                    .AsNoTracking()
                    .Select(p => new CachedRagInstruction(p.Content, p.IsSystematic, p.CreateDateTime))
                    .ToListAsync());

            contents.AddRange(cachedInstructions.Select(i => i.Content));
        }

        return string.Join(Environment.NewLine, contents);
    }
}
