using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiloAI.Api.Auth;
using SiloAI.Application.Api;
using SiloAI.Shared;

namespace SiloAI.Api.Controllers;

public class ImageGenerationForm
{
    public string? Prompt { get; set; }
    public string? Key { get; set; }
    public Guid? RequestId { get; set; }
    public List<IFormFile> Images { get; set; } = [];
}

/// <summary>
/// Instructions + text + images in, generated image out. JWT (admin panel) or API key (customers),
/// same as the RAG chat endpoints; the customer, when present, is billed through the ledger.
/// </summary>
[ApiController]
[Route("api/image-generation")]
[Authorize(AuthenticationSchemes =
    $"{JwtBearerDefaults.AuthenticationScheme},{ApiKeyAuthenticationHandler.SchemeName}")]
public class ImageGenerationController(IMediator mediator) : ControllerBase
{
    public const int MaxImages = 8;
    public const long MaxImageBytes = 10 * 1024 * 1024;

    private static readonly string[] AllowedTypes = ["image/png", "image/jpeg", "image/webp", "image/gif"];

    [HttpPost]
    [RequestSizeLimit(MaxImages * MaxImageBytes + 1024 * 1024)]
    public async Task<IActionResult> Generate([FromForm] ImageGenerationForm form, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(form.Prompt))
            return BadRequest(new { message = "Prompt is required." });

        if (form.Images.Count > MaxImages)
            return BadRequest(new { message = $"At most {MaxImages} input images are allowed." });

        var images = new List<ImageGenerationInputImage>();

        foreach (var file in form.Images)
        {
            if (file.Length == 0 || file.Length > MaxImageBytes)
                return BadRequest(new { message = $"Each image must be between 1 byte and {MaxImageBytes / 1024 / 1024} MB." });

            if (!AllowedTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
                return BadRequest(new { message = "Unsupported image type. Use PNG, JPEG, WebP or GIF." });

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, cancellationToken);
            images.Add(new ImageGenerationInputImage { Data = ms.ToArray(), MediaType = file.ContentType.ToLowerInvariant() });
        }

        try
        {
            var result = await mediator.Send(new ImageGenerationCommand
            {
                Prompt = form.Prompt.Trim(),
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
            // Misconfiguration (no model assigned / wrong capabilities / no image returned).
            return StatusCode(422, new { message = ex.Message });
        }
    }
}
