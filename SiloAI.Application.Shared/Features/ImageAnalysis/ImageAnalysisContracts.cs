namespace SiloAI.Application.Shared.Features;

public class ImageAnalysisInputImage
{
    public byte[] Data { get; set; } = [];
    public string MediaType { get; set; } = "image/png";
}

/// <summary>
/// Instructions (the active <see cref="RagDocType.ImageAnalysis"/> RagInstructions for
/// <see cref="Key"/>) + an optional text request + one or more images in, a text analysis out.
/// No knowledge-base retrieval is involved.
/// </summary>
public class ImageAnalysisCommand : IRequest<ImageAnalysisResponse>
{
    /// <summary>Optional per-request question/task. May be empty when the instructions say it all.</summary>
    public string? Prompt { get; set; }
    public List<ImageAnalysisInputImage> Images { get; set; } = [];
    public string? Key { get; set; }

    /// <summary>Client-supplied id making a retried request idempotent (never charged twice).</summary>
    public Guid? RequestId { get; set; }

    public int? CustomerId { get; set; }
}

public class ImageAnalysisResponse
{
    public string Text { get; set; } = string.Empty;
    public ChatTokenUsageDto TokenUsage { get; set; } = new();
}
