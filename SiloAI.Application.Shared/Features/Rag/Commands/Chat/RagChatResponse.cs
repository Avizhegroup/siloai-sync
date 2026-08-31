namespace SiloAI.Application.Shared.Features;

public class RagChatResponse
{
    public string ResponseText { get; set; }
    public Guid ConversationId { get; set; }
    public List<RagChatCitationDto> Citations { get; set; } = new();
    public ChatTokenUsageDto? TokenUsage { get; set; }
    public decimal? PriceUsage   { get; set; }
}
