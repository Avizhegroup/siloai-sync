namespace SiloAI.UI.Pages;

public partial class Financial
{
    private int _activeTab;

    private List<LedgerAccountDto>? _accounts;
    private List<LedgerTransactionDto>? _transactions;
    private List<UsageRecordDto>? _usageRecords;
    private List<PricingSettingDto>? _pricingSettings;
    private List<FxRateSettingDto>? _fxRates;
    private List<CustomerDto>? _customers;

    private static readonly UsageFeature[] _features = Enum.GetValues<UsageFeature>();

    private bool _isLoading;
    private bool _isSaving;

    // Top-up form
    private int _topUpCustomerId;
    private decimal _topUpAmount;
    private string _topUpReference = string.Empty;
    private string? _topUpDescription;

    // Pricing form
    private UsageFeature _pricingFeature = UsageFeature.SupportChat;
    private decimal _pricingMultiplier = 2m;
    private decimal _pricingFloorToman;
    private decimal _pricingFloorUsd;

    // FX form
    private decimal _fxTomanPerUsd;

    [Inject] public AiApiClient ApiClient { get; set; }
    [CascadingParameter] public TelerikNotification Notification { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadAll();
    }

    private async Task LoadAll()
    {
        _isLoading = true;
        try
        {
            var accountsTask = ApiClient.GetFromJsonAsync<List<LedgerAccountDto>>("admin/financial/accounts");
            var transactionsTask = ApiClient.GetFromJsonAsync<List<LedgerTransactionDto>>("admin/financial/transactions?take=100");
            var usageTask = ApiClient.GetFromJsonAsync<List<UsageRecordDto>>("admin/financial/usage?take=100");
            var pricingTask = ApiClient.GetFromJsonAsync<List<PricingSettingDto>>("admin/financial/pricing");
            var fxTask = ApiClient.GetFromJsonAsync<List<FxRateSettingDto>>("admin/financial/fx-rates");
            var customersTask = ApiClient.GetFromJsonAsync<List<CustomerDto>>("admin/customers");

            await Task.WhenAll(accountsTask, transactionsTask, usageTask, pricingTask, fxTask, customersTask);

            _accounts = accountsTask.Result;
            _transactions = transactionsTask.Result;
            _usageRecords = usageTask.Result;
            _pricingSettings = pricingTask.Result;
            _fxRates = fxTask.Result;
            _customers = customersTask.Result;
        }
        catch (Exception ex)
        {
            Notification.Show($"خطا در بارگذاری اطلاعات مالی: {ex.Message}", "error");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void OnTabChanged(int index)
    {
        _activeTab = index;
    }

    private async Task SubmitTopUp()
    {
        if (_topUpCustomerId <= 0)
        {
            Notification.Show("مشتری را انتخاب کنید.", "error");
            return;
        }
        if (_topUpAmount <= 0)
        {
            Notification.Show("مبلغ شارژ باید بزرگ‌تر از صفر باشد.", "error");
            return;
        }
        if (string.IsNullOrWhiteSpace(_topUpReference))
        {
            Notification.Show("مرجع شارژ (شناسه تراکنش درگاه) الزامی است.", "error");
            return;
        }

        _isSaving = true;
        try
        {
            var response = await ApiClient.PostAsJsonAsync("admin/financial/topup", new TopUpLedgerCommand
            {
                CustomerId = _topUpCustomerId,
                AmountToman = _topUpAmount,
                Reference = _topUpReference,
                Description = _topUpDescription
            });

            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                Notification.Show("این مرجع شارژ قبلاً ثبت شده است.", "error");
                return;
            }

            response.EnsureSuccessStatusCode();

            Notification.Show("شارژ با موفقیت ثبت شد.", "success");
            _topUpAmount = 0;
            _topUpReference = string.Empty;
            _topUpDescription = null;
            await LoadAll();
        }
        catch (Exception ex)
        {
            Notification.Show($"خطا در ثبت شارژ: {ex.Message}", "error");
        }
        finally
        {
            _isSaving = false;
        }
    }

    private async Task SubmitPricingSetting()
    {
        if (_pricingMultiplier <= 0)
        {
            Notification.Show("ضریب باید بزرگ‌تر از صفر باشد.", "error");
            return;
        }

        _isSaving = true;
        try
        {
            var response = await ApiClient.PostAsJsonAsync("admin/financial/pricing", new AddPricingSettingCommand
            {
                Feature = _pricingFeature,
                Multiplier = _pricingMultiplier,
                FloorToman = _pricingFloorToman,
                FloorUsd = _pricingFloorUsd
            });
            response.EnsureSuccessStatusCode();

            Notification.Show("تنظیم قیمت‌گذاری جدید ثبت شد.", "success");
            await LoadAll();
        }
        catch (Exception ex)
        {
            Notification.Show($"خطا در ثبت تنظیم قیمت‌گذاری: {ex.Message}", "error");
        }
        finally
        {
            _isSaving = false;
        }
    }

    private async Task SubmitFxRate()
    {
        if (_fxTomanPerUsd <= 0)
        {
            Notification.Show("نرخ ارز باید بزرگ‌تر از صفر باشد.", "error");
            return;
        }

        _isSaving = true;
        try
        {
            var response = await ApiClient.PostAsJsonAsync("admin/financial/fx-rates", new AddFxRateSettingCommand
            {
                TomanPerUsd = _fxTomanPerUsd
            });
            response.EnsureSuccessStatusCode();

            Notification.Show("نرخ ارز جدید ثبت شد.", "success");
            _fxTomanPerUsd = 0;
            await LoadAll();
        }
        catch (Exception ex)
        {
            Notification.Show($"خطا در ثبت نرخ ارز: {ex.Message}", "error");
        }
        finally
        {
            _isSaving = false;
        }
    }
}
