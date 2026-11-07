using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiloAI.Agent.ImageGeneration;
using SiloAI.Api.Auth;
using SiloAI.Application.Api;
using SiloAI.Shared;

namespace SiloAI.Api.Controllers;

public class ImageAnalysisForm
{
    public string? Prompt { get; set; }
    public string? Key { get; set; }
    public Guid? RequestId { get; set; }
    public List<IFormFile> Images { get; set; } = [];
}

/// <summary>
/// Instructions + optional text + images in, a text analysis out. JWT (admin panel) or API key
/// (customers), same as the RAG chat endpoints; the customer, when present, is billed through the ledger.
/// </summary>
[ApiController]
[Route("api/image-analysis")]
[Authorize(AuthenticationSchemes =
    $"{JwtBearerDefaults.AuthenticationScheme},{ApiKeyAuthenticationHandler.SchemeName}")]
public class ImageAnalysisController(IMediator mediator) : ControllerBase
{
    public const int MaxImages = 8;
    public const long MaxImageBytes = 10 * 1024 * 1024;

    [HttpPost]
    [RequestSizeLimit(MaxImages * MaxImageBytes + 1024 * 1024)]
    public async Task<IActionResult> Analyze([FromForm] ImageAnalysisForm form, CancellationToken cancellationToken)
    {
        if (form.Images.Count == 0)
            return BadRequest(new { message = "At least one image is required." });

        if (form.Images.Count > MaxImages)
            return BadRequest(new { message = $"At most {MaxImages} images are allowed." });

        var images = new List<ImageAnalysisInputImage>();

        foreach (var file in form.Images)
        {
            if (file.Length == 0 || file.Length > MaxImageBytes)
                return BadRequest(new { message = $"Each image must be between 1 byte and {MaxImageBytes / 1024 / 1024} MB." });

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, cancellationToken);
            var data = ms.ToArray();

            // Trust the bytes, not the declared content type (browsers send "" / wrong types).
            var mediaType = ImageFormat.Detect(data);
            if (mediaType is null)
                return BadRequest(new { message = "Unsupported image type. Use PNG, JPEG or WebP." });

            images.Add(new ImageAnalysisInputImage { Data = data, MediaType = mediaType });
        }

        try
        {
            var result = await mediator.Send(new ImageAnalysisCommand
            {
                Prompt = form.Prompt?.Trim(),
                Images = images,
                Key = form.Key,
                RequestId = form.RequestId,
                CustomerId = int.TryParse(User.GetCustomerId(), out var customerId) ? customerId : null
            }, cancellationToken);

            return Ok(result);
        }
        catch (InsufficientCreditException)
        {
            return StatusCode(402, new { message = "اعتبار مشتری به پایان رسیده است." });
        }
        catch (InvalidOperationException ex)
        {
            // Misconfiguration (no model assigned / wrong capabilities / nothing to analyze / empty answer).
            return StatusCode(422, new { message = ex.Message });
        }
    }
}
