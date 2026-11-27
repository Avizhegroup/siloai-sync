namespace SiloAI.Application.Shared.Features;

/// <summary>How the model receives an attachment.</summary>
public enum RagChatAttachmentKind
{
    /// <summary>PNG / JPEG / WebP, sent to the model as an image (needs an image-capable chat model).</summary>
    Image = 1,

    /// <summary>PDF, sent to the model as a file (needs a file-capable chat model).</summary>
    Pdf = 2,

    /// <summary>Plain-text formats, decoded and inlined into the message (works with every chat model).</summary>
    Text = 3
}

/// <summary>
/// An attachment that already passed validation. It lives only for the one chat turn it was sent
/// with: the model sees it for that turn, the saved conversation keeps just the file names.
/// </summary>
public class RagChatAttachment
{
    public string FileName { get; set; } = string.Empty;
    public string MediaType { get; set; } = string.Empty;
    public RagChatAttachmentKind Kind { get; set; }
    public int SizeBytes { get; set; }

    /// <summary>Raw bytes for <see cref="RagChatAttachmentKind.Image"/> and <see cref="RagChatAttachmentKind.Pdf"/>.</summary>
    public byte[] Data { get; set; } = [];

    /// <summary>Decoded text for <see cref="RagChatAttachmentKind.Text"/>.</summary>
    public string? Text { get; set; }
}

/// <summary>
/// The single definition of what a chat message may carry as attachments. The API validates against
/// it and the Chat page reads the same constants, so the two can't drift apart.
/// </summary>
public static class RagChatAttachmentRules
{
    /// <summary>Maximum number of files in one message.</summary>
    public const int MaxFiles = 5;

    /// <summary>Maximum size of one file.</summary>
    public const long MaxFileBytes = 10 * 1024 * 1024;

    /// <summary>Maximum combined size of all files in one message.</summary>
    public const long MaxTotalBytes = 25 * 1024 * 1024;

    /// <summary>Request-size ceiling for the multipart endpoint (files + form fields).</summary>
    public const long MaxRequestBytes = MaxTotalBytes + 1024 * 1024;

    /// <summary>A text file longer than this is cut (with a notice to the model) so one file can't flood the prompt.</summary>
    public const int MaxTextCharsPerFile = 100_000;

    public static readonly IReadOnlyList<string> ImageExtensions = [".png", ".jpg", ".jpeg", ".webp"];

    public static readonly IReadOnlyList<string> PdfExtensions = [".pdf"];

    public static readonly IReadOnlyList<string> TextExtensions =
        [".txt", ".md", ".csv", ".json", ".xml", ".yaml", ".yml", ".log"];

    public static IReadOnlyList<string> AllExtensions { get; } =
        [.. ImageExtensions, .. PdfExtensions, .. TextExtensions];

    /// <summary>Value for an HTML file input's <c>accept</c> attribute.</summary>
    public static string AcceptAttribute => string.Join(",", AllExtensions);

    /// <summary>The kind for a file name, or null when its extension is not allowed.</summary>
    public static RagChatAttachmentKind? Classify(string? fileName)
    {
        var ext = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();

        if (ImageExtensions.Contains(ext)) return RagChatAttachmentKind.Image;
        if (PdfExtensions.Contains(ext)) return RagChatAttachmentKind.Pdf;
        if (TextExtensions.Contains(ext)) return RagChatAttachmentKind.Text;

        return null;
    }
}
