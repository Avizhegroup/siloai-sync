namespace SiloAI.Application.Shared.Features;

public class ChatMessage
{
    public bool IsUser { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime Time { get; set; } = DateTime.Now;
    public List<RagChatCitationDto>? Citations { get; set; }

    /// <summary>What the answer to this message cost (assistant messages only).</summary>
    public RequestPriceDto? Price { get; set; }

    /// <summary>Names of the files the user attached to this message (shown as chips in the chat).</summary>
    public List<string>? AttachmentNames { get; set; }
}
