using System.Globalization;

namespace SiloAI.UI.Pages;

public partial class Index
{
    public const int MaxRecords = 5000;   

    public static readonly string[] Palette =
    [
        "#2563eb", "#7c3aed", "#059669", "#d97706", "#dc2626",
        "#0891b2", "#db2777", "#65a30d", "#4b5563", "#ea580c"
    ];

    public static readonly string[] PersianMonths =
    [
        "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
        "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
    ];

    public bool _isLoading = true;
    public bool _isTruncated;
    public bool _showUsd;
    public int _chartVersion;
    public string _rangeTitle = string.Empty;

    public decimal _totalToman;
    public decimal _totalUsd;
    public decimal _totalCreditUsd;
    public int _activeCustomers;
    public int _customerCount;

    public List<UsageRecordDto> _records = [];
    public List<CustomerDto> _customers = [];
    public Dictionary<int, decimal> _balances = [];
    public List<UsageSeries> _series = [];
    public List<CustomerUsageRow> _rows = [];

    public string ValueFormat => _showUsd ? "{0:N2}" : "{0:N0}";

    [Inject] public AiApiClient ApiClient { get; set; }
    [CascadingParameter] public TelerikNotification Notification { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    private async Task LoadData()
    {
        _isLoading = true;

        try
        {
            var usageTask = ApiClient.GetFromJsonAsync<List<UsageRecordDto>>(
                                $"admin/financial/usage?take={MaxRecords}");
            var accountsTask = ApiClient.GetFromJsonAsync<List<LedgerAccountDto>>(
                                "admin/financial/accounts");
            var customersTask = ApiClient.GetFromJsonAsync<List<CustomerDto>>(
                                "admin/customers");

            await Task.WhenAll(usageTask, accountsTask, customersTask);

            _records = usageTask.Result ?? [];
            _customers = customersTask.Result ?? [];
            _balances = (accountsTask.Result ?? [])
                .GroupBy(a => a.CustomerId)
                .ToDictionary(g => g.Key, g => g.Sum(a => a.BalanceToman));

            BuildChart();
        }
        catch (Exception ex)
        {
            Notification.Show($"خطا در بارگذاری آمار مصرف: {ex.Message}", "error");
            _series = [];
            _rows = [];
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void SetUnit(bool usd)
    {
        _showUsd = usd;
        BuildChart();
    }

    private void BuildChart()
    {
        var pc = new PersianCalendar();
        var today = DateTime.Today;

        var monthStart = today.AddDays(-(pc.GetDayOfMonth(today) - 1));

        _rangeTitle = $"{PersianMonths[pc.GetMonth(today) - 1]} {pc.GetYear(today)}";

        var days = Enumerable.Range(0, (today - monthStart).Days + 1)
                             .Select(i => monthStart.AddDays(i))
                             .ToList();

        _isTruncated = _records.Count >= MaxRecords && _records.Min(r => r.CreatedAt.ToLocalTime().Date) > monthStart;

        var inMonth = _records
            .Select(r => new
            {
                r.CustomerId,
                r.CustomerName,
                Day = r.CreatedAt.ToLocalTime().Date,
                Toman = r.ChargeToman,
                Usd = r.CostUsd
            })
            .Where(r => r.Day >= monthStart && r.Day <= today)
            .ToList();

        var byCustomer = inMonth.ToLookup(r => r.CustomerId);

        var names = _customers.ToDictionary(c => c.Id, c => c.Name);
        var credits = _customers.ToDictionary(c => c.Id, c => c.RemainingCredit);

        foreach (var g in inMonth.GroupBy(r => r.CustomerId))
            names.TryAdd(g.Key, g.First().CustomerName);

        var ordered = names.Keys
            .OrderByDescending(id => byCustomer[id].Sum(x => x.Toman))
            .ThenBy(id => names[id])
            .ToList();

        _totalToman = inMonth.Sum(x => x.Toman);
        _totalUsd = inMonth.Sum(x => x.Usd);
        _totalCreditUsd = credits.Values.Sum();
        _activeCustomers = byCustomer.Count;
        _customerCount = names.Count;

        _series = ordered
            .Select((id, i) =>
            {
                var perDay = byCustomer[id]
                    .GroupBy(x => x.Day)
                    .ToDictionary(d => d.Key,
                                  d => _showUsd ? d.Sum(x => x.Usd) : d.Sum(x => x.Toman));

                decimal running = 0;
                var points = new List<DayPoint>();

                foreach (var day in days)
                {
                    running += perDay.GetValueOrDefault(day);

                    points.Add(new DayPoint($"{pc.GetMonth(day):00}/{pc.GetDayOfMonth(day):00}",running));
                }

                return new UsageSeries(names[id], points, Palette[i % Palette.Length]);
            })
            .ToList();

        _rows = ordered
            .Select((id, i) =>
            {
                var toman = byCustomer[id].Sum(x => x.Toman);

                return new CustomerUsageRow(
                    names[id],
                    toman,
                    byCustomer[id].Sum(x => x.Usd),
                    credits.GetValueOrDefault(id),
                    _balances.GetValueOrDefault(id),
                    _totalToman > 0 ? Math.Round(toman / _totalToman * 100, 1) : 0,
                    Palette[i % Palette.Length]);
            })
            .ToList();

        _chartVersion++;
    }

    public record DayPoint(string Label, decimal Value);
    public record UsageSeries(string Name, List<DayPoint> Points, string Color);
    public record CustomerUsageRow(
        string Name,
        decimal Toman,
        decimal Usd,
        decimal CreditUsd,
        decimal BalanceToman,
        decimal Share,
        string Color);
}
