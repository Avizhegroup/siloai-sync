using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using SiloAI.Shared;
using SiloAI.UI.Components;
using Telerik.Blazor;
using static Telerik.Blazor.ThemeConstants;

namespace SiloAI.UI.Pages;

public partial class Knowledge
{
    private List<RagDocumentDto>? _documents;
    private bool _isLoading;
    private bool _isUploading;

    private RagDocType _docType = RagDocType.GeneralChat;
    private string _key = string.Empty;
    private string _category = string.Empty;
    private string _tags = string.Empty;

    private IBrowserFile? _selectedFile;

    private RagDocumentDto? _rebuildTarget;
    private IBrowserFile? _rebuildFile;

    private Modal? ModalKnowledgeFiles;
    private int? _rebuildChunkCount;
    private const long MaxUploadSize = 25 * 1024 * 1024;
    private bool _showRebuildResult;
    private List<DocTypeItem> DocTypes { get; } =
[
    new()
    {
        Code = RagDocType.GeneralChat,
        Title = "گفتگوی عمومی"
    },
    new()
    {
        Code = RagDocType.Report,
        Title = "گزارش"
    },
    new()
    {
        Code = RagDocType.Image,
        Title = "تصویر"
    },
    new()
    {
        Code = RagDocType.PageAgent,
        Title = "عامل صفحه محور"
    }
];

    [Inject] public AiApiClient ApiClient { get; set; }

    [CascadingParameter]
    public TelerikNotification Notification { get; set; }
    [CascadingParameter] public DialogFactory Dialog { get; set; }


    protected override async Task OnInitializedAsync()
    {
        await LoadDocumentsAsync();
    }


    private async Task LoadDocumentsAsync()
    {
        _isLoading = true;

        try
        {
            _documents =
                await ApiClient.GetFromJsonAsync<List<RagDocumentDto>>(
                    "api/rag/documents");
        }
        catch (Exception ex)
        {
            Notification.Show(
                $"خطا در بارگذاری لیست: {ex.Message}",
                "error");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task OnOpenKnowledgeFilesModal(MouseEventArgs e)
    {
        await LoadDocumentsAsync();

        if (ModalKnowledgeFiles is not null)
        {
            await ModalKnowledgeFiles.Open(e);
        }
    }

    private void OnFileSelected(InputFileChangeEventArgs args)
    {
        _selectedFile = args.File;
    }


    private async Task OnRebuildFileSelected(InputFileChangeEventArgs args)
    {
        _rebuildFile = args.File;

        await RebuildAsync();
    }


    private void StartRebuild(RagDocumentDto document)
    {
        _rebuildTarget = document;
        _rebuildFile = null;
    }


    private async Task UploadAsync()
    {
        if (_selectedFile is null)
            return;

        _isUploading = true;

        try
        {
            using var content = new MultipartFormDataContent();

            using var stream =
                _selectedFile.OpenReadStream(MaxUploadSize);

            using var fileContent =
                new StreamContent(stream);

            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(
                    string.IsNullOrWhiteSpace(_selectedFile.ContentType)
                        ? "application/octet-stream"
                        : _selectedFile.ContentType);

            content.Add(
                fileContent,
                "file",
                _selectedFile.Name);

            content.Add(
                new StringContent(_docType.ToString()),
                "docType");

            if (!string.IsNullOrWhiteSpace(_key))
                content.Add(
                    new StringContent(_key.Trim()),
                    "key");

            if (!string.IsNullOrWhiteSpace(_category))
                content.Add(
                    new StringContent(_category),
                    "category");

            if (!string.IsNullOrWhiteSpace(_tags))
                content.Add(
                    new StringContent(_tags),
                    "tags");

            var response =
                await ApiClient.PostMultipartAsync(
                    "api/rag/documents",
                    content);

            response.EnsureSuccessStatusCode();

            var result =
                await response.Content
                    .ReadFromJsonAsync<RagUploadResponseDto>();

            if (result is { ProcessingStatus: "Failed" })
            {
                Notification.Show(
                    $"بارگذاری انجام شد اما پردازش با خطا مواجه شد: {result.ProcessingError}",
                    "warning");
            }
            else
            {
                Notification.Show(
                    $"بارگذاری انجام شد. تعداد قطعات: {result?.ChunkCount ?? 0}",
                    "success");
            }

            _selectedFile = null;
            _docType = RagDocType.GeneralChat;
            _key = string.Empty;
            _category = string.Empty;
            _tags = string.Empty;

            await LoadDocumentsAsync();
        }
        catch (Exception ex)
        {
            Notification.Show(
                $"خطا در بارگذاری: {ex.Message}",
                "error");
        }
        finally
        {
            _isUploading = false;
        }
    }


    private async Task DeleteAsync(Guid id)
    {

        var dialogResult = await Dialog.ConfirmAsync(
        "آیا از حذف این فایل اطمینان دارید؟",
        "توجه",
         "حذف",
        "انصراف");

        if (!dialogResult)
        {
            return;
        }

        _isUploading = true;

        try
        {
            var response =
                await ApiClient.DeleteAsync(
                    $"api/rag/documents/{id}");

            response.EnsureSuccessStatusCode();

            await LoadDocumentsAsync();
        }
        catch (Exception ex)
        {
            Notification.Show(
                $"خطا در حذف: {ex.Message}",
                "error");
        }
        finally
        {
            _isUploading = false;
        }
    }


    private async Task RebuildAsync()
    {
        if (_rebuildTarget is null || _rebuildFile is null)
            return;

        _isUploading = true;

            using var content = new MultipartFormDataContent();

            using var stream =
                _rebuildFile.OpenReadStream(MaxUploadSize);

            using var fileContent =
                new StreamContent(stream);

            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(
                    string.IsNullOrWhiteSpace(_rebuildFile.ContentType)
                        ? "application/octet-stream"
                        : _rebuildFile.ContentType);

            content.Add(
                fileContent,
                "file",
                _rebuildFile.Name);

            var response =
                await ApiClient.PostMultipartAsync(
                    $"api/rag/documents/{_rebuildTarget.Id}/rebuild",
                    content);

            response.EnsureSuccessStatusCode();

            var result =
                await response.Content
                    .ReadFromJsonAsync<RagUploadResponseDto>();

            if (result is { ProcessingStatus: "Failed" })
            {
                Notification.Show(
                    $"بازسازی با خطا مواجه شد: {result.ProcessingError}",
                    "warning");

                return;
            }

            await LoadDocumentsAsync();

            _rebuildChunkCount = result?.ChunkCount ?? 0;

            _rebuildTarget = null;
            _rebuildFile = null;


        _showRebuildResult = true;
        StateHasChanged();

        _isUploading = false;
        
    }


    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024)
            return $"{bytes} B";

        if (bytes < 1024 * 1024)
            return $"{bytes / 1024d:F1} KB";

        if (bytes < 1024L * 1024 * 1024)
            return $"{bytes / (1024d * 1024):F1} MB";

        return $"{bytes / (1024d * 1024 * 1024):F1} GB";
    }
}
