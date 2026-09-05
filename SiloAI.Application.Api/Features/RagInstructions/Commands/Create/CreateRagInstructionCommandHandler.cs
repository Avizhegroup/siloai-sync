namespace SiloAI.Application.Api.Features;

public class CreateRagInstructionCommandHandler(AiApiContext context) : IRequestHandler<CreateRagInstructionCommand, RagInstructionDto>
{
    public async Task<RagInstructionDto> Handle(CreateRagInstructionCommand request, CancellationToken cancellationToken)
    {
        var docType = (request.DocType ?? RagDocType.GeneralChat).ToString();

        if (request.IsSystematic)
        {
            var existingSystematic = await context.RagInstructions
                .AnyAsync(x => x.DocType == docType && x.IsSystematic, cancellationToken);

            if (existingSystematic)
            {
                var docTypeDisplay = request.DocType.Value.ToDisplay();
                throw new SiloValidationException(new List<ValidationResult>
                {
                    new ValidationResult($"برای نوع سند '{docTypeDisplay}' یک دستورالعمل سیستماتیک دیگر قبلاً ثبت شده است.")
                });
            }
        }

        var now = DateTime.UtcNow;

        var instruction = new RagInstruction
        {
            Id = Guid.NewGuid(),
            DocType = docType,
            Key = string.IsNullOrWhiteSpace(request.Key) ? null : request.Key.Trim(),
            Category = request.Category,
            Tags = request.Tags,
            Content = request.Content,
            IsSystematic = request.IsSystematic,
            IsActive = true,
            CreateDateTime = now,
            CreatorUserId = request.CreatorUserId,
            LastUpdateDateTime = now,
            LastUpdateUserId = request.CreatorUserId
        };

        context.RagInstructions.Add(instruction);
        await context.SaveChangesAsync(cancellationToken);

        return new RagInstructionDto
        {
            Id = instruction.Id,
            DocType = Enum.TryParse<RagDocType>(instruction.DocType, out var dt) ? dt : RagDocType.GeneralChat,
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
