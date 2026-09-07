using MediatR;
using MindSync.Application.Common.Interfaces;
using MindSync.Domain.Entities;
using MindSync.Domain.Exceptions;
using MindSync.Domain.Interfaces;

namespace MindSync.Application.Features.FavoritedSessionLists.Commands.CreateFavoritedList;

public class CreateFavoritedListCommandHandler(
    IFavoritedListRepository repository,
    ICurrentUserService currentUserService) : IRequestHandler<CreateFavoritedListCommandRequest, Guid>
{
    public async Task<Guid> Handle(
        CreateFavoritedListCommandRequest request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        var nameExists = await repository.ExistsByNameAndUserIdAsync(request.Name, userId, cancellationToken);
        if (nameExists)
            throw new DomainException($"Você já possui uma lista de favoritos com o nome '{request.Name}'.");

        var favoritedList = new FavoritedList(userId, request.Name);
        await repository.AddAsync(favoritedList, cancellationToken);

        return favoritedList.Id;
    }
}