namespace SiloAI.Application.Shared.Features;

public class NewChatSessionCommand : IRequest<NewSessionResponse>
{
    public RagDocType DocType { get; set; }
    public int? CustomerId { get; set; }
}
