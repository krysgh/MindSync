using MediatR;
using MindSync.Application.Common.Exceptions;
using MindSync.Application.Common.Interfaces;
using MindSync.Domain.Entities;
using MindSync.Domain.Exceptions;
using MindSync.Domain.Interfaces;

namespace MindSync.Application.Features.FavoritedSessionLists.Commands.AddFavoritedSession;

public class AddFavoritedSessionCommandHandler(
    IFavoritedListRepository repository,
    ICurrentUserService currentUserService) : IRequestHandler<AddFavoritedSessionCommandRequest, Guid>
{
    public async Task<Guid> Handle(
        AddFavoritedSessionCommandRequest request,
        CancellationToken cancellationToken)
    {
        var list = await repository.GetByIdAsync(request.FavoritedListId, cancellationToken);

        if (list is null || list.UserId != currentUserService.UserId)
            throw new NotFoundException("Lista de favoritos não encontrada.");

        var nameExists = await repository.ExistsByCustomNameAndListIdAsync(
            request.CustomName, list.Id, excludingSessionId: null, cancellationToken);

        if (nameExists)
            throw new DomainException($"Já existe uma sessão chamada '{request.CustomName}' nesta lista.");

        var item = new FavoritedSession(list.Id, request.StressSessionId, request.CustomName);

        await repository.AddSessionAsync(item, cancellationToken);

        return item.Id;
    }
}