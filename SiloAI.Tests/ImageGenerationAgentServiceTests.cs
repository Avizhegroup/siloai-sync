using SiloAI.Agent.ImageGeneration;
using SiloAI.Application.Shared.Contracts.AiModels;
using SiloAI.Application.Shared.Features;
using SiloAI.Domains;
using Xunit;

namespace SiloAI.Tests;

public class ImageGenerationAgentServiceTests
{
    internal static ResolvedAiModel Model(AiModelKind kind = AiModelKind.ImageGeneration) => new(
        Guid.NewGuid(), "openai/gpt-image-2", kind, null,
        false, false, false, false, false, false, false, false, 1m, 2m, 0.5m);

    private static readonly ImageGenerationInputImage Input = new() { Data = [1, 2, 3], MediaType = "image/png" };

    [Theory]
    [InlineData(AiModelKind.Normal)]
    [InlineData(AiModelKind.Rag)]
    public void Only_image_generation_models_are_accepted(AiModelKind kind)
        => Assert.Throws<InvalidOperationException>(() => ImageGenerationAgentService.ValidateModel(Model(kind)));

    [Fact]
    public void Image_generation_model_is_accepted()
        => ImageGenerationAgentService.ValidateModel(Model());

    [Fact]
    public void Prompt_without_instructions_is_just_the_request()
        => Assert.Equal("make them happy", ImageGenerationAgentService.BuildPrompt("  ", " make them happy "));

    [Fact]
    public void Prompt_puts_instructions_before_the_request()
    {
        var prompt = ImageGenerationAgentService.BuildPrompt("Use watercolor", "a cat");

        Assert.StartsWith("Use watercolor", prompt);
        Assert.EndsWith("a cat", prompt);
    }

    [Fact]
    public void Empty_request_is_rejected()
        => Assert.Throws<ArgumentException>(() => ImageGenerationAgentService.BuildPrompt("x", " "));

    [Fact]
    public async Task Without_an_input_image_the_text_to_image_call_is_used()
    {
        var backend = new FakeBackend();
        var service = new ImageGenerationAgentService(backend, null!);

        var result = await service.ExecuteAsync("a cat", [], Model(), default);

        Assert.Equal(new[] { "generate" }, backend.Calls);
        Assert.Equal("openai/gpt-image-2", backend.LastModel);
        Assert.Equal("a cat", backend.LastPrompt);
        Assert.Single(result.Images);
        Assert.Equal(0, result.OutputTokens);
    }

    [Fact]
    public async Task With_an_input_image_the_edit_call_is_used()
    {
        var backend = new FakeBackend();
        var service = new ImageGenerationAgentService(backend, null!);

        await service.ExecuteAsync("make them happy", [Input], Model(), default);

        Assert.Equal(new[] { "edit" }, backend.Calls);
        Assert.Same(Input, backend.LastImage);
    }

    [Fact]
    public async Task More_than_one_input_image_is_rejected_before_calling_the_model()
    {
        var backend = new FakeBackend();
        var service = new ImageGenerationAgentService(backend, null!);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExecuteAsync("x", [Input, Input], Model(), default));

        Assert.Empty(backend.Calls);
    }

    [Fact]
    public async Task A_non_image_model_is_rejected_before_calling_the_model()
    {
        var backend = new FakeBackend();
        var service = new ImageGenerationAgentService(backend, null!);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExecuteAsync("x", [], Model(AiModelKind.Normal), default));

        Assert.Empty(backend.Calls);
    }

    private sealed class FakeBackend : IImageBackend
    {
        public List<string> Calls { get; } = [];
        public string? LastModel, LastPrompt;
        public ImageGenerationInputImage? LastImage;

        private static GeneratedImageDto Image() => new() { DataBase64 = "AQ==", MediaType = "image/png" };

        public Task<GeneratedImageDto> GenerateAsync(string model, string prompt, CancellationToken ct)
        {
            Calls.Add("generate");
            (LastModel, LastPrompt) = (model, prompt);
            return Task.FromResult(Image());
        }

        public Task<GeneratedImageDto> EditAsync(string model, ImageGenerationInputImage image, string prompt, CancellationToken ct)
        {
            Calls.Add("edit");
            (LastModel, LastPrompt, LastImage) = (model, prompt, image);
            return Task.FromResult(Image());
        }
    }
}
