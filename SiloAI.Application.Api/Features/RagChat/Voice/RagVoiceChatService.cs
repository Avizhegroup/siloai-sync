using SiloAI.Application.Shared.Features;

namespace SiloAI.Application.Api;

public interface ISpeechToTextService
{
    Task<string> TranscribeAsync(Stream audio, string fileName, string? language, CancellationToken cancellationToken);
}

public interface ITextToSpeechService
{
    Task<SpeechResult> SynthesizeAsync(string text, string? voice, CancellationToken cancellationToken);
}

public sealed record SpeechResult(byte[] Audio, string ContentType);

public sealed class RagVoiceChatRequest
{
    public required Stream Audio { get; init; }
    public required string FileName { get; init; }
    public string? Language { get; init; }
    public string? Voice { get; init; }
    public Guid? ConversationId { get; init; }
    public RagDocType DocType { get; init; } = RagDocType.GeneralChat;
    public string? Key { get; init; }
    public bool IsMainChat { get; init; }
    public bool UseRag { get; init; } = true;
    public required string Username { get; init; }
    public required string OwnerId { get; init; }
    public int? CustomerId { get; init; }
}

public sealed class RagVoiceChatResult
{
    public required string Transcript { get; init; }
    public required string ResponseText { get; init; }
    public required Guid ConversationId { get; init; }
    public required byte[] Audio { get; init; }
    public required string AudioContentType { get; init; }
}

public interface IRagVoiceChatService
{
    Task<RagVoiceChatResult> SendAsync(RagVoiceChatRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Voice pipeline: speech-to-text -> existing RAG chat (RagChatSendHandler, which also handles
/// session state, pricing and ledger) -> text-to-speech.
/// </summary>
public sealed class RagVoiceChatService(
    ISpeechToTextService stt,
    ITextToSpeechService tts,
    IMediator mediator) : IRagVoiceChatService
{
    public async Task<RagVoiceChatResult> SendAsync(RagVoiceChatRequest request, CancellationToken cancellationToken)
    {
        var transcript = (await stt.TranscribeAsync(
            request.Audio, request.FileName, request.Language, cancellationToken)).Trim();

        if (transcript.Length == 0)
            throw new InvalidOperationException("No speech could be recognized in the supplied audio.");

        var chat = await mediator.Send(new RagChatSendCommand
        {
            ConversationId = request.ConversationId,
            Message = transcript,
            IsMainChat = request.IsMainChat,
            DocType = request.DocType,
            Key = request.Key,
            UseRag = request.UseRag,
            Username = request.Username,
            OwnerId = request.OwnerId,
            CustomerId = request.CustomerId
        }, cancellationToken);

        var speech = await tts.SynthesizeAsync(chat.ResponseText, request.Voice, cancellationToken);

        return new RagVoiceChatResult
        {
            Transcript = transcript,
            ResponseText = chat.ResponseText,
            ConversationId = chat.ConversationId,
            Audio = speech.Audio,
            AudioContentType = speech.ContentType
        };
    }
}
