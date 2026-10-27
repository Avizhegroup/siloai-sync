namespace SiloAI.Application.Shared.Features;

public class UsageSeries
{
    public string Name { get; set; } = string.Empty;
    public List<DayPoint> Points { get; set; } = [];
    public string Color { get; set; } = string.Empty;
}
