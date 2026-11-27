using Microsoft.AspNetCore.Http;
using SiloAI.Application.Shared;
using SiloAI.Application.Shared.Features;

namespace SiloAI.Application.Api.Features;

/// <summary>
/// multipart form of POST api/rag/chat/send-with-files: the same fields as <c>send</c> plus optional
/// files. Only <see cref="Message"/> is required. Validated and converted by its handler.
/// </summary>
public class RagChatSendFormCommand : IRequest<RagChatResponse>
{
    public Guid? ConversationId { get; set; }
    public string? Message { get; set; }
    public int TopK { get; set; } = 5;
    public bool IsMainChat { get; set; }
    public RagDocType DocType { get; set; } = RagDocType.GeneralChat;
    public string? Key { get; set; }
    public bool UseRag { get; set; } = true;
    public List<IFormFile> Files { get; set; } = [];
}

public class RagChatSendFormCommandHandler(IMediator mediator)
    : IRequestHandler<RagChatSendFormCommand, RagChatResponse>
{
    public async Task<RagChatResponse> Handle(RagChatSendFormCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            throw new BadRequestException("Message is required.");

        var attachments = await RagChatAttachmentReader.ReadAsync(request.Files, cancellationToken);

        // The caller (customer id, owner, username) is stamped onto the inner command by the pipeline behavior.
        return await mediator.Send(ToSendCommand(request, attachments), cancellationToken);
    }

    /// <summary>Same fields as the JSON <c>send</c> endpoint, plus the validated attachments.</summary>
    public static RagChatSendCommand ToSendCommand(RagChatSendFormCommand request, List<RagChatAttachment> attachments) => new()
    {
        ConversationId = request.ConversationId,
        Message = request.Message!.Trim(),
        TopK = request.TopK,
        IsMainChat = request.IsMainChat,
        DocType = request.DocType,
        Key = request.Key,
        UseRag = request.UseRag,
        Attachments = attachments
    };
}
