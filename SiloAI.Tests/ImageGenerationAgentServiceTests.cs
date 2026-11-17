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

    private static readonly ImageBackendResult Result = new(
        [new GeneratedImageDto { DataBase64 = "AQ==", MediaType = "image/png" }], 120, 20, 30);

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
        Assert.Equal((120L, 20L, 30L), (result.InputTokens, result.CachedInputTokens, result.OutputTokens));
    }

    [Fact]
    public async Task With_an_input_image_the_edit_call_is_used()
    {
        var backend = new FakeBackend();
        var service = new ImageGenerationAgentService(backend, null!);

        await service.ExecuteAsync("make them happy", [Input], Model(), default);

        Assert.Equal(new[] { "edit" }, backend.Calls);
        Assert.Same(Input, Assert.Single(backend.LastImages!));
    }

    [Fact]
    public async Task Several_input_images_are_all_sent_to_the_edit_call()
    {
        var backend = new FakeBackend();
        var service = new ImageGenerationAgentService(backend, null!);

        await service.ExecuteAsync("basket", [Input, Input, Input, Input], Model(), default);

        Assert.Equal(new[] { "edit" }, backend.Calls);
        Assert.Equal(4, backend.LastImages!.Count);
    }

    [Fact]
    public async Task More_than_the_maximum_input_images_is_rejected_before_calling_the_model()
    {
        var backend = new FakeBackend();
        var service = new ImageGenerationAgentService(backend, null!);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExecuteAsync("x", Enumerable.Repeat(Input, ImageGenerationAgentService.MaxInputImages + 1).ToList(), Model(), default));

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
        public IReadOnlyList<ImageGenerationInputImage>? LastImages;

        public Task<ImageBackendResult> GenerateAsync(string model, string prompt, CancellationToken ct)
        {
            Calls.Add("generate");
            (LastModel, LastPrompt) = (model, prompt);
            return Task.FromResult(Result);
        }

        public Task<ImageBackendResult> EditAsync(string model, IReadOnlyList<ImageGenerationInputImage> images, string prompt, CancellationToken ct)
        {
            Calls.Add("edit");
            (LastModel, LastPrompt, LastImages) = (model, prompt, images);
            return Task.FromResult(Result);
        }
    }
}
