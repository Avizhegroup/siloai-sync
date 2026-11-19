using System.Net;
using System.Net.Http.Headers;
using SiloAI.UI.Components;
using Telerik.Blazor;

namespace SiloAI.UI.Pages;

public partial class Speech
{
    private enum SpeechTab { Voice, Stt, Tts }

    private sealed record DocTypeOption(RagDocType Code, string Title);

    private sealed record VoiceTurn(string Transcript, string ResponseText, string AudioSrc);

    private readonly List<DocTypeOption> _docTypes =
    [
        new(RagDocType.GeneralChat, RagDocType.GeneralChat.ToDisplay()),
        new(RagDocType.Support, RagDocType.Support.ToDisplay()),
        new(RagDocType.Report, RagDocType.Report.ToDisplay()),
        new(RagDocType.PageAgent, RagDocType.PageAgent.ToDisplay())
    ];

    private readonly List<VoiceTurn> _turns = [];

    private SpeechTab _activeTab = SpeechTab.Voice;
    private bool _isBusy;

    // voice chat
    private AudioClip? _voiceClip;
    private RagDocType _voiceDocType = RagDocType.GeneralChat;
    private string _voiceKey = string.Empty;
    private string _voiceName = string.Empty;
    private Guid? _voiceSttModelId;
    private Guid? _voiceTtsModelId;
    private string _voiceLanguage = string.Empty;
    private bool _voiceUseRag = true;
    private Guid? _conversationId;

    // speech to text
    private AudioClip? _sttClip;
    private Guid? _sttModelId;
    private string _sttLanguage = string.Empty;
    private string? _sttResult;

    // text to speech
    private string _ttsText = string.Empty;
    private Guid? _ttsModelId;
    private string _ttsVoice = string.Empty;
    private string? _ttsAudioSrc;

    // Models come from the AI Models page; voices belong to the selected text-to-speech model.
    private List<AiModelDto> _sttModels = [];
    private List<AiModelDto> _ttsModels = [];

    [Inject] public AiApiClient ApiClient { get; set; } = default!;

    [CascadingParameter] public TelerikNotification? Notification { get; set; }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var models = await ApiClient.GetFromJsonAsync<List<AiModelDto>>("admin/ai-models") ?? [];
            var assignments = await ApiClient.GetFromJsonAsync<List<ModelAssignmentDto>>("admin/ai-models/assignments") ?? [];

            _sttModels = models.Where(m => m.IsActive && m.Kind == AiModelKind.SpeechToText).ToList();
            _ttsModels = models.Where(m => m.IsActive && m.Kind == AiModelKind.TextToSpeech).ToList();

            // Start on the globally assigned model (what customers get), else the first available.
            _sttModelId = _voiceSttModelId = PickInitial(_sttModels, assignments, UsageFeature.SpeechToText);
            _ttsModelId = _voiceTtsModelId = PickInitial(_ttsModels, assignments, UsageFeature.TextToSpeech);

            _voiceName = DefaultVoice(_voiceTtsModelId);
            _ttsVoice = DefaultVoice(_ttsModelId);
        }
        catch (HttpRequestException)
        {
            Notification?.Show("بارگذاری مدل‌ها ناموفق بود.", "error");
        }
    }

    private static Guid? PickInitial(List<AiModelDto> models, List<ModelAssignmentDto> assignments, UsageFeature feature)
    {
        var assigned = assignments.FirstOrDefault(a => a.Feature == feature)?.AiModelId;

        return assigned is { } id && models.Any(m => m.Id == id) ? id : models.FirstOrDefault()?.Id;
    }

    private IReadOnlyList<string> VoicesOf(Guid? modelId) =>
        _ttsModels.FirstOrDefault(m => m.Id == modelId)?.VoiceList ?? [];

    private string DefaultVoice(Guid? modelId) => VoicesOf(modelId).FirstOrDefault() ?? string.Empty;

    private void VoiceTtsModelChanged(Guid? id)
    {
        _voiceTtsModelId = id;
        _voiceName = DefaultVoice(id);
    }

    private void TtsModelChanged(Guid? id)
    {
        _ttsModelId = id;
        _ttsVoice = DefaultVoice(id);
    }

    private void NewConversation()
    {
        _turns.Clear();
        _conversationId = null;
        _voiceClip = null;
    }

    private async Task SendVoiceAsync()
    {
        if (_voiceClip is null) return;

        await RunAsync(SpeechTab.Voice, async () =>
        {
            using var content = new MultipartFormDataContent();
            content.Add(AudioPart(_voiceClip), "audio", _voiceClip.FileName);
            content.Add(new StringContent(((int)_voiceDocType).ToString()), "docType");
            content.Add(new StringContent(_voiceUseRag ? "true" : "false"), "useRag");
            content.Add(new StringContent(_voiceName), "voice");

            if (_voiceSttModelId is { } sttId) content.Add(new StringContent(sttId.ToString()), "sttModelId");
            if (_voiceTtsModelId is { } ttsId) content.Add(new StringContent(ttsId.ToString()), "ttsModelId");

            if (_conversationId is { } id) content.Add(new StringContent(id.ToString()), "conversationId");
            if (!string.IsNullOrWhiteSpace(_voiceKey)) content.Add(new StringContent(_voiceKey.Trim()), "key");
            if (!string.IsNullOrWhiteSpace(_voiceLanguage)) content.Add(new StringContent(_voiceLanguage.Trim()), "language");

            var response = await ApiClient.PostMultipartAsync("api/rag/chat/voice", content);
            if (!response.IsSuccessStatusCode)
            {
                Notification?.Show(await ReadErrorAsync(response, "خطا در گفتگوی صوتی"), "error");
                return;
            }

            if (response.Headers.TryGetValues("X-Conversation-Id", out var ids)
                && Guid.TryParse(ids.FirstOrDefault(), out var conversationId))
                _conversationId = conversationId;

            var audio = await response.Content.ReadAsByteArrayAsync();
            var mediaType = response.Content.Headers.ContentType?.MediaType ?? "audio/mpeg";

            _turns.Add(new VoiceTurn(
                SpeechHeaders.Decode(HeaderValue(response, "X-Transcript")),
                SpeechHeaders.Decode(HeaderValue(response, "X-Response-Text")),
                $"data:{mediaType};base64,{Convert.ToBase64String(audio)}"));

            _voiceClip = null;
        });
    }

    private async Task TranscribeAsync()
    {
        if (_sttClip is null) return;

        await RunAsync(SpeechTab.Stt, async () =>
        {
            _sttResult = null;

            using var content = new MultipartFormDataContent();
            content.Add(AudioPart(_sttClip), "audio", _sttClip.FileName);

            if (_sttModelId is { } modelId) content.Add(new StringContent(modelId.ToString()), "modelId");

            if (!string.IsNullOrWhiteSpace(_sttLanguage)) content.Add(new StringContent(_sttLanguage.Trim()), "language");

            var response = await ApiClient.PostMultipartAsync("api/speech/transcribe", content);
            if (!response.IsSuccessStatusCode)
            {
                Notification?.Show(await ReadErrorAsync(response, "خطا در تبدیل گفتار به متن"), "error");
                return;
            }

            var result = await response.Content.ReadFromJsonAsync<TranscriptionResponse>();
            _sttResult = result?.Text;

            if (string.IsNullOrWhiteSpace(_sttResult))
                Notification?.Show("گفتاری شناسایی نشد.", "warning");
        });
    }

    private async Task SynthesizeAsync()
    {
        await RunAsync(SpeechTab.Tts, async () =>
        {
            _ttsAudioSrc = null;

            var response = await ApiClient.PostAsJsonAsync("api/speech/synthesize",
                new SynthesizeSpeechRequest { Text = _ttsText.Trim(), Voice = _ttsVoice, ModelId = _ttsModelId });

            if (!response.IsSuccessStatusCode)
            {
                Notification?.Show(await ReadErrorAsync(response, "خطا در تبدیل متن به گفتار"), "error");
                return;
            }

            var audio = await response.Content.ReadAsByteArrayAsync();
            var mediaType = response.Content.Headers.ContentType?.MediaType ?? "audio/mpeg";

            _ttsAudioSrc = $"data:{mediaType};base64,{Convert.ToBase64String(audio)}";
        });
    }

    private async Task RunAsync(SpeechTab tab, Func<Task> action)
    {
        if (_isBusy) return;

        _isBusy = true;
        _activeTab = tab;

        try
        {
            await action();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            Notification?.Show("ارتباط با سرور برقرار نشد یا زمان درخواست به پایان رسید.", "error");
        }
        finally
        {
            _isBusy = false;
        }
    }

    private static ByteArrayContent AudioPart(AudioClip clip)
    {
        var part = new ByteArrayContent(clip.Data);
        part.Headers.ContentType = new MediaTypeHeaderValue(clip.ContentType);
        return part;
    }

    private static string? HeaderValue(HttpResponseMessage response, string name) =>
        response.Headers.TryGetValues(name, out var values) ? values.FirstOrDefault() : null;

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, string fallback)
    {
        if (response.StatusCode == HttpStatusCode.PaymentRequired)
            return "اعتبار مشتری به پایان رسیده است.";

        try
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorBody>();
            if (!string.IsNullOrWhiteSpace(error?.Message)) return error.Message;
        }
        catch { /* not JSON */ }

        return $"{fallback} ({(int)response.StatusCode}).";
    }

    private sealed class ErrorBody { public string? Message { get; set; } }
}
