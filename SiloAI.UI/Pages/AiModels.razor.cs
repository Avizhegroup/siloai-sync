namespace SiloAI.UI.Pages;

public partial class AiModels
{
    private int _activeTab;
    private bool _isLoading;
    private bool _isSaving;

    private List<AiModelDto>? _models;
    private List<AiModelDto>? _normalModels => _models?.Where(m => m.Kind == AiModelKind.Normal && m.IsActive).ToList();

    private bool _showForm;
    private UpsertAiModelRequest _form = new();

    private List<CustomerDto>? _customers;
    private int? _assignmentCustomerId;
    private List<ModelAssignmentDto>? _assignments;
    private bool _isAssignmentsLoading;

    [Inject] public AiApiClient ApiClient { get; set; }
    [CascadingParameter] public TelerikNotification Notification { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadModels();
        _customers = await ApiClient.GetFromJsonAsync<List<CustomerDto>>("admin/customers");
        await LoadAssignments();
    }

    private async Task LoadModels()
    {
        _isLoading = true;
        try
        {
            _models = await ApiClient.GetFromJsonAsync<List<AiModelDto>>("admin/ai-models");
        }
        catch (Exception ex)
        {
            Notification.Show($"خطا در بارگذاری مدل‌ها: {ex.Message}", "error");
        }
        finally
        {
            _isLoading = false;
        }
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
        try
        {
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
        }
        catch (Exception ex)
        {
            Notification.Show($"خطا در ذخیره مدل: {ex.Message}", "error");
        }
        finally
        {
            _isSaving = false;
        }
    }

    private async Task ToggleActive(AiModelDto model)
    {
        try
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
        catch (Exception ex)
        {
            Notification.Show($"خطا: {ex.Message}", "error");
        }
    }

    private async Task SetDefaultRag(AiModelDto model)
    {
        try
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
        catch (Exception ex)
        {
            Notification.Show($"خطا: {ex.Message}", "error");
        }
    }

    private async Task LoadAssignments()
    {
        _isAssignmentsLoading = true;
        try
        {
            var query = _assignmentCustomerId is { } id ? $"?customerId={id}" : string.Empty;
            _assignments = await ApiClient.GetFromJsonAsync<List<ModelAssignmentDto>>($"admin/ai-models/assignments{query}");
        }
        catch (Exception ex)
        {
            Notification.Show($"خطا در بارگذاری تخصیص‌ها: {ex.Message}", "error");
        }
        finally
        {
            _isAssignmentsLoading = false;
        }
    }

    private async Task AssignModelChanged(UsageFeature feature, object? value)
    {
        var raw = value?.ToString();
        if (string.IsNullOrWhiteSpace(raw) || !Guid.TryParse(raw, out var modelId))
            return;

        try
        {
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
        catch (Exception ex)
        {
            Notification.Show($"خطا در تخصیص مدل: {ex.Message}", "error");
        }
    }

    private async Task ClearOverride(UsageFeature feature)
    {
        if (_assignmentCustomerId is not { } customerId)
            return;

        try
        {
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
        catch (Exception ex)
        {
            Notification.Show($"خطا: {ex.Message}", "error");
        }
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
