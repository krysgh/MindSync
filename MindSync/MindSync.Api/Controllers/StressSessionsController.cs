using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindSync.Api.Common;
using MindSync.Application.Features.StressSessions.Commands.CompleteStressSession;
using MindSync.Application.Features.StressSessions.Commands.CreateStressSession;
using MindSync.Application.Features.StressSessions.Queries.GetStressSessionsByUserId;

namespace MindSync.Api.Controllers;

[Authorize]
[Route("api/sessions")]
[ApiController]
public class StressSessionsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStressSessionCommandRequest command)
    {
        var sessionId = await mediator.Send(command);
        return Ok(ApiResponse<Guid>.Ok(sessionId, "Sessão criada com sucesso."));
    }

    [HttpPatch("{id:guid}/complete")]
    public async Task<IActionResult> CompleteSession(
        Guid id,
        [FromBody] CompleteStressSessionCommandRequest command)
    {
        await mediator.Send<Unit>(new CompleteStressSessionCommandRequest(id, command.StressLevelAfter));
        return Ok(ApiResponse<object>.Ok("Sessão finalizada com sucesso."));
    }

    [HttpGet("users/{userId:guid}")]
    public async Task<IActionResult> GetByUserId(Guid userId)
    {
        var sessions = await mediator.Send(new GetStressSessionsByUserIdQueryRequest(userId));
        return Ok(ApiResponse<IEnumerable<GetStressSessionsByUserIdQueryResponse>>.Ok(sessions));
    }
}