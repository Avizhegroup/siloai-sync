namespace SiloAI.UI.Pages;

public partial class Customers
{
    public bool IsLoading;
    public string _newName = string.Empty;
    public decimal _newCredit = 0;
    public int? _editingId;
    public string _editName = string.Empty;
    public decimal _editCredit;
    public bool _showApiKeys;
    public string _newKeyLabel = string.Empty;
    public DateTime _newKeyExpiresAt = DateTime.Today.AddYears(1);
    public string? _newKeyValue;
    public bool _isKeyLoading;

    public List<CustomerDto>? _customers;
    public List<ApiKeyDto>? _customerKeys;
    public CustomerDto? _selectedCustomer;


    [Inject] public AiApiClient ApiClient { get; set; }

    [CascadingParameter] public TelerikNotification Notification { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadCustomers();
    }

    private async Task LoadCustomers()
    {
        IsLoading = true;
       
        _customers = await ApiClient.GetFromJsonAsync<List<CustomerDto>>("admin/customers");
        
        IsLoading = false;     
    }

    private async Task CreateCustomer()
    {
        if (string.IsNullOrWhiteSpace(_newName))
        {
            Notification.Show("نام مشتری الزامی است.", "error");

            return;
        }

        IsLoading = true;
        
        var response = await ApiClient.PostAsJsonAsync("admin/customers", new CreateCustomerRequest
           {
              Name = _newName,
              RemainingCredit = _newCredit
           });

         response.EnsureSuccessStatusCode();

        _newName = string.Empty;

        _newCredit = 0;

        await LoadCustomers();
       
       IsLoading = false;
    }

    private void StartEdit(CustomerDto customer)
    {
        _editingId = customer.Id;

        _editName = customer.Name;

        _editCredit = Math.Round(customer.RemainingCredit, 2);
    }

    private void CancelEdit()
    {
        _editingId = null;
    }

    private async Task SaveEdit(int id)
    {
        IsLoading = true;
         
        var response = await ApiClient.PutAsJsonAsync($"admin/customers/{id}", new UpdateCustomerRequest
           {
               Name = _editName,
               RemainingCredit = _editCredit
           });

        response.EnsureSuccessStatusCode();

        _editingId = null;

        await LoadCustomers();
       
         IsLoading = false;
    }

    private async Task DeleteCustomer(int id)
    {
        IsLoading = true;
      
        var response = await ApiClient.DeleteAsync($"admin/customers/{id}");

        response.EnsureSuccessStatusCode();
       
        await LoadCustomers();
       
        IsLoading = false;
    }

    private async Task ShowApiKeys(CustomerDto customer)
    {
        _selectedCustomer = customer;

        _showApiKeys = true;

        _newKeyValue = null;

        await LoadCustomerApiKeys(customer.Id);
    }

    private async Task LoadCustomerApiKeys(int customerId)
    {
        _isKeyLoading = true;
     
       _customerKeys = await ApiClient.GetFromJsonAsync<List<ApiKeyDto>>($"admin/customers/{customerId}/api-keys");
       
       _isKeyLoading = false;
       
    }

    private async Task CreateApiKey()
    {
       if (string.IsNullOrWhiteSpace(_newKeyLabel))
        {
             Notification.Show("عنوان کلید الزامی است.", "error");

             return;
        }

       _isKeyLoading = true;

       _newKeyValue = null;
       
       var response = await ApiClient.PostAsJsonAsync("admin/api-keys", new CreateApiKeyRequest
            {
                Label = _newKeyLabel,
                ExpiresAt = _newKeyExpiresAt.ToUniversalTime(),
                CustomerId = _selectedCustomer!.Id
            });

       response.EnsureSuccessStatusCode();

       var created = await response.Content.ReadFromJsonAsync<ApiKeyDto>();

       _newKeyValue = created?.KeyValue;

       _newKeyLabel = string.Empty;

       _newKeyExpiresAt = DateTime.Today.AddYears(1);

       await LoadCustomerApiKeys(_selectedCustomer.Id);
     
      _isKeyLoading = false;
    }

    private async Task RevokeApiKey(int keyId)
    {
        _isKeyLoading = true;
      
        var response = await ApiClient.DeleteAsync($"admin/api-keys/{keyId}");

        response.EnsureSuccessStatusCode();

        await LoadCustomerApiKeys(_selectedCustomer!.Id);
      
       _isKeyLoading = false;
    }
}
