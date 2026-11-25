using Microsoft.AspNetCore.Http;
using SiloAI.Application.Shared;
using SiloAI.Application.Shared.Contracts.Customers;
using SiloAI.Application.Shared.Features;

namespace SiloAI.Application.Api.Features;

/// <summary>multipart form of POST api/speech/transcribe.</summary>
public class TranscribeSpeechCommand : IRequest<TranscriptionResponse>
{
    public IFormFile? Audio { get; set; }
    public string? Language { get; set; }

    /// <summary>Admin-only override of the speech-to-text model; null = the assigned model.</summary>
    public Guid? ModelId { get; set; }
}

public class TranscribeSpeechCommandHandler(
    ICustomerContext caller,
    ISpeechToTextService stt,
    ISpeechModelService speechModels,
    ISpeechBillingService billing) : IRequestHandler<TranscribeSpeechCommand, TranscriptionResponse>
{
    public async Task<TranscriptionResponse> Handle(TranscribeSpeechCommand request, CancellationToken cancellationToken)
    {
        var customerId = caller.CustomerId;

        // Choosing a model is an admin (no customer) capability; customers get their assigned one.
        if (request.ModelId is not null && customerId is not null)
            throw new ForbiddenException("Customers cannot choose the speech model.");

        if (request.Audio is null || request.Audio.Length == 0)
            throw new BadRequestException("Audio file is required.");

        if (request.Audio.Length > SpeechLimits.MaxAudioBytes)
            throw new BadRequestException("Audio file is too large.");

        if (!SpeechValidation.IsSupportedAudioFileName(request.Audio.FileName))
            throw new BadRequestException("Unsupported audio type. Use mp3, wav, flac, m4a, mp4, ogg or webm.");

        await using var stream = request.Audio.OpenReadStream();

        var model = await speechModels.ResolveSpeechToTextAsync(customerId, request.ModelId, cancellationToken);

        await billing.EnsureCreditAsync(customerId, cancellationToken);

        var transcription = await stt.TranscribeWithUsageAsync(
            stream, request.Audio.FileName, request.Language, model, cancellationToken);

        await billing.ChargeSpeechToTextAsync(customerId, model, transcription.Usage, Guid.NewGuid(), cancellationToken);

        return new TranscriptionResponse { Text = transcription.Text.Trim() };
    }
}

/// <summary>JSON body of POST api/speech/synthesize (same fields as <see cref="SynthesizeSpeechRequest"/>).</summary>
public class SynthesizeSpeechCommand : SynthesizeSpeechRequest, IRequest<SpeechResult>;

public class SynthesizeSpeechCommandHandler(
    ICustomerContext caller,
    ITextToSpeechService tts,
    ISpeechModelService speechModels,
    ISpeechBillingService billing) : IRequestHandler<SynthesizeSpeechCommand, SpeechResult>
{
    public async Task<SpeechResult> Handle(SynthesizeSpeechCommand request, CancellationToken cancellationToken)
    {
        var customerId = caller.CustomerId;

        if (request.ModelId is not null && customerId is not null)
            throw new ForbiddenException("Customers cannot choose the speech model.");

        var error = SpeechValidation.ValidateSynthesis(request);
        if (error is not null)
            throw new BadRequestException(error);

        var model = await speechModels.ResolveTextToSpeechAsync(customerId, request.ModelId, cancellationToken);

        await billing.EnsureCreditAsync(customerId, cancellationToken);

        var text = request.Text!.Trim();
        var speech = await tts.SynthesizeAsync(text, request.Voice?.Trim(), model, cancellationToken);

        await billing.ChargeTextToSpeechAsync(customerId, model, text.Length, Guid.NewGuid(), cancellationToken);

        return speech;
    }
}

/// <summary>multipart form of POST api/rag/chat/voice.</summary>
public class RagVoiceChatCommand : IRequest<RagVoiceChatResult>
{
    public IFormFile? Audio { get; set; }
    public Guid? ConversationId { get; set; }
    public RagDocType DocType { get; set; } = RagDocType.GeneralChat;
    public string? Key { get; set; }
    public bool IsMainChat { get; set; }
    public bool UseRag { get; set; } = true;
    public string? Language { get; set; }
    public string? Voice { get; set; }
    public Guid? SttModelId { get; set; }
    public Guid? TtsModelId { get; set; }
}

public class RagVoiceChatCommandHandler(ICustomerContext caller, IRagVoiceChatService voiceService)
    : IRequestHandler<RagVoiceChatCommand, RagVoiceChatResult>
{
    public async Task<RagVoiceChatResult> Handle(RagVoiceChatCommand request, CancellationToken cancellationToken)
    {
        if (request.Audio is null || request.Audio.Length == 0)
            throw new BadRequestException("Audio file is required.");

        // Choosing speech models is an admin (no customer) capability.
        if ((request.SttModelId is not null || request.TtsModelId is not null) && caller.CustomerId is not null)
            throw new ForbiddenException("Customers cannot choose the speech models.");

        await using var audioStream = request.Audio.OpenReadStream();

        return await voiceService.SendAsync(new RagVoiceChatRequest
        {
            Audio = audioStream,
            FileName = request.Audio.FileName,
            Language = request.Language,
            Voice = request.Voice,
            SttModelId = request.SttModelId,
            TtsModelId = request.TtsModelId,
            ConversationId = request.ConversationId,
            DocType = request.DocType,
            Key = request.Key,
            IsMainChat = request.IsMainChat,
            UseRag = request.UseRag,
            Username = caller.Username,
            OwnerId = caller.OwnerId,
            CustomerId = caller.CustomerId
        }, cancellationToken);
    }
}
