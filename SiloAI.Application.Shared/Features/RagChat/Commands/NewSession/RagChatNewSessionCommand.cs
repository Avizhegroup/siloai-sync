namespace SiloAI.Application.Shared.Features;

public class RagChatNewSessionCommand : IRequest<RagChatResponse>
{
    public string? RagModel { get; set; }
    public string OwnerId { get; set; }
}
