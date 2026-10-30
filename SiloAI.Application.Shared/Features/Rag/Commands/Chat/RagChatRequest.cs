namespace SiloAI.Application.Shared.Features;

public class RagChatRequest
{
    public Guid? ConversationId { get; set; }
    public string Message { get; set; }
    public int TopK { get; set; } = 5;
    public bool IsMainChat { get; set; } = false;
    public RagDocType DocType { get; set; } = RagDocType.GeneralChat;
    public string? Key { get; set; }

    /// <summary>
    /// Set to false to chat using only the DocType's instructions (no RAG). Only takes effect
    /// when this request starts a new conversation; an existing conversation keeps its mode.
    /// </summary>
    public bool UseRag { get; set; } = true;
}
