using Microsoft.AspNetCore.Components.Forms;

namespace SiloAI.UI.Pages;
public partial class RagInstructions
{
    public bool IsLoading = true;
    public bool IsSaving;
    public Guid? EditingId;
    public List<RagInstructionDto>? Instructions;
    public RagInstructionDto Request;
    public const long MaxUploadSize = 5 * 1024 * 1024;

    [Inject] public AiApiClient ApiClient { get; set; }
    [CascadingParameter] public TelerikNotification Notification { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Request = NewModel();

        await LoadInstructionsAsync();
    }

    private async Task LoadInstructionsAsync()
    {
        IsLoading = true;
        try
        {
            Instructions = await ApiClient.GetFromJsonAsync<List<RagInstructionDto>>("api/rag/instructions");
        }
        catch (Exception ex)
        {
            Notification.Show($"خطا در بارگذاری لیست: {ex.Message}", "error");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task OnFileSelected(InputFileChangeEventArgs args)
    {
        try
        {
            using var stream = args.File.OpenReadStream(MaxUploadSize);
            using var reader = new StreamReader(stream);
            Request.Content = await reader.ReadToEndAsync();
        }
        catch (Exception ex)
        {
            Notification.Show($"خطا در خواندن فایل: {ex.Message}", "error");
        }
    }

    private void StartEdit(RagInstructionDto instruction)
    {
        EditingId = instruction.Id;

        Request = new RagInstructionDto
        {
            Id = instruction.Id,
            DocType = instruction.DocType,
            Key = instruction.Key ?? string.Empty,
            Category = instruction.Category ?? string.Empty,
            Tags = instruction.Tags ?? string.Empty,
            Content = instruction.Content,
            IsSystematic = instruction.IsSystematic,
            IsActive = instruction.IsActive,
            CreateDateTime = instruction.CreateDateTime,
            LastUpdateDateTime = instruction.LastUpdateDateTime
        };
    }

    private void ResetForm()
    {
        EditingId = null;
        Request = NewModel();
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Request.Content)) return;

        IsSaving = true;
        try
        {
            HttpResponseMessage response;

            if (EditingId is null)
            {
                response = await ApiClient.PostAsJsonAsync("api/rag/instructions", new CreateRagInstructionCommand
                {
                    DocType = Request.DocType,
                    Key = string.IsNullOrWhiteSpace(Request.Key) ? null : Request.Key.Trim(),
                    Category = Request.Category,
                    Tags = Request.Tags,
                    Content = Request.Content,
                    IsSystematic = Request.IsSystematic
                });
            }
            else
            {
                response = await ApiClient.PutAsJsonAsync($"api/rag/instructions/{EditingId}", new UpdateRagInstructionCommand
                {
                    DocType = Request.DocType,
                    Key = string.IsNullOrWhiteSpace(Request.Key) ? null : Request.Key.Trim(),
                    Category = Request.Category,
                    Tags = Request.Tags,
                    Content = Request.Content,
                    IsSystematic = Request.IsSystematic,
                    IsActive = Request.IsActive
                });
            }

            response.EnsureSuccessStatusCode();

            Notification.Show(EditingId is null ? "دستورالعمل با موفقیت ثبت شد." : "دستورالعمل با موفقیت به‌روزرسانی شد.", "success");

            ResetForm();
            await LoadInstructionsAsync();
        }
        catch (Exception ex)
        {
            Notification.Show($"خطا در ذخیره‌سازی: {ex.Message}", "error");
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task DeleteAsync(Guid id)
    {
        IsSaving = true;
        try
        {
            var response = await ApiClient.DeleteAsync($"api/rag/instructions/{id}");
            response.EnsureSuccessStatusCode();

            if (EditingId == id)
                ResetForm();

            await LoadInstructionsAsync();
        }
        catch (Exception ex)
        {
            Notification.Show($"خطا در حذف: {ex.Message}", "error");
        }
        finally
        {
            IsSaving = false;
        }
    }

    private RagInstructionDto NewModel()
    => new()
    {
        DocType = RagDocType.GeneralChat,
        Key = string.Empty,
        Category = string.Empty,
        Tags = string.Empty,
        Content = string.Empty,
        IsSystematic = false,
        IsActive = true
    };
}
