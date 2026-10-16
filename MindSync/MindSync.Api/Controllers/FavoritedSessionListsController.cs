using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindSync.Api.Common;
using MindSync.Application.Common.Interfaces;
using MindSync.Application.Features.FavoritedSessionLists.Commands.AddFavoritedSession;
using MindSync.Application.Features.FavoritedSessionLists.Commands.CreateFavoritedList;
using MindSync.Application.Features.FavoritedSessionLists.Commands.DeleteFavoritedList;
using MindSync.Application.Features.FavoritedSessionLists.Commands.RemoveFavoritedSession;
using MindSync.Application.Features.FavoritedSessionLists.Commands.UpdateFavoritedList;
using MindSync.Application.Features.FavoritedSessionLists.Commands.UpdateFavoritedSession;
using MindSync.Application.Features.FavoritedSessionLists.Queries.GetFavoritedListById;
using MindSync.Application.Features.FavoritedSessionLists.Queries.GetFavoritedListstByUserId;
using MindSync.Domain.Exceptions;

namespace MindSync.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/favoritedLists")]
public class FavoritedSessionListsController(
    IMediator mediator,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet("mine")]
    public async Task<IActionResult> GetMyLists(CancellationToken cancellationToken)
    {
        var query = new GetFavoritedListsByUserIdQueryRequest(currentUserService.UserId);
        var response = await mediator.Send(query, cancellationToken);

        return Ok(ApiResponse<IEnumerable<GetFavoritedListsByUserIdQueryResponse>>.Ok(response));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetFavoritedListByIdQueryRequest(id), cancellationToken);
        return Ok(ApiResponse<GetFavoritedListByIdQueryResponse>.Ok(response!));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateFavoritedListCommandRequest command,
        CancellationToken cancellationToken)
    {
        var createdId = await mediator.Send(command, cancellationToken);
        var apiResponse = ApiResponse<Guid>.Ok(createdId, "Lista criada com sucesso.");

        return CreatedAtAction(nameof(GetById), new { id = createdId }, apiResponse);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateFavoritedListCommandRequest command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            throw new DomainException("O ID da URL não coincide com o ID do corpo da requisição.");

        await mediator.Send<Unit>(command, cancellationToken);
        return Ok(ApiResponse<object>.Ok("Lista atualizada com sucesso."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await mediator.Send<Unit>(new DeleteFavoritedListCommandRequest(id), cancellationToken);
        return Ok(ApiResponse<object>.Ok("Lista removida com sucesso."));
    }

    [HttpPost("{id:guid}/sessions")]
    public async Task<IActionResult> AddSession(
        Guid id,
        [FromBody] AddFavoritedSessionCommandRequest command,
        CancellationToken cancellationToken)
    {
        if (id != command.FavoritedListId)
            throw new DomainException("O ID da lista na URL não coincide com o corpo da requisição.");

        var sessionId = await mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<Guid>.Ok(sessionId, "Sessão favoritada adicionada com sucesso."));
    }

    [HttpPut("sessions/{favoritedSessionId:guid}")]
    public async Task<IActionResult> UpdateSession(
        Guid favoritedSessionId,
        [FromBody] UpdateFavoritedSessionCommandRequest command,
        CancellationToken cancellationToken)
    {
        if (favoritedSessionId != command.Id)
            throw new DomainException("O ID da sessão favoritada na URL não coincide com o corpo da requisição.");

        await mediator.Send<Unit>(command, cancellationToken);
        return Ok(ApiResponse<object>.Ok("Nome customizado da sessão atualizado com sucesso."));
    }

    [HttpDelete("sessions/{favoritedSessionId:guid}")]
    public async Task<IActionResult> RemoveSession(
        Guid favoritedSessionId,
        CancellationToken cancellationToken)
    {
        await mediator.Send<Unit>(new RemoveFavoritedSessionCommandRequest(favoritedSessionId), cancellationToken);
        return Ok(ApiResponse<object>.Ok("Sessão favoritada removida com sucesso."));
    }
}