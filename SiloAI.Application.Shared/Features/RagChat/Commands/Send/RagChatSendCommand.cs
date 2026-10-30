namespace SiloAI.Application.Shared.Features;

public class RagChatSendCommand : IRequest<RagChatResponse>
{
    public Guid? ConversationId { get; set; }
    public string Message { get; set; }
    public int TopK { get; set; } = 5;
    public bool IsMainChat { get; set; }
    public RagDocType DocType { get; set; } = RagDocType.GeneralChat;
    public string? Key { get; set; }

    /// <summary>
    /// When false, the conversation runs on the DocType's instructions alone: no knowledge-base
    /// retrieval, no search tool, no systematic message template, no citations. Only consulted
    /// when this call creates the conversation (no <see cref="ConversationId"/>); an existing
    /// conversation keeps the mode it was created with.
    /// </summary>
    public bool UseRag { get; set; } = true;

    public string AugmentedMessageTemplate { get; set; }
    public string Username { get; set; }
    public string OwnerId { get; set; }
    public int? CustomerId { get; set; }
}
