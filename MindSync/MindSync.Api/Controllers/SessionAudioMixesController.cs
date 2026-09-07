using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindSync.Api.Common;
using MindSync.Application.Features.SessionAudioMixes.Commands.GetOrCreateSessionAudioMix;

namespace MindSync.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/mixes")]
public class SessionAudioMixesController(IMediator mediator) : ControllerBase
{
    [HttpPost("sessions/{stressSessionId:guid}")]
    public async Task<IActionResult> Resolve(
        Guid stressSessionId,
        CancellationToken cancellationToken)
    {
        var command = new GetOrCreateSessionAudioMixCommandRequest(stressSessionId);
        var response = await mediator.Send(command, cancellationToken);

        return Ok(ApiResponse<SessionAudioMixResponse>.Ok(response));
    }
}