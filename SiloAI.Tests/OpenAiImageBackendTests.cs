using SiloAI.Agent.ImageGeneration;
using Xunit;

namespace SiloAI.Tests;

public class OpenAiImageBackendTests
{
    [Fact]
    public void PNG_is_the_default_media_type()
        => Assert.Equal("image/png", OpenAiImageBackend.DetectMediaType([0x89, 0x50, 0x4E, 0x47, 0x0D]));

    [Fact]
    public void JPEG_is_detected()
        => Assert.Equal("image/jpeg", OpenAiImageBackend.DetectMediaType([0xFF, 0xD8, 0xFF, 0xE0]));

    [Fact]
    public void WebP_is_detected()
        => Assert.Equal("image/webp", OpenAiImageBackend.DetectMediaType(
            "RIFF\0\0\0\0WEBP"u8.ToArray()));

    [Theory]
    [InlineData("image/jpeg", ".jpg")]
    [InlineData("image/webp", ".webp")]
    [InlineData("image/gif", ".gif")]
    [InlineData("image/png", ".png")]
    [InlineData("application/x", ".png")]
    public void Extension_matches_media_type(string mediaType, string ext)
        => Assert.Equal(ext, OpenAiImageBackend.ExtensionFor(mediaType));
}
