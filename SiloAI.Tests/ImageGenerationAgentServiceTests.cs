using Microsoft.Extensions.AI;
using SiloAI.Agent.ImageGeneration;
using SiloAI.Application.Shared.Contracts.AiModels;
using SiloAI.Domains;
using Xunit;

namespace SiloAI.Tests;

public class ImageGenerationAgentServiceTests
{
    internal static ResolvedAiModel Model(bool textIn = true, bool imgIn = true, bool imgOut = true) => new(
        Guid.NewGuid(), "vendor/img", AiModelKind.Normal, null,
        textIn, true, imgIn, imgOut, false, false, false, false, 1m, 2m, 0.5m);

    [Fact]
    public void Model_must_support_text_in_and_image_out()
    {
        Assert.Throws<InvalidOperationException>(() => ImageGenerationAgentService.ValidateModel(Model(imgOut: false), false));
        Assert.Throws<InvalidOperationException>(() => ImageGenerationAgentService.ValidateModel(Model(textIn: false), false));
        ImageGenerationAgentService.ValidateModel(Model(imgIn: false), hasInputImages: false); // ok
    }

    [Fact]
    public void Input_images_require_image_input_support()
    {
        Assert.Throws<InvalidOperationException>(() => ImageGenerationAgentService.ValidateModel(Model(imgIn: false), true));
        ImageGenerationAgentService.ValidateModel(Model(), true); // ok
    }

    [Fact]
    public void ToResult_collects_images_text_and_usage()
    {
        var result = ImageGenerationAgentService.ToResult(
            [new TextContent(" hello "), new DataContent(new byte[] { 1, 2 }, "image/png")], 10, 2, 5);

        var image = Assert.Single(result.Images);
        Assert.Equal(Convert.ToBase64String(new byte[] { 1, 2 }), image.DataBase64);
        Assert.Equal("image/png", image.MediaType);
        Assert.Equal("hello", result.Text);
        Assert.Equal((10L, 2L, 5L), (result.InputTokens, result.CachedInputTokens, result.OutputTokens));
    }

    [Fact]
    public void ToResult_without_an_image_throws_and_includes_model_text()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => ImageGenerationAgentService.ToResult([new TextContent("I cannot do that")], 1, 0, 1));

        Assert.Contains("I cannot do that", ex.Message);
    }
}
