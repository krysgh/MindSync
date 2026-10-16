using MediatR;
using MindSync.Application.Common.Exceptions;
using MindSync.Application.Common.Interfaces;
using MindSync.Domain.Interfaces;

namespace MindSync.Application.Features.FavoritedSessionLists.Commands.RemoveFavoritedSession;

public class RemoveFavoritedSessionCommandHandler(
    IFavoritedListRepository repository,
    ICurrentUserService currentUserService) : IRequestHandler<RemoveFavoritedSessionCommandRequest, Unit>
{
    public async Task<Unit> Handle(
        RemoveFavoritedSessionCommandRequest request,
        CancellationToken cancellationToken)
    {
        var session = await repository.GetSessionByIdAsync(request.FavoritedSessionId, cancellationToken)
            ?? throw new NotFoundException("Sessão favoritada não encontrada.");

        var list = await repository.GetByIdAsync(session.FavoritedListId, cancellationToken);

        if (list is null || list.UserId != currentUserService.UserId)
            throw new NotFoundException("Sessão favoritada não encontrada.");

        await repository.RemoveSessionAsync(session.Id, cancellationToken);

        return Unit.Value;

    }
}