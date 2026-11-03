using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Forms;
using Telerik.Blazor;

namespace SiloAI.UI.Pages;

public partial class ImageGeneration
{
    public const int MaxImages = 1;
    public const long MaxImageSize = 5 * 1024 * 1024;

    private sealed record InputImage(IBrowserFile File, string Preview);

    private readonly List<InputImage> _inputs = [];
    private readonly List<GeneratedImageDto> _results = [];
    private string _prompt = string.Empty;
    private string _key = string.Empty;
    private string? _resultText;
    private bool _isSending;

    [Inject] public AiApiClient ApiClient { get; set; } = default!;

    [CascadingParameter] public TelerikNotification? Notification { get; set; }

    private async Task OnFilesSelected(InputFileChangeEventArgs args)
    {
        // The image-edit API takes one input image: a new selection replaces the previous one.
        foreach (var file in args.GetMultipleFiles(MaxImages))
        {
            if (file.Size > MaxImageSize)
            {
                Notification?.Show($"حجم «{file.Name}» بیش از ۵ مگابایت است.", "warning");
                continue;
            }

            _inputs.Clear();

            using var stream = file.OpenReadStream(MaxImageSize);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);

            _inputs.Add(new InputImage(file, $"data:{file.ContentType};base64,{Convert.ToBase64String(ms.ToArray())}"));
        }
    }

    private void RemoveInput(InputImage item) => _inputs.Remove(item);

    private void Reset()
    {
        _inputs.Clear();
        _results.Clear();
        _resultText = null;
        _prompt = string.Empty;
    }

    private async Task GenerateAsync()
    {
        if (_isSending || string.IsNullOrWhiteSpace(_prompt)) return;

        _isSending = true;
        _results.Clear();
        _resultText = null;

        try
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(_prompt.Trim()), "prompt");
            content.Add(new StringContent(Guid.NewGuid().ToString()), "requestId");

            if (!string.IsNullOrWhiteSpace(_key))
                content.Add(new StringContent(_key.Trim()), "key");

            foreach (var item in _inputs)
            {
                var stream = item.File.OpenReadStream(MaxImageSize);
                var part = new StreamContent(stream);
                part.Headers.ContentType = new MediaTypeHeaderValue(item.File.ContentType);
                content.Add(part, "images", item.File.Name);
            }

            var response = await ApiClient.PostMultipartAsync("api/image-generation", content);

            if (!response.IsSuccessStatusCode)
            {
                Notification?.Show(await ReadErrorAsync(response), "error");
                return;
            }

            var result = await response.Content.ReadFromJsonAsync<ImageGenerationResponse>();

            _results.AddRange(result?.Images ?? []);
            _resultText = result?.Text;

            if (_results.Count == 0)
                Notification?.Show("تصویری دریافت نشد.", "warning");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            Notification?.Show("ارتباط با سرور برقرار نشد یا زمان درخواست به پایان رسید.", "error");
        }
        finally
        {
            _isSending = false;
        }
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

        return $"خطا در تولید تصویر ({(int)response.StatusCode}).";
    }

    private sealed class ErrorBody { public string? Message { get; set; } }

    private static string ToDataUri(GeneratedImageDto image) => $"data:{image.MediaType};base64,{image.DataBase64}";

    private static string Extension(string mediaType) => mediaType switch
    {
        "image/jpeg" => "jpg",
        "image/webp" => "webp",
        "image/gif" => "gif",
        _ => "png"
    };
}
