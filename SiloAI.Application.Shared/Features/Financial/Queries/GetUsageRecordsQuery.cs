namespace SiloAI.Application.Shared.Features;

public class GetUsageRecordsQuery : IRequest<List<UsageRecordDto>>
{
    public int? CustomerId { get; set; }
    public int Take { get; set; } = 100;
}
