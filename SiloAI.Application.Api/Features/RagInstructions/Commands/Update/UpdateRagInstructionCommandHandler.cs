namespace SiloAI.Application.Api.Features;

public class UpdateRagInstructionCommandHandler(AiApiContext context) : IRequestHandler<UpdateRagInstructionCommand, RagInstructionDto?>
{
    public async Task<RagInstructionDto?> Handle(UpdateRagInstructionCommand request, CancellationToken cancellationToken)
    {
        var instruction = await context.RagInstructions
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (instruction is null)
        {
            return null; 
        }

        if (request.IsSystematic)
        {
            var existingSystematic = await context.RagInstructions
                .AnyAsync(x => x.Id != request.Id && x.DocType == instruction.DocType && x.IsSystematic, cancellationToken);

            if (existingSystematic)
            {
                var docTypeDisplay = ((RagDocType)instruction.DocType).ToDisplay();

                throw new SiloValidationException(new List<ValidationResult>
                {
                    new ValidationResult($"برای نوع سند '{docTypeDisplay}' یک دستورالعمل سیستماتیک دیگر قبلاً ثبت شده است.")
                });
            }
        }

        instruction.Key = string.IsNullOrWhiteSpace(request.Key) ? null : request.Key.Trim();
        instruction.Category = request.Category;
        instruction.Tags = request.Tags;
        instruction.Content = request.Content;
        instruction.IsSystematic = request.IsSystematic;
        instruction.IsActive = request.IsActive;
        instruction.LastUpdateDateTime = DateTime.UtcNow;
        instruction.LastUpdateUserId = request.UpdaterUserId;

        await context.SaveChangesAsync(cancellationToken);

        return new RagInstructionDto
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
