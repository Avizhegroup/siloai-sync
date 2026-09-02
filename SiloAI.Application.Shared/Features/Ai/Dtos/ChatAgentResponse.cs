namespace SiloAI.Application.Shared.Features;

public class ChatAgentResponse
{
    public CopilotMessageDto Response { get; set; } = new();

    public string SerializedSession { get; set; } = string.Empty;

    public ChatTokenUsageDto TokenUsage { get; set; } = new();

    public decimal PriceUsage { get; set; }
}