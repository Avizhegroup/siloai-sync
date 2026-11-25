using System.Text;
using Microsoft.AspNetCore.Http;
using SiloAI.Application.Api.Features;
using SiloAI.Application.Shared;
using SiloAI.Application.Shared.Contracts.Customers;
using Xunit;

namespace SiloAI.Tests;

/// <summary>
/// Rules that used to live in controllers (validation, "customers may not pick a model") now live in
/// handlers; they reject the request before anything else is touched, so no collaborators are needed.
/// </summary>
public class RequestHandlerRuleTests
{
    private sealed class Caller(int? customerId) : ICustomerContext
    {
        public int? CustomerId => customerId;
        public string OwnerId => customerId is null ? "user:1" : $"customer:{customerId}";
        public string Username => "tester";
    }

    private static IFormFile File(string name, byte[] bytes)
        => new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", name);

    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0];

    // --- image generation / analysis / OCR ---

    [Fact]
    public async Task Image_generation_requires_a_prompt()
        => await Assert.ThrowsAsync<BadRequestException>(() =>
            new GenerateImageFormCommandHandler(null!).Handle(new GenerateImageFormCommand { Prompt = "  " }, default));

    [Fact]
    public async Task Image_generation_rejects_too_many_input_images()
    {
        var command = new GenerateImageFormCommand { Prompt = "x" };
        for (var i = 0; i <= ImageUploadLimits.MaxImages; i++) command.Images.Add(File("a.png", Png));

        await Assert.ThrowsAsync<BadRequestException>(() => new GenerateImageFormCommandHandler(null!).Handle(command, default));
    }

    [Fact]
    public async Task Image_generation_rejects_files_that_are_not_images()
    {
        var command = new GenerateImageFormCommand { Prompt = "x" };
        command.Images.Add(File("a.png", Encoding.UTF8.GetBytes("definitely not an image")));

        await Assert.ThrowsAsync<BadRequestException>(() => new GenerateImageFormCommandHandler(null!).Handle(command, default));
    }

    [Fact]
    public async Task Image_generation_rejects_empty_files()
    {
        var command = new GenerateImageFormCommand { Prompt = "x" };
        command.Images.Add(File("a.png", []));

        await Assert.ThrowsAsync<BadRequestException>(() => new GenerateImageFormCommandHandler(null!).Handle(command, default));
    }

    [Fact]
    public async Task Image_analysis_requires_at_least_one_image()
        => await Assert.ThrowsAsync<BadRequestException>(() =>
            new AnalyzeImageFormCommandHandler(null!).Handle(new AnalyzeImageFormCommand(), default));

    [Fact]
    public async Task Image_analysis_rejects_files_that_are_not_images()
    {
        var command = new AnalyzeImageFormCommand();
        command.Images.Add(File("a.png", Encoding.UTF8.GetBytes("nope")));

        await Assert.ThrowsAsync<BadRequestException>(() => new AnalyzeImageFormCommandHandler(null!).Handle(command, default));
    }

    [Fact]
    public async Task Ocr_requires_a_file()
        => await Assert.ThrowsAsync<BadRequestException>(() =>
            new OcrFormCommandHandler(null!).Handle(new OcrFormCommand(), default));

    // --- speech ---

    [Fact]
    public async Task A_customer_cannot_choose_the_transcription_model()
    {
        var handler = new TranscribeSpeechCommandHandler(new Caller(5), null!, null!, null!);

        await Assert.ThrowsAsync<ForbiddenException>(() => handler.Handle(
            new TranscribeSpeechCommand { ModelId = Guid.NewGuid(), Audio = File("a.mp3", [1]) }, default));
    }

    [Fact]
    public async Task Transcription_requires_audio_and_a_supported_type()
    {
        var handler = new TranscribeSpeechCommandHandler(new Caller(null), null!, null!, null!);

        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(new TranscribeSpeechCommand(), default));
        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(
            new TranscribeSpeechCommand { Audio = File("notes.txt", [1, 2]) }, default));
    }

    [Fact]
    public async Task A_customer_cannot_choose_the_synthesis_model()
    {
        var handler = new SynthesizeSpeechCommandHandler(new Caller(5), null!, null!, null!);

        await Assert.ThrowsAsync<ForbiddenException>(() => handler.Handle(
            new SynthesizeSpeechCommand { Text = "hi", ModelId = Guid.NewGuid() }, default));
    }

    [Fact]
    public async Task Synthesis_rejects_blank_text()
    {
        var handler = new SynthesizeSpeechCommandHandler(new Caller(null), null!, null!, null!);

        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(new SynthesizeSpeechCommand { Text = " " }, default));
    }

    [Fact]
    public async Task A_customer_cannot_choose_the_voice_chat_speech_models()
    {
        var handler = new RagVoiceChatCommandHandler(new Caller(5), null!);

        await Assert.ThrowsAsync<ForbiddenException>(() => handler.Handle(
            new RagVoiceChatCommand { Audio = File("a.webm", [1]), SttModelId = Guid.NewGuid() }, default));
        await Assert.ThrowsAsync<ForbiddenException>(() => handler.Handle(
            new RagVoiceChatCommand { Audio = File("a.webm", [1]), TtsModelId = Guid.NewGuid() }, default));
    }

    [Fact]
    public async Task Voice_chat_requires_audio()
        => await Assert.ThrowsAsync<BadRequestException>(() =>
            new RagVoiceChatCommandHandler(new Caller(null), null!).Handle(new RagVoiceChatCommand(), default));
}
