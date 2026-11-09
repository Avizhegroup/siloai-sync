namespace SiloAI.Application.Shared.Features;

public class RagChatNewSessionCommand : IRequest<RagChatResponse>
{
    public RagDocType DocType { get; set; } = RagDocType.GeneralChat;

    /// <summary>
    /// When false, the new conversation runs on the DocType's instructions alone (no knowledge-base
    /// retrieval or search tool). The mode is fixed for the life of the conversation.
    /// </summary>
    public bool UseRag { get; set; } = true;

    public string OwnerId { get; set; }
    public int? CustomerId { get; set; }
}
