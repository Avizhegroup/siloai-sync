using Microsoft.AspNetCore.Http;
namespace SiloAI.Application.Api.Features;

/// <summary>Upload limits shared by the image endpoints (and their request-size attributes).</summary>
public static class ImageUploadLimits
{
    public const int MaxImages = 8;
    public const long MaxImageBytes = 10 * 1024 * 1024;
    public const long MaxRequestBytes = MaxImages * MaxImageBytes + 1024 * 1024;

    /// <summary>
    /// Reads an uploaded image, enforcing size and type. The type is detected from the bytes, not the
    /// declared content type (browsers send "" / wrong types).
    /// </summary>
    public static async Task<(byte[] Data, string MediaType)> ReadAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0 || file.Length > MaxImageBytes)
            throw new BadRequestException($"Each image must be between 1 byte and {MaxImageBytes / 1024 / 1024} MB.");

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, cancellationToken);
        var data = ms.ToArray();

        var mediaType = SiloAI.Agent.ImageGeneration.ImageFormat.Detect(data)
            ?? throw new BadRequestException("Unsupported image type. Use PNG, JPEG or WebP.");

        return (data, mediaType);
    }
}
