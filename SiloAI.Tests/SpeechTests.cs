using SiloAI.Application.Api;
using SiloAI.Application.Shared.Contracts.AiModels;
using SiloAI.Application.Shared.Features;
using SiloAI.Domains;
using Xunit;

namespace SiloAI.Tests;

public class SpeechValidationTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Blank_text_is_rejected(string? text)
        => Assert.NotNull(SpeechValidation.ValidateSynthesis(new SynthesizeSpeechRequest { Text = text }));

    [Fact]
    public void Missing_request_is_rejected()
        => Assert.NotNull(SpeechValidation.ValidateSynthesis(null));

    [Fact]
    public void Text_at_the_limit_is_accepted_and_over_it_is_rejected()
    {
        Assert.Null(SpeechValidation.ValidateSynthesis(new SynthesizeSpeechRequest { Text = new string('a', SpeechLimits.MaxTextLength) }));
        Assert.NotNull(SpeechValidation.ValidateSynthesis(new SynthesizeSpeechRequest { Text = new string('a', SpeechLimits.MaxTextLength + 1) }));
    }

    // Voices are validated against the selected model's own list at synthesis time (AiModelVoices.Choose),
    // so request validation only rejects absurd names.
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("alloy")]
    [InlineData(" NOVA ")]
    [InlineData("en-US-JennyNeural")]
    public void Blank_or_plausible_voices_pass_request_validation(string? voice)
        => Assert.Null(SpeechValidation.ValidateSynthesis(new SynthesizeSpeechRequest { Text = "hi", Voice = voice }));

    [Fact]
    public void Overlong_voice_name_is_rejected()
        => Assert.NotNull(SpeechValidation.ValidateSynthesis(new SynthesizeSpeechRequest { Text = "hi", Voice = new string('v', 61) }));

    [Fact]
    public void The_service_default_voice_is_in_the_voice_list()
        => Assert.Contains("alloy", SpeechLimits.Voices);

    [Theory]
    [InlineData("clip.mp3", true)]
    [InlineData("clip.WAV", true)]
    [InlineData("recording.webm", true)]
    [InlineData("voice.m4a", true)]
    [InlineData("notes.txt", false)]
    [InlineData("noextension", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void Audio_file_names_are_checked_by_extension(string? name, bool expected)
        => Assert.Equal(expected, SpeechValidation.IsSupportedAudioFileName(name));
}

public class SpeechHeadersTests
{
    [Fact]
    public void Persian_text_round_trips_through_the_escaped_header_form()
    {
        const string text = "سلام، حال شما چطور است؟";

        Assert.Equal(text, SpeechHeaders.Decode(Uri.EscapeDataString(text)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Missing_header_decodes_to_empty(string? value)
        => Assert.Equal(string.Empty, SpeechHeaders.Decode(value));
}

/// <summary>Regression guard for the existing voice-chat pipeline (/api/rag/chat/voice).</summary>
public class RagVoiceChatServiceTests
{
    [Fact]
    public async Task Silence_is_rejected_before_chat_or_text_to_speech_run()
    {
        var stt = new FakeStt("   ");
        var tts = new FakeTts();
        var service = new RagVoiceChatService(stt, tts, mediator: null!, new FakeSpeechModels());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SendAsync(new RagVoiceChatRequest
        {
            Audio = new MemoryStream([1, 2, 3]),
            FileName = "a.webm",
            Username = "u",
            OwnerId = "o"
        }, default));

        Assert.Equal(1, stt.Calls);
        Assert.Equal(0, tts.Calls);
    }

    [Fact]
    public async Task Customer_without_credit_is_refused_before_any_provider_call()
    {
        var stt = new FakeStt("hello");
        var billing = new FakeBilling { Refuse = true };
        var service = new RagVoiceChatService(stt, new FakeTts(), mediator: null!, new FakeSpeechModels(), billing);

        await Assert.ThrowsAsync<InsufficientCreditException>(() => service.SendAsync(new RagVoiceChatRequest
        {
            Audio = new MemoryStream([1, 2, 3]), FileName = "a.webm", Username = "u", OwnerId = "o", CustomerId = 9
        }, default));

        Assert.Equal(0, stt.Calls);
    }

    [Fact]
    public async Task Transcription_is_billed_even_when_no_speech_is_recognized()
    {
        var stt = new FakeStt("   ");
        var billing = new FakeBilling();
        var service = new RagVoiceChatService(stt, new FakeTts(), mediator: null!, new FakeSpeechModels(), billing);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SendAsync(new RagVoiceChatRequest
        {
            Audio = new MemoryStream([1, 2, 3]), FileName = "a.webm", Username = "u", OwnerId = "o", CustomerId = 9
        }, default));

        Assert.Equal(1, billing.SttCharges);
        Assert.Equal(0, billing.TtsCharges);
    }

    private sealed class FakeBilling : ISpeechBillingService
    {
        public bool Refuse;
        public int SttCharges, TtsCharges;

        public Task EnsureCreditAsync(int? customerId, CancellationToken ct)
            => Refuse && customerId is not null ? throw new InsufficientCreditException() : Task.CompletedTask;

        public Task ChargeSpeechToTextAsync(int? customerId, ResolvedAiModel model, SpeechUsage usage, Guid requestId, CancellationToken ct)
        {
            SttCharges++;
            return Task.CompletedTask;
        }

        public Task ChargeTextToSpeechAsync(int? customerId, ResolvedAiModel model, int characters, Guid requestId, CancellationToken ct)
        {
            TtsCharges++;
            return Task.CompletedTask;
        }
    }

    private static ResolvedAiModel Model(AiModelKind kind, string? voices = null) =>
        new(Guid.NewGuid(), "m/" + kind, kind, null, false, false, false, false, false, false, false, false, 0m, 0m, 0m, voices);

    private sealed class FakeSpeechModels : ISpeechModelService
    {
        public Task<ResolvedAiModel> ResolveSpeechToTextAsync(int? customerId, Guid? modelId, CancellationToken ct)
            => Task.FromResult(Model(AiModelKind.SpeechToText));

        public Task<ResolvedAiModel> ResolveTextToSpeechAsync(int? customerId, Guid? modelId, CancellationToken ct)
            => Task.FromResult(Model(AiModelKind.TextToSpeech, "alloy,nova"));
    }

    private sealed class FakeStt(string transcript) : ISpeechToTextService
    {
        public int Calls;

        public Task<string> TranscribeAsync(Stream audio, string fileName, string? language, ResolvedAiModel model, CancellationToken ct)
        {
            Calls++;
            return Task.FromResult(transcript);
        }
    }

    [Fact]
    public async Task Unknown_voice_is_rejected_before_any_provider_call()
    {
        var stt = new FakeStt("hello");
        var tts = new FakeTts();
        var service = new RagVoiceChatService(stt, tts, mediator: null!, new FakeSpeechModels());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SendAsync(new RagVoiceChatRequest
        {
            Audio = new MemoryStream([1, 2, 3]),
            FileName = "a.webm",
            Voice = "robot",
            Username = "u",
            OwnerId = "o"
        }, default));

        Assert.Equal(0, stt.Calls);
        Assert.Equal(0, tts.Calls);
    }

    private sealed class FakeTts : ITextToSpeechService
    {
        public int Calls;

        public Task<SpeechResult> SynthesizeAsync(string text, string? voice, ResolvedAiModel model, CancellationToken ct)
        {
            Calls++;
            return Task.FromResult(new SpeechResult([1], "audio/mpeg"));
        }
    }
}

public class AiModelVoicesTests
{
    [Fact]
    public void Parse_trims_splits_and_drops_blanks_and_duplicates()
        => Assert.Equal(new[] { "alloy", "nova" }, AiModelVoices.Parse(" alloy, nova;;ALLOY\n").ToArray());

    [Fact]
    public void Normalize_returns_null_when_empty()
        => Assert.Null(AiModelVoices.Normalize(" , ; "));

    [Fact]
    public void Normalize_joins_with_commas()
        => Assert.Equal("alloy,nova", AiModelVoices.Normalize("alloy ; nova"));

    [Theory]
    [InlineData("alloy,nova", true)]
    [InlineData("en-US-JennyNeural", true)]
    [InlineData("bad voice!", false)]
    [InlineData("-leading", false)]
    public void Validate_checks_names(string voices, bool valid)
        => Assert.Equal(valid, AiModelVoices.Validate(voices) is null);

    [Fact]
    public void Choose_blank_gives_first_voice_or_fallback()
    {
        Assert.Equal("nova", AiModelVoices.Choose(new[] { "nova", "echo" }, null));
        Assert.Equal("alloy", AiModelVoices.Choose(Array.Empty<string>(), " "));
    }

    [Fact]
    public void Choose_matches_case_insensitively_and_returns_model_casing()
        => Assert.Equal("Nova", AiModelVoices.Choose(new[] { "alloy", "Nova" }, " nova "));

    [Fact]
    public void Choose_rejects_a_voice_the_model_does_not_offer()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => AiModelVoices.Choose(new[] { "alloy", "nova" }, "robot"));
        Assert.Contains("alloy, nova", ex.Message);
    }

    [Fact]
    public void Choose_passes_name_through_when_model_lists_no_voices()
        => Assert.Equal("robot", AiModelVoices.Choose(Array.Empty<string>(), " robot "));
}

public class SpeechModelServiceTests
{
    private static ResolvedAiModel Model(AiModelKind kind) =>
        new(Guid.NewGuid(), "m/x", kind, null, false, false, false, false, false, false, false, false, 0m, 0m, 0m);

    private sealed class FakeResolver(ResolvedAiModel model) : IAiModelResolver
    {
        public int? LastCustomer;
        public UsageFeature? LastFeature;

        public Task<ResolvedAiModel> ResolveAsync(int? customerId, UsageFeature feature, CancellationToken ct)
        {
            LastCustomer = customerId;
            LastFeature = feature;
            return Task.FromResult(model);
        }

        public Task<ResolvedAiModel> GetDefaultRagModelAsync(CancellationToken ct) => throw new NotSupportedException();
    }

    [Fact]
    public async Task Without_override_it_uses_the_assigned_model_for_the_feature()
    {
        var resolver = new FakeResolver(Model(AiModelKind.TextToSpeech));
        var service = new SpeechModelService(context: null!, resolver);

        var model = await service.ResolveTextToSpeechAsync(7, null, default);

        Assert.Equal(AiModelKind.TextToSpeech, model.Kind);
        Assert.Equal(7, resolver.LastCustomer);
        Assert.Equal(UsageFeature.TextToSpeech, resolver.LastFeature);
    }

    [Fact]
    public async Task A_model_of_the_wrong_kind_is_rejected()
    {
        var service = new SpeechModelService(context: null!, new FakeResolver(Model(AiModelKind.Normal)));

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ResolveSpeechToTextAsync(null, null, default));
    }
}
