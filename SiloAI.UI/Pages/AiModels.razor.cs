namespace SiloAI.UI.Pages;

public partial class AiModels
{
    public bool IsLoading;
    public int _activeTab;
    public bool _isSaving;
    public bool _showForm;
    public bool _isAssignmentsLoading;
    public UpsertAiModelRequest _form = new();

    public List<AiModelDto>? _models;
    public List<AiModelDto>? _normalModels => _models?.Where(m => m.Kind == AiModelKind.Normal && m.IsActive).ToList();
    public List<CustomerDto>? _customers;
    public int? _assignmentCustomerId;
    public List<ModelAssignmentDto>? _assignments;

    [Inject] public AiApiClient ApiClient { get; set; }

    [CascadingParameter] public TelerikNotification Notification { get; set; }

    public readonly List<ModelKindOption> _modelKindOptions =
    [
        new()
    {
        Text = "مدل عمومی (چت/تصویر/فایل/صدا)",
        Value = AiModelKind.Normal
    },
    new()
    {
        Text = "مدل بازیابی (RAG / Embedding)",
        Value = AiModelKind.Rag
    }
    ];

    protected override async Task OnInitializedAsync()
    {
        await LoadModels();

        _customers = await ApiClient.GetFromJsonAsync<List<CustomerDto>>("admin/customers");

        await LoadAssignments();
    }

    private async Task LoadModels()
    {
        IsLoading = true;
      
        _models = await ApiClient.GetFromJsonAsync<List<AiModelDto>>("admin/ai-models");
       
        IsLoading = false;
    }

    private void OpenCreateForm()
    {
        _form = new UpsertAiModelRequest { Kind = AiModelKind.Normal, IsActive = true };

        _showForm = true;
    }

    private void OpenEditForm(AiModelDto model)
    {
        _form = new UpsertAiModelRequest
        {
            Id = model.Id,
            Name = model.Name,
            Identifier = model.Identifier,
            Kind = model.Kind,
            SupportsTextInput = model.SupportsTextInput,
            SupportsTextOutput = model.SupportsTextOutput,
            SupportsImageInput = model.SupportsImageInput,
            SupportsImageOutput = model.SupportsImageOutput,
            SupportsFileInput = model.SupportsFileInput,
            SupportsFileOutput = model.SupportsFileOutput,
            SupportsVoiceInput = model.SupportsVoiceInput,
            SupportsVoiceOutput = model.SupportsVoiceOutput,
            EmbeddingDimensions = model.EmbeddingDimensions,
            InputPricePerMillionTokens = model.InputPricePerMillionTokens,
            OutputPricePerMillionTokens = model.OutputPricePerMillionTokens,
            CachedInputPricePerMillionTokens = model.CachedInputPricePerMillionTokens,
            IsActive = model.IsActive
        };

        _showForm = true;
    }

    private void CloseForm() => _showForm = false;

    private async Task SaveModel()
    {
        if (string.IsNullOrWhiteSpace(_form.Name) || string.IsNullOrWhiteSpace(_form.Identifier))
        {
            Notification.Show("نام و شناسه مدل الزامی است.", "error");
            return;
        }

        if (_form.Kind == AiModelKind.Rag && (_form.EmbeddingDimensions is null || _form.EmbeddingDimensions <= 0))
        {
            Notification.Show("برای مدل RAG، ابعاد بردار الزامی است.", "error");
            return;
        }

        _isSaving = true;
       
            var response = await ApiClient.PostAsJsonAsync("admin/ai-models", _form);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                Notification.Show($"خطا در ذخیره مدل: {error}", "error");

                return;
            }

            _showForm = false;

            await LoadModels();

            Notification.Show("مدل با موفقیت ذخیره شد.", "success");
      
            _isSaving = false;
    }

    private async Task ToggleActive(AiModelDto model)
    {
            var response = await ApiClient.PostAsJsonAsync($"admin/ai-models/{model.Id}/active", !model.IsActive);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                Notification.Show($"خطا: {error}", "error");

                return;
            }

            await LoadModels();
    }

    private async Task SetDefaultRag(AiModelDto model)
    {
            var response = await ApiClient.PostAsJsonAsync($"admin/ai-models/{model.Id}/set-default-rag", new { });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                Notification.Show($"خطا: {error}", "error");

                return;
            }

            await LoadModels();

            Notification.Show($"«{model.Name}» به‌عنوان مدل پیش‌فرض RAG تنظیم شد.", "success");
    }

    private async Task LoadAssignments()
    {
        _isAssignmentsLoading = true;

            var query = _assignmentCustomerId is { } id ? $"?customerId={id}" : string.Empty;

            _assignments = await ApiClient.GetFromJsonAsync<List<ModelAssignmentDto>>($"admin/ai-models/assignments{query}");
       
            _isAssignmentsLoading = false;
    }

    private async Task AssignModelChanged(UsageFeature feature, object? value)
    {
        var raw = value?.ToString();

        if (string.IsNullOrWhiteSpace(raw) || !Guid.TryParse(raw, out var modelId))
        {
            return;
        }
            var response = await ApiClient.PostAsJsonAsync("admin/ai-models/assignments", new SetModelAssignmentRequest
            {
                CustomerId = _assignmentCustomerId,
                Feature = feature,
                AiModelId = modelId
            });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                Notification.Show($"خطا در تخصیص مدل: {error}", "error");
                return;
            }

            await LoadAssignments();

            Notification.Show("تخصیص مدل ذخیره شد.", "success");
    }

    private async Task ClearOverride(UsageFeature feature)
    {
        if (_assignmentCustomerId is not { } customerId)
        {
            return;
        }
      
            var response = await ApiClient.DeleteAsync($"admin/ai-models/assignments/{customerId}/{feature}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                Notification.Show($"خطا: {error}", "error");

                return;
            }

            await LoadAssignments();

            Notification.Show("بازگشت به پیش‌فرض انجام شد.", "success");
     
    }

    private static string CapabilitiesSummary(AiModelDto m)
    {
        var parts = new List<string>();

        if (m.SupportsTextInput || m.SupportsTextOutput) parts.Add($"متن({Dir(m.SupportsTextInput, m.SupportsTextOutput)})");

        if (m.SupportsImageInput || m.SupportsImageOutput) parts.Add($"تصویر({Dir(m.SupportsImageInput, m.SupportsImageOutput)})");

        if (m.SupportsFileInput || m.SupportsFileOutput) parts.Add($"فایل({Dir(m.SupportsFileInput, m.SupportsFileOutput)})");

        if (m.SupportsVoiceInput || m.SupportsVoiceOutput) parts.Add($"صدا({Dir(m.SupportsVoiceInput, m.SupportsVoiceOutput)})");

        return parts.Count > 0 ? string.Join(" | ", parts) : "بدون قابلیت تعریف‌شده";

        static string Dir(bool inp, bool outp) => (inp, outp) switch
        {
            (true, true) => "ورودی/خروجی",
            (true, false) => "ورودی",
            (false, true) => "خروجی",
            _ => ""
        };
    }
}
