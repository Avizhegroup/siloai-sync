using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

namespace SiloAI.UI.Pages;

public partial class Chat
{
    public readonly List<ChatMessage> _messages = new();
    public string _input = string.Empty;
    public Guid? _conversationId;
    public bool _isSending;
    public int _topK = 5;
    public RagDocType _docType = RagDocType.GeneralChat;
    public string _key = string.Empty;

    /// <summary>A file chosen for the next message (read into memory right away so it survives until Send).</summary>
    public sealed record DraftAttachment(
        string Name, string ContentType, long Size, byte[] Data, RagChatAttachmentKind Kind, string? Preview);

    public readonly List<DraftAttachment> _attachments = [];

    public static int MaxFiles => RagChatAttachmentRules.MaxFiles;
    public static string AcceptAttribute => RagChatAttachmentRules.AcceptAttribute;
    public static string AllowedExtensionsText => string.Join("  ", RagChatAttachmentRules.AllExtensions);
    public static long MaxFileMegabytes => RagChatAttachmentRules.MaxFileBytes / 1024 / 1024;

    [Inject] public AiApiClient ApiClient { get; set; }

    [CascadingParameter] public TelerikNotification Notification { get; set; }

    private void StartNewChat()
    {
        if (_isSending) return;

        _messages.Clear();

        _conversationId = null;

        _input = string.Empty;

        _attachments.Clear();
    }

    private async Task OnInputKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
        {
            await SendAsync();
        }
    }

    private async Task OnFilesSelected(InputFileChangeEventArgs args)
    {
        foreach (var file in args.GetMultipleFiles(100))
        {
            if (_attachments.Count >= RagChatAttachmentRules.MaxFiles)
            {
                Notification?.Show($"حداکثر {RagChatAttachmentRules.MaxFiles} فایل برای هر پیام مجاز است.", "warning");
                break;
            }

            var kind = RagChatAttachmentRules.Classify(file.Name);

            if (kind is null)
            {
                Notification?.Show($"فرمت «{file.Name}» پشتیبانی نمی‌شود. فرمت‌های مجاز: {AllowedExtensionsText}", "warning");
                continue;
            }

            if (file.Size == 0 || file.Size > RagChatAttachmentRules.MaxFileBytes)
            {
                Notification?.Show($"حجم «{file.Name}» باید بین ۱ بایت و {MaxFileMegabytes} مگابایت باشد.", "warning");
                continue;
            }

            if (_attachments.Sum(a => a.Size) + file.Size > RagChatAttachmentRules.MaxTotalBytes)
            {
                Notification?.Show($"مجموع حجم فایل‌ها نباید بیش از {RagChatAttachmentRules.MaxTotalBytes / 1024 / 1024} مگابایت باشد.", "warning");
                break;
            }

            using var stream = file.OpenReadStream(RagChatAttachmentRules.MaxFileBytes);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var data = ms.ToArray();

            var preview = kind == RagChatAttachmentKind.Image
                ? $"data:{file.ContentType};base64,{Convert.ToBase64String(data)}"
                : null;

            _attachments.Add(new DraftAttachment(file.Name, file.ContentType, file.Size, data, kind.Value, preview));
        }
    }

    private void RemoveAttachment(DraftAttachment attachment) => _attachments.Remove(attachment);

    private static string AttachmentIcon(RagChatAttachmentKind kind) => kind switch
    {
        RagChatAttachmentKind.Image => "bi-file-earmark-image",
        RagChatAttachmentKind.Pdf => "bi-file-earmark-pdf",
        _ => "bi-file-earmark-text"
    };

    private static string FormatBytes(long bytes) => bytes switch
    {
        >= 1024 * 1024 => $"{bytes / 1024d / 1024d:0.#} MB",
        >= 1024 => $"{bytes / 1024d:0.#} KB",
        _ => $"{bytes} B"
    };

    private async Task SendAsync()
    {
        if (_isSending) return;

        var text = (_input ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(text)) return;

        _isSending = true;

        var sentAttachments = _attachments.ToList();

        var userMessage = new ChatMessage
        {
            IsUser = true,
            Text = text,
            Time = DateTime.Now,
            AttachmentNames = sentAttachments.Count > 0 ? sentAttachments.Select(a => a.Name).ToList() : null
        };

        _messages.Add(userMessage);

        _input = string.Empty;

        var succeeded = false;

        try
        {
            using var response = sentAttachments.Count == 0
                ? await ApiClient.PostAsJsonAsync("api/rag/chat/send", new RagChatRequest
                {
                    Message = text,
                    ConversationId = _conversationId,
                    TopK = _topK <= 0 ? 5 : _topK,
                    DocType = _docType,
                    Key = string.IsNullOrWhiteSpace(_key) ? null : _key.Trim()
                })
                : await ApiClient.PostMultipartAsync("api/rag/chat/send-with-files", BuildForm(text, sentAttachments));

            if (!response.IsSuccessStatusCode)
            {
                Notification?.Show(await ReadErrorAsync(response), "error");
                return;
            }

            var result = await response.Content.ReadFromJsonAsync<RagChatResponse>();

            if (result is null)
            {
                Notification?.Show("پاسخی از سرور دریافت نشد.", "warning");
                return;
            }

            _conversationId = result.ConversationId;

            _messages.Add(new ChatMessage
            {
                IsUser = false,
                Text = string.IsNullOrWhiteSpace(result.ResponseText) ? "—" : result.ResponseText,
                Time = DateTime.Now,
                Citations = result.Citations,
                Price = result.Price
            });

            succeeded = true;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            Notification?.Show("ارتباط با سرور برقرار نشد یا زمان درخواست به پایان رسید.", "error");
        }
        finally
        {
            if (succeeded)
            {
                _attachments.Clear();
            }
            else
            {
                // Nothing was answered: give the text back and keep the files so the user can retry.
                _messages.Remove(userMessage);
                _input = text;
            }

            _isSending = false;
        }
    }

    private MultipartFormDataContent BuildForm(string text, List<DraftAttachment> files)
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent(text), "message" },
            { new StringContent(((int)_docType).ToString()), "docType" },
            { new StringContent((_topK <= 0 ? 5 : _topK).ToString()), "topK" }
        };

        if (_conversationId is { } id)
            content.Add(new StringContent(id.ToString()), "conversationId");

        if (!string.IsNullOrWhiteSpace(_key))
            content.Add(new StringContent(_key.Trim()), "key");

        foreach (var file in files)
        {
            var part = new ByteArrayContent(file.Data);
            part.Headers.ContentType = new MediaTypeHeaderValue(
                string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);
            content.Add(part, "files", file.Name);
        }

        return content;
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.PaymentRequired)
            return "اعتبار مشتری به پایان رسیده است.";

        try
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorBody>();
            if (!string.IsNullOrWhiteSpace(error?.Message)) return error.Message;
        }
        catch { /* not JSON */ }

        return $"خطا در ارسال پیام ({(int)response.StatusCode}).";
    }

    private sealed class ErrorBody { public string? Message { get; set; } }

    private readonly List<RagDocTypeItem> _ragDocTypes =
    Enum.GetValues<RagDocType>()
        .Where(type => type != RagDocType.ImageGeneration) // image generation has its own page (/image-generation)
        .Select(type => new RagDocTypeItem
        {
            Code = type,
            Title = type.ToDisplay()
        })
        .ToList();
}
