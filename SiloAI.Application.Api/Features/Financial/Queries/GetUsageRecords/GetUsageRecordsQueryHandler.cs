namespace SiloAI.Application.Api.Features;

public class GetUsageRecordsQueryHandler(AiApiContext context)
    : IRequestHandler<GetUsageRecordsQuery, List<UsageRecordDto>>
{
    public async Task<List<UsageRecordDto>> Handle(GetUsageRecordsQuery request, CancellationToken cancellationToken)
    {
        var take = request.Take <= 0 ? 100 : Math.Clamp(request.Take, 1, 500);

        var query = context.UsageRecords
            .AsNoTracking()
            .AsQueryable();

        if (request.CustomerId.HasValue)
            query = query.Where(u => u.CustomerId == request.CustomerId.Value);

        return await query
            .OrderByDescending(u => u.CreatedAt)
            .Take(take)
            .Select(u => new UsageRecordDto
            {
                Id = u.Id,
                CustomerId = u.CustomerId,
                CustomerName = u.Customer.Name,
                Feature = u.Feature,
                Model = u.Model,
                InputTokens = u.InputTokens,
                CachedTokens = u.CachedTokens,
                OutputTokens = u.OutputTokens,
                CostUsd = u.CostUsd,
                FxRateUsed = u.FxRateUsed,
                MultiplierUsed = u.MultiplierUsed,
                FloorTomanUsed = u.FloorTomanUsed,
                ChargeToman = u.ChargeToman,
                ConversationId = u.ConversationId,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
