namespace SiloAI.Agent.ImageGeneration;

/// <summary>
/// Identifies an image by its bytes (not by the client-declared content type or file name, which
/// can be wrong or empty). The image-edit endpoint accepts only PNG, JPEG and WebP.
/// </summary>
public static class ImageFormat
{
    /// <returns>"image/png", "image/jpeg" or "image/webp"; null for anything else (GIF, BMP, HEIC, not an image...).</returns>
    public static string? Detect(ReadOnlySpan<byte> b)
    {
        if (b.Length >= 8 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47
            && b[4] == 0x0D && b[5] == 0x0A && b[6] == 0x1A && b[7] == 0x0A)
            return "image/png";

        if (b.Length >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF)
            return "image/jpeg";

        if (b.Length >= 12 && b[0] == 'R' && b[1] == 'I' && b[2] == 'F' && b[3] == 'F'
            && b[8] == 'W' && b[9] == 'E' && b[10] == 'B' && b[11] == 'P')
            return "image/webp";

        return null;
    }

    public static string Extension(string mediaType) => mediaType switch
    {
        "image/jpeg" => ".jpg",
        "image/webp" => ".webp",
        _ => ".png"
    };
}
