namespace SiloAI.Application.Shared.Features;

public class RagChatNewSessionCommand : IRequest<RagChatResponse>
{
    public RagDocType DocType { get; set; } = RagDocType.GeneralChat;
    public string OwnerId { get; set; }
    public int? CustomerId { get; set; }
}
