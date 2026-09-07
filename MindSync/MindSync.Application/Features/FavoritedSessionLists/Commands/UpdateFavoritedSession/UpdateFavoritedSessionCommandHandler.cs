using MediatR;
using MindSync.Application.Common.Exceptions;
using MindSync.Application.Common.Interfaces;
using MindSync.Domain.Exceptions;
using MindSync.Domain.Interfaces;

namespace MindSync.Application.Features.FavoritedSessionLists.Commands.UpdateFavoritedSession;

public class UpdateFavoritedSessionCommandHandler(
    IFavoritedListRepository repository,
    ICurrentUserService currentUserService) : IRequestHandler<UpdateFavoritedSessionCommandRequest, Unit>
{
    public async Task<Unit> Handle(
        UpdateFavoritedSessionCommandRequest request,
        CancellationToken cancellationToken)
    {
        var item = await repository.GetSessionByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Sessão favoritada não encontrada.");

        var list = await repository.GetByIdAsync(item.FavoritedListId, cancellationToken);

        if (list is null || list.UserId != currentUserService.UserId)
            throw new NotFoundException("Sessão favoritada não encontrada.");

        var nameExists = await repository.ExistsByCustomNameAndListIdAsync(
            request.CustomName, item.FavoritedListId, excludingSessionId: item.Id, cancellationToken);

        if (nameExists)
            throw new DomainException($"Já existe uma sessão chamada '{request.CustomName}' nesta lista.");

        item.UpdateCustomName(request.CustomName);

        await repository.UpdateSessionAsync(item, cancellationToken);

        return Unit.Value;
    }
}