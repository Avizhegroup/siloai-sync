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

    [Inject] public AiApiClient ApiClient { get; set; }

    [CascadingParameter] public TelerikNotification Notification { get; set; }

    private void StartNewChat()
    {
        if (_isSending) return;

        _messages.Clear();

        _conversationId = null;

        _input = string.Empty;
    }

    private async Task OnInputKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
        {
            await SendAsync();
        }
    }

    private async Task SendAsync()
    {
        if (_isSending) return;

        var text = (_input ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(text)) return;

        _isSending = true;

        _input = string.Empty;

        _messages.Add(new ChatMessage
        {
            IsUser = true,
            Text = text,
            Time = DateTime.Now
        });

            var request = new RagChatRequest
            {
                Message = text,
                ConversationId = _conversationId,
                TopK = _topK <= 0 ? 5 : _topK,
                DocType = _docType,
                Key = string.IsNullOrWhiteSpace(_key) ? null : _key.Trim()
            };

            var response = await ApiClient.PostAsJsonAsync("api/rag/chat/send", request);

            response.EnsureSuccessStatusCode();

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
                Citations = result.Citations
            });
    
            _isSending = false;
    }

    private readonly List<RagDocTypeItem> _ragDocTypes =
    Enum.GetValues<RagDocType>()
        .Select(type => new RagDocTypeItem
        {
            Code = type,
            Title = type.ToDisplay()
        })
        .ToList();
}
