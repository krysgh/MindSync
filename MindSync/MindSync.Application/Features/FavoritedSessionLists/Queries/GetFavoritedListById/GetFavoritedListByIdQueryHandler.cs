using MediatR;
using MindSync.Application.Common.Exceptions;
using MindSync.Application.Common.Interfaces;
using MindSync.Application.Features.FavoritedSessionLists.Interfaces;

namespace MindSync.Application.Features.FavoritedSessionLists.Queries.GetFavoritedListById;

public class GetFavoritedListByIdQueryHandler(
    IFavoritedListQueries queries,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetFavoritedListByIdQueryRequest, GetFavoritedListByIdQueryResponse?>
{
    public async Task<GetFavoritedListByIdQueryResponse?> Handle(
        GetFavoritedListByIdQueryRequest request,
        CancellationToken cancellationToken)
    {
        var list = await queries.GetByIdAsync(request.Id, cancellationToken);

        return list is null || list.UserId != currentUserService.UserId
            ? throw new NotFoundException($"Lista de favoritos com o ID '{request.Id}' não foi encontrada.")
            : list;
    }
}