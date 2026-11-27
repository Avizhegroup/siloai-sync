using Microsoft.AspNetCore.Http;
using SiloAI.Application.Shared;
using SiloAI.Application.Shared.Features;

namespace SiloAI.Application.Api.Features;

/// <summary>multipart form of POST api/image-generation. Validated and converted by its handler.</summary>
public class GenerateImageFormCommand : IRequest<ImageGenerationResponse>
{
    public string? Prompt { get; set; }
    public string? Key { get; set; }
    public Guid? RequestId { get; set; }
    public List<IFormFile> Images { get; set; } = [];
}

public class GenerateImageFormCommandHandler(IMediator mediator)
    : IRequestHandler<GenerateImageFormCommand, ImageGenerationResponse>
{
    public async Task<ImageGenerationResponse> Handle(GenerateImageFormCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
            throw new BadRequestException("Prompt is required.");

        if (request.Images.Count > ImageUploadLimits.MaxImages)
            throw new BadRequestException($"At most {ImageUploadLimits.MaxImages} input images are allowed.");

        var images = new List<ImageGenerationInputImage>();
        foreach (var file in request.Images)
        {
            var (data, mediaType) = await ImageUploadLimits.ReadAsync(file, cancellationToken);
            images.Add(new ImageGenerationInputImage { Data = data, MediaType = mediaType });
        }

        // The caller (customer id) is stamped onto this command by the pipeline behavior.
        return await mediator.Send(new ImageGenerationCommand
        {
            Prompt = request.Prompt.Trim(),
            Images = images,
            Key = request.Key,
            RequestId = request.RequestId
        }, cancellationToken);
    }
}

/// <summary>multipart form of POST api/image-analysis.</summary>
public class AnalyzeImageFormCommand : IRequest<ImageAnalysisResponse>
{
    public string? Prompt { get; set; }
    public string? Key { get; set; }
    public Guid? RequestId { get; set; }
    public List<IFormFile> Images { get; set; } = [];
}

public class AnalyzeImageFormCommandHandler(IMediator mediator)
    : IRequestHandler<AnalyzeImageFormCommand, ImageAnalysisResponse>
{
    public async Task<ImageAnalysisResponse> Handle(AnalyzeImageFormCommand request, CancellationToken cancellationToken)
    {
        if (request.Images.Count == 0)
            throw new BadRequestException("At least one image is required.");

        if (request.Images.Count > ImageUploadLimits.MaxImages)
            throw new BadRequestException($"At most {ImageUploadLimits.MaxImages} images are allowed.");

        var images = new List<ImageAnalysisInputImage>();
        foreach (var file in request.Images)
        {
            var (data, mediaType) = await ImageUploadLimits.ReadAsync(file, cancellationToken);
            images.Add(new ImageAnalysisInputImage { Data = data, MediaType = mediaType });
        }

        return await mediator.Send(new ImageAnalysisCommand
        {
            Prompt = request.Prompt?.Trim(),
            Images = images,
            Key = request.Key,
            RequestId = request.RequestId
        }, cancellationToken);
    }
}

/// <summary>multipart form of POST api/image/ocr.</summary>
public class OcrFormCommand : IRequest<OcrResponse>
{
    public IFormFile? ImageData { get; set; }
    public string? MediaType { get; set; }
    public RagDocType DocType { get; set; }
    public string? Key { get; set; }
    public Guid? RequestId { get; set; }
}

public class OcrFormCommandHandler(IMediator mediator) : IRequestHandler<OcrFormCommand, OcrResponse>
{
    public async Task<OcrResponse> Handle(OcrFormCommand request, CancellationToken cancellationToken)
    {
        if (request.ImageData is null || request.ImageData.Length == 0)
            throw new BadRequestException("Image data is required.");

        using var ms = new MemoryStream();
        await request.ImageData.CopyToAsync(ms, cancellationToken);

        return await mediator.Send(new OcrCommand
        {
            ImageData = ms.ToArray(),
            MediaType = request.MediaType ?? string.Empty,
            DocType = request.DocType,
            Key = request.Key ?? string.Empty,
            RequestId = request.RequestId
        }, cancellationToken);
    }
}
