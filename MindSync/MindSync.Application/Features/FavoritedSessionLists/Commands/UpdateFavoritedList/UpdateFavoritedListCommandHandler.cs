using MediatR;
using MindSync.Application.Common.Exceptions;
using MindSync.Application.Common.Interfaces;
using MindSync.Domain.Exceptions;
using MindSync.Domain.Interfaces;

namespace MindSync.Application.Features.FavoritedSessionLists.Commands.UpdateFavoritedList;

public class UpdateFavoritedListCommandHandler(
    IFavoritedListRepository repository,
    ICurrentUserService currentUserService) : IRequestHandler<UpdateFavoritedListCommandRequest, Unit>
{
    public async Task<Unit> Handle(
        UpdateFavoritedListCommandRequest request,
        CancellationToken cancellationToken)
    {
        var list = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (list is null || list.UserId != currentUserService.UserId)
            throw new NotFoundException($"Lista de favoritos não encontrada.");

        if (list.Name != request.Name && await repository.ExistsByNameAndUserIdAsync(request.Name, currentUserService.UserId, cancellationToken))
            throw new DomainException($"Você já possui uma lista de favoritos com o nome '{request.Name}'.");

        list.UpdateName(request.Name);
        await repository.UpdateAsync(list, cancellationToken);

        return Unit.Value;
    }
}