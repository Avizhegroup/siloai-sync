using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SiloAI.Api.Controllers;

[ApiController]
[Route("admin/ai-models")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class AiModelsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetModels(CancellationToken cancellationToken)
        => Ok(await mediator.Send(new GetAiModelsQuery(), cancellationToken));

    [HttpPost]
    public async Task<IActionResult> UpsertModel([FromBody] UpsertAiModelRequest request, CancellationToken cancellationToken)
        => Ok(await mediator.Send(new UpsertAiModelCommand { Model = request }, cancellationToken));

    [HttpPost("{id:guid}/active")]
    public async Task<IActionResult> SetActive(Guid id, [FromBody] bool isActive, CancellationToken cancellationToken)
        => Ok(await mediator.Send(new SetAiModelActiveCommand { Id = id, IsActive = isActive }, cancellationToken));

    [HttpPost("{id:guid}/set-default-rag")]
    public async Task<IActionResult> SetDefaultRag(Guid id, CancellationToken cancellationToken)
        => Ok(await mediator.Send(new SetDefaultRagModelCommand { AiModelId = id }, cancellationToken));

    [HttpGet("assignments")]
    public async Task<IActionResult> GetAssignments([FromQuery] int? customerId, CancellationToken cancellationToken)
        => Ok(await mediator.Send(new GetModelAssignmentsQuery { CustomerId = customerId }, cancellationToken));

    [HttpPost("assignments")]
    public async Task<IActionResult> SetAssignment([FromBody] SetModelAssignmentRequest request, CancellationToken cancellationToken)
        => Ok(await mediator.Send(new SetModelAssignmentCommand { Request = request }, cancellationToken));

    [HttpDelete("assignments/{customerId:int}/{feature}")]
    public async Task<IActionResult> ClearAssignment(int customerId, UsageFeature feature, CancellationToken cancellationToken)
        => Ok(await mediator.Send(new ClearModelAssignmentCommand { CustomerId = customerId, Feature = feature }, cancellationToken));
}
