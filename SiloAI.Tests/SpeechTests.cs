using SiloAI.Application.Api;
using SiloAI.Application.Shared.Features;
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

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("alloy")]
    [InlineData(" NOVA ")]
    public void Known_or_blank_voices_are_accepted(string? voice)
        => Assert.Null(SpeechValidation.ValidateSynthesis(new SynthesizeSpeechRequest { Text = "hi", Voice = voice }));

    [Fact]
    public void Unknown_voice_is_rejected()
        => Assert.NotNull(SpeechValidation.ValidateSynthesis(new SynthesizeSpeechRequest { Text = "hi", Voice = "robot" }));

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
        var service = new RagVoiceChatService(stt, tts, mediator: null!);

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

    private sealed class FakeStt(string transcript) : ISpeechToTextService
    {
        public int Calls;

        public Task<string> TranscribeAsync(Stream audio, string fileName, string? language, CancellationToken ct)
        {
            Calls++;
            return Task.FromResult(transcript);
        }
    }

    private sealed class FakeTts : ITextToSpeechService
    {
        public int Calls;

        public Task<SpeechResult> SynthesizeAsync(string text, string? voice, CancellationToken ct)
        {
            Calls++;
            return Task.FromResult(new SpeechResult([1], "audio/mpeg"));
        }
    }
}
