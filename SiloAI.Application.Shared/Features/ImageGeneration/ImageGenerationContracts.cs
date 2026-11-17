namespace SiloAI.Application.Shared.Features;

/// <summary>An input image sent with an image-generation request.</summary>
public class ImageGenerationInputImage
{
    public byte[] Data { get; set; } = [];
    public string MediaType { get; set; } = "image/png";
}

/// <summary>
/// Instructions (the active <see cref="RagDocType.ImageGeneration"/> RagInstructions for
/// <see cref="Key"/>) + a text request + optional input images in, generated image(s) out.
/// No knowledge-base retrieval is involved.
/// </summary>
public class ImageGenerationCommand : IRequest<ImageGenerationResponse>
{
    public string Prompt { get; set; } = string.Empty;
    public List<ImageGenerationInputImage> Images { get; set; } = [];
    public string? Key { get; set; }

    /// <summary>Client-supplied id making a retried request idempotent (never charged twice).</summary>
    public Guid? RequestId { get; set; }

    public int? CustomerId { get; set; }
}

public class GeneratedImageDto
{
    public string DataBase64 { get; set; } = string.Empty;
    public string MediaType { get; set; } = "image/png";
}

public class ImageGenerationResponse
{
    public List<GeneratedImageDto> Images { get; set; } = [];

    /// <summary>Any accompanying text the model returned (may be empty).</summary>
    public string? Text { get; set; }
}
