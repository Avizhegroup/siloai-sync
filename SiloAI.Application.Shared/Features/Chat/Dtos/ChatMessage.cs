namespace SiloAI.Application.Shared.Features;

public class ChatMessage
{
    public bool IsUser { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime Time { get; set; } = DateTime.Now;
    public List<RagChatCitationDto>? Citations { get; set; }
}
