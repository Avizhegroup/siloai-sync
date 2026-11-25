namespace SiloAI.UI.Pages;

public partial class Dashboard
{
    private List<ApiKeyDto>? _recentKeys;
    private List<NegativeCreditCustomerDto>? _negativeCustomers;
    private int _activeKeys;
    private int _totalKeys;
    private int _revokedKeys;
    private bool _isLoading = true;
    private string? _errorMessage;

    [Inject] public AiApiClient ApiClient { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadStats();
    }

    private async Task LoadStats()
    {
        _isLoading = true;

        try
        {
            var keys = await ApiClient.GetFromJsonAsync<List<ApiKeyDto>>("admin/api-keys");

            if (keys is not null)
            {
                _totalKeys = keys.Count;
                _activeKeys = keys.Count(k => !k.IsRevoked && k.ExpiresAt > DateTime.Now);
                _revokedKeys = keys.Count(k => k.IsRevoked);
                _recentKeys = keys.Take(5).ToList();
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Could not load stats: {ex.Message}";
            _recentKeys = [];
        }

        // Customers allowed to go below zero, so they can be collected. Loaded separately: a failure
        // here must not hide the key statistics (and vice versa).
        try
        {
            _negativeCustomers = await ApiClient.GetFromJsonAsync<List<NegativeCreditCustomerDto>>("admin/financial/negative-balances") ?? [];
        }
        catch (Exception ex)
        {
            _errorMessage = $"{_errorMessage} Could not load negative-credit customers: {ex.Message}".Trim();
            _negativeCustomers = [];
        }
        finally
        {
            _isLoading = false;
        }
    }
}
