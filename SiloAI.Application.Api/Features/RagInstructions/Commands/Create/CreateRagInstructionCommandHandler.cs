using System.Net;
using SiloAI.Agent.Chat;

namespace SiloAI.Application.Api.Features;

public class CreateRagInstructionCommandHandler(AiApiContext context, ChatAgentCache agentCache) : IRequestHandler<CreateRagInstructionCommand, RagInstructionDto>
{
    public async Task<RagInstructionDto> Handle(CreateRagInstructionCommand request, CancellationToken cancellationToken)
    {
        if (request.IsSystematic)
        {
            var existingSystematic = await context.RagInstructions
                .AnyAsync(x => x.DocType == (int)request.DocType && x.IsSystematic, cancellationToken);

            if (existingSystematic)
            {
                var docTypeDisplay = request.DocType.Value.ToDisplay();
                throw new SiloValidationException(new List<ValidationResult>
                {
                    new ValidationResult($"برای نوع سند '{docTypeDisplay}' یک دستورالعمل سیستماتیک دیگر قبلاً ثبت شده است.")
                });
            }
        }

        var now = DateTime.Now;

        var rawContent = request.Content.HasValue() ? WebUtility.HtmlDecode(request.Content) : request.Content;

        RagInstruction instruction = new()
        {
            Id = Guid.NewGuid(),
            DocType = (int)request.DocType,
            Key = string.IsNullOrWhiteSpace(request.Key) ? null : request.Key.Trim(),
            Category = request.Category,
            Tags = request.Tags,
            Content = rawContent,
            IsSystematic = request.IsSystematic,
            IsActive = true,
            CreateDateTime = now,
            CreatorUserId = request.CreatorUserId,
            LastUpdateDateTime = now,
            LastUpdateUserId = request.CreatorUserId
        };

        context.RagInstructions.Add(instruction);

        await context.SaveChangesAsync(cancellationToken);

        agentCache.InvalidateInstructions(instruction.DocType);

        return new()
        {
            Id = instruction.Id,
            DocType = (RagDocType)instruction.DocType,
            Key = instruction.Key,
            Category = instruction.Category,
            Tags = instruction.Tags,
            Content = instruction.Content,
            IsSystematic = instruction.IsSystematic,
            IsActive = instruction.IsActive,
            CreateDateTime = instruction.CreateDateTime,
            LastUpdateDateTime = instruction.LastUpdateDateTime
        };
    }
}
