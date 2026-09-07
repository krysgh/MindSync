using MediatR;
using MindSync.Application.Common.Exceptions;
using MindSync.Application.Common.Interfaces;
using MindSync.Domain.Interfaces;

namespace MindSync.Application.Features.FavoritedSessionLists.Commands.DeleteFavoritedList;

public class DeleteFavoritedListCommandHandler(
    IFavoritedListRepository repository,
    ICurrentUserService currentUserService) : IRequestHandler<DeleteFavoritedListCommandRequest, Unit>
{
    public async Task<Unit> Handle(
        DeleteFavoritedListCommandRequest request,
        CancellationToken cancellationToken)
    {
        var list = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (list is null || list.UserId != currentUserService.UserId)
            throw new NotFoundException("Lista de favoritos não encontrada.");

        await repository.DeleteAsync(list.Id, cancellationToken);

        return Unit.Value;
    }
}