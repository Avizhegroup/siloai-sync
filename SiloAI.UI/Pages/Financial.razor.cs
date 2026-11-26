namespace SiloAI.UI.Pages;

public partial class Financial
{
    public bool IsLoading;
    public int _activeTab;
    public bool _isSaving;

    // Top-up form
    public int _topUpCustomerId;
    public decimal _topUpAmount;
    public string _topUpReference = string.Empty;
    public string? _topUpDescription;

    public List<LedgerAccountDto>? _accounts;
    public List<LedgerTransactionDto>? _transactions;
    public List<UsageRecordDto>? _usageRecords;
    public List<FxRateSettingDto>? _fxRates;
    public List<CustomerDto>? _customers;

    // FX form (the dollar rate is one global setting)
    public decimal _fxTomanPerUsd;

    public static readonly UsageFeature[] _features = Enum.GetValues<UsageFeature>();

    [Inject] public AiApiClient ApiClient { get; set; }

    [CascadingParameter] public TelerikNotification Notification { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadAll();
    }

    private async Task LoadAll()
    {
        IsLoading = true;
        
        var accountsTask = ApiClient.GetFromJsonAsync<List<LedgerAccountDto>>("admin/financial/accounts");

        var transactionsTask = ApiClient.GetFromJsonAsync<List<LedgerTransactionDto>>("admin/financial/transactions?take=100");

        var usageTask = ApiClient.GetFromJsonAsync<List<UsageRecordDto>>("admin/financial/usage?take=100");

        var fxTask = ApiClient.GetFromJsonAsync<List<FxRateSettingDto>>("admin/financial/fx-rates");

        var customersTask = ApiClient.GetFromJsonAsync<List<CustomerDto>>("admin/customers");

        await Task.WhenAll(accountsTask, transactionsTask, usageTask, fxTask, customersTask);

        _accounts = accountsTask.Result;

        _transactions = transactionsTask.Result;

        _usageRecords = usageTask.Result;

        _fxRates = fxTask.Result;

        _customers = customersTask.Result;
     
       IsLoading = false;    
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
      
         _isSaving = false;
        
    }

    private async Task SubmitFxRate()
    {
        if (_fxTomanPerUsd <= 0)
        {
            Notification.Show("نرخ ارز باید بزرگ‌تر از صفر باشد.", "error");

            return;
        }

        _isSaving = true;
        
         var response = await ApiClient.PostAsJsonAsync("admin/financial/fx-rates", new AddFxRateSettingCommand
            {
                TomanPerUsd = _fxTomanPerUsd
            });

         response.EnsureSuccessStatusCode();

         Notification.Show("نرخ ارز جدید ثبت شد.", "success");

        _fxTomanPerUsd = 0;

         await LoadAll();
      
         _isSaving = false;
    }
}
