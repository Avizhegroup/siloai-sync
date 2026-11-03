using SiloAI.Agent.ImageGeneration;
using Xunit;

namespace SiloAI.Tests;

public class ImageFormatTests
{
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0];

    [Fact]
    public void PNG_is_detected() => Assert.Equal("image/png", ImageFormat.Detect(Png));

    [Fact]
    public void JPEG_is_detected() => Assert.Equal("image/jpeg", ImageFormat.Detect([0xFF, 0xD8, 0xFF, 0xE0]));

    [Fact]
    public void WebP_is_detected() => Assert.Equal("image/webp", ImageFormat.Detect("RIFF\0\0\0\0WEBPVP8 "u8));

    [Fact]
    public void GIF_is_not_accepted() => Assert.Null(ImageFormat.Detect("GIF89a\0\0\0\0\0\0"u8));

    [Fact]
    public void Non_images_and_empty_input_are_not_accepted()
    {
        Assert.Null(ImageFormat.Detect("hello world, not an image"u8));
        Assert.Null(ImageFormat.Detect([]));
    }

    [Theory]
    [InlineData("image/jpeg", ".jpg")]
    [InlineData("image/webp", ".webp")]
    [InlineData("image/png", ".png")]
    public void Extension_matches_media_type(string mediaType, string ext)
        => Assert.Equal(ext, ImageFormat.Extension(mediaType));
}

public class OpenAiImageBackendTests
{
    [Fact]
    public async Task Edit_rejects_a_non_jpg_png_webp_input_before_calling_the_endpoint()
    {
        var backend = new OpenAiImageBackend(Microsoft.Extensions.Options.Options.Create(new SiloAI.Agent.Rag.OpenAIOptions()));

        await Assert.ThrowsAsync<InvalidOperationException>(() => backend.EditAsync(
            "m", new SiloAI.Application.Shared.Features.ImageGenerationInputImage { Data = "GIF89a....."u8.ToArray(), MediaType = "image/gif" },
            "p", default));
    }
}
