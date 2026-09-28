namespace SiloAI.Application.Api.Features;

public class GetModelAssignmentsQueryHandler(AiApiContext context) : IRequestHandler<GetModelAssignmentsQuery, List<ModelAssignmentDto>>
{
    private static readonly UsageFeature[] AllFeatures =
        [UsageFeature.SupportChat, UsageFeature.Report, UsageFeature.PageAgent, UsageFeature.Ocr];

    public async Task<List<ModelAssignmentDto>> Handle(GetModelAssignmentsQuery request, CancellationToken cancellationToken)
    {
        // Fetch the customer's own rows (if any) and the global-default rows in one query, then
        // resolve each feature in memory — cheaper than one round trip per feature.
        var relevant = await context.CustomerModelAssignments
            .AsNoTracking()
            .Where(a => a.CustomerId == null || a.CustomerId == request.CustomerId)
            .Select(a => new { a.CustomerId, a.Feature, a.AiModelId, ModelName = a.AiModel.Name })
            .ToListAsync(cancellationToken);

        var byFeature = relevant.ToLookup(a => a.Feature);

        return AllFeatures.Select(feature =>
        {
            var row = byFeature[feature];

            var overrideRow = request.CustomerId is not null
                ? row.FirstOrDefault(r => r.CustomerId == request.CustomerId)
                : null;

            var defaultRow = row.FirstOrDefault(r => r.CustomerId == null);

            var chosen = overrideRow ?? defaultRow;

            return new ModelAssignmentDto
            {
                Feature = feature,
                AiModelId = chosen?.AiModelId,
                AiModelName = chosen?.ModelName,
                IsOverride = overrideRow is not null
            };
        }).ToList();
    }
}
