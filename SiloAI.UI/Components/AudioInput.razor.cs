using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Telerik.Blazor;

namespace SiloAI.UI.Components;

/// <summary>An audio clip recorded from the microphone or chosen from disk.</summary>
public sealed record AudioClip(byte[] Data, string FileName, string ContentType);

public partial class AudioInput : IAsyncDisposable
{
    private bool IsRecording;

    [Parameter] public AudioClip? Value { get; set; }
    [Parameter] public EventCallback<AudioClip?> ValueChanged { get; set; }
    [Parameter] public bool Disabled { get; set; }

    [Inject] private IJSRuntime JS { get; set; } = default!;

    [CascadingParameter] public TelerikNotification? Notification { get; set; }

    private sealed class RecordingResult
    {
        public string Data { get; set; } = string.Empty;
        public string Mime { get; set; } = "audio/webm";
    }

    private async Task StartRecordingAsync()
    {
        var supported = await JS.InvokeAsync<bool>("siloSpeech.isSupported");
        if (!supported || !await JS.InvokeAsync<bool>("siloSpeech.start"))
        {
            Notification?.Show("دسترسی به میکروفون ممکن نیست. اجازه دسترسی را بررسی کنید یا فایل صوتی انتخاب کنید.", "warning");
            return;
        }

        IsRecording = true;
    }

    private async Task StopRecordingAsync()
    {
        IsRecording = false;

        var result = await JS.InvokeAsync<RecordingResult?>("siloSpeech.stop");
        if (result is null || string.IsNullOrEmpty(result.Data))
        {
            Notification?.Show("صدایی ضبط نشد.", "warning");
            return;
        }

        var mime = result.Mime.Split(';')[0];
        await SetAsync(new AudioClip(Convert.FromBase64String(result.Data), "recording" + ExtensionFor(mime), mime));
    }

    private async Task OnFileSelectedAsync(InputFileChangeEventArgs args)
    {
        var file = args.File;

        if (!SpeechValidation.IsSupportedAudioFileName(file.Name))
        {
            Notification?.Show("نوع فایل صوتی پشتیبانی نمی‌شود.", "warning");
            return;
        }

        if (file.Size > SpeechLimits.MaxAudioBytes)
        {
            Notification?.Show("حجم فایل صوتی بیش از ۲۵ مگابایت است.", "warning");
            return;
        }

        using var stream = file.OpenReadStream(SpeechLimits.MaxAudioBytes);
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);

        await SetAsync(new AudioClip(ms.ToArray(), file.Name,
            string.IsNullOrWhiteSpace(file.ContentType) ? "audio/mpeg" : file.ContentType));
    }

    private Task ClearAsync() => SetAsync(null);

    private async Task SetAsync(AudioClip? clip)
    {
        Value = clip;
        await ValueChanged.InvokeAsync(clip);
    }

    internal static string ExtensionFor(string mime) => mime switch
    {
        "audio/mp4" => ".mp4",
        "audio/ogg" => ".ogg",
        "audio/wav" or "audio/x-wav" => ".wav",
        "audio/mpeg" => ".mp3",
        _ => ".webm"
    };

    public async ValueTask DisposeAsync()
    {
        if (IsRecording)
        {
            try { await JS.InvokeVoidAsync("siloSpeech.cancel"); } catch { /* circuit already gone */ }
        }
    }
}
