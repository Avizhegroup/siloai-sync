using System.Text;
using Microsoft.AspNetCore.Http;
using SiloAI.Agent.ImageGeneration;
using SiloAI.Application.Shared;
using SiloAI.Application.Shared.Features;

namespace SiloAI.Application.Api.Features;

/// <summary>
/// Validates chat attachments. The kind is decided by the extension, then checked against the
/// actual bytes (browsers send empty/wrong content types, and a renamed file must not slip through).
/// </summary>
public static class RagChatAttachmentReader
{
    private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    /// <summary>Validates one file's bytes and turns them into an attachment. Throws <see cref="BadRequestException"/>.</summary>
    public static RagChatAttachment Prepare(string? fileName, byte[] data)
    {
        var name = Path.GetFileName(fileName ?? string.Empty)?.Trim();

        if (string.IsNullOrEmpty(name))
            throw new BadRequestException("Every attachment needs a file name.");

        var kind = RagChatAttachmentRules.Classify(name)
            ?? throw new BadRequestException(
                $"File '{name}' has an unsupported type. Allowed extensions: {string.Join(", ", RagChatAttachmentRules.AllExtensions)}.");

        if (data.Length == 0)
            throw new BadRequestException($"File '{name}' is empty.");

        if (data.Length > RagChatAttachmentRules.MaxFileBytes)
            throw new BadRequestException(
                $"File '{name}' is larger than {RagChatAttachmentRules.MaxFileBytes / 1024 / 1024} MB.");

        var attachment = new RagChatAttachment { FileName = name, Kind = kind, SizeBytes = data.Length };

        switch (kind)
        {
            case RagChatAttachmentKind.Image:
                attachment.MediaType = ImageFormat.Detect(data)
                    ?? throw new BadRequestException($"File '{name}' is not a valid PNG, JPEG or WebP image.");
                attachment.Data = data;
                break;

            case RagChatAttachmentKind.Pdf:
                if (!LooksLikePdf(data))
                    throw new BadRequestException($"File '{name}' is not a valid PDF.");
                attachment.MediaType = "application/pdf";
                attachment.Data = data;
                break;

            default:
                attachment.MediaType = "text/plain";
                attachment.Text = DecodeText(name, data);
                break;
        }

        return attachment;
    }

    /// <summary>Enforces the per-message limits (count and combined size) on a set of attachments.</summary>
    public static void ValidateSet(IReadOnlyCollection<RagChatAttachment> attachments)
    {
        if (attachments.Count > RagChatAttachmentRules.MaxFiles)
            throw new BadRequestException($"At most {RagChatAttachmentRules.MaxFiles} files can be attached to one message.");

        var total = attachments.Sum(a => (long)a.SizeBytes);

        if (total > RagChatAttachmentRules.MaxTotalBytes)
            throw new BadRequestException(
                $"The attachments together are larger than {RagChatAttachmentRules.MaxTotalBytes / 1024 / 1024} MB.");
    }

    /// <summary>Reads and validates uploaded files. Count and declared sizes are checked before any bytes are read.</summary>
    public static async Task<List<RagChatAttachment>> ReadAsync(
        IReadOnlyCollection<IFormFile> files, CancellationToken cancellationToken)
    {
        if (files.Count > RagChatAttachmentRules.MaxFiles)
            throw new BadRequestException($"At most {RagChatAttachmentRules.MaxFiles} files can be attached to one message.");

        if (files.Sum(f => f.Length) > RagChatAttachmentRules.MaxTotalBytes)
            throw new BadRequestException(
                $"The attachments together are larger than {RagChatAttachmentRules.MaxTotalBytes / 1024 / 1024} MB.");

        var result = new List<RagChatAttachment>(files.Count);

        foreach (var file in files)
        {
            if (file.Length > RagChatAttachmentRules.MaxFileBytes)
                throw new BadRequestException(
                    $"File '{Path.GetFileName(file.FileName)}' is larger than {RagChatAttachmentRules.MaxFileBytes / 1024 / 1024} MB.");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, cancellationToken);

            result.Add(Prepare(file.FileName, ms.ToArray()));
        }

        ValidateSet(result);

        return result;
    }

    /// <summary>The model's chat text for the text files of one turn (empty when there are none).</summary>
    public static string BuildTextSection(IEnumerable<RagChatAttachment> attachments)
    {
        var sb = new StringBuilder();

        foreach (var a in attachments.Where(a => a.Kind == RagChatAttachmentKind.Text))
        {
            sb.AppendLine().AppendLine($"--- Attached file: {a.FileName} ---");
            sb.AppendLine(a.Text);
            sb.AppendLine($"--- End of {a.FileName} ---");
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>The note appended to the saved conversation text so later turns know which files were sent.</summary>
    public static string BuildHistoryNote(IReadOnlyCollection<RagChatAttachment> attachments)
        => attachments.Count == 0
            ? string.Empty
            : $"\n\n[Attached files: {string.Join(", ", attachments.Select(a => a.FileName))}]";

    private static bool LooksLikePdf(byte[] d)
        => d.Length >= 5 && d[0] == '%' && d[1] == 'P' && d[2] == 'D' && d[3] == 'F' && d[4] == '-';

    private static string DecodeText(string name, byte[] data)
    {
        if (Array.IndexOf(data, (byte)0) >= 0)
            throw new BadRequestException($"File '{name}' looks like a binary file, not text.");

        string text;

        try
        {
            // Skip a UTF-8 byte-order mark, then decode strictly so a wrongly-encoded file is rejected.
            var offset = data.Length >= 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF ? 3 : 0;
            text = StrictUtf8.GetString(data, offset, data.Length - offset);
        }
        catch (DecoderFallbackException)
        {
            throw new BadRequestException($"File '{name}' is not valid UTF-8 text.");
        }

        if (string.IsNullOrWhiteSpace(text))
            throw new BadRequestException($"File '{name}' contains no text.");

        if (text.Length > RagChatAttachmentRules.MaxTextCharsPerFile)
            text = text[..RagChatAttachmentRules.MaxTextCharsPerFile]
                + $"\n[... file '{name}' was cut after {RagChatAttachmentRules.MaxTextCharsPerFile} characters ...]";

        return text;
    }
}
