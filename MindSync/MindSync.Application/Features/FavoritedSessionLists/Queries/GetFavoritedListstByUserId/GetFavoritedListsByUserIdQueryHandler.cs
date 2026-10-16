using MediatR;
using MindSync.Application.Features.FavoritedSessionLists.Interfaces;

namespace MindSync.Application.Features.FavoritedSessionLists.Queries.GetFavoritedListstByUserId;

public class GetFavoritedListsByUserIdQueryHandler(IFavoritedListQueries queries)
    : IRequestHandler<GetFavoritedListsByUserIdQueryRequest, IEnumerable<GetFavoritedListsByUserIdQueryResponse>>
{
    public async Task<IEnumerable<GetFavoritedListsByUserIdQueryResponse>> Handle(
        GetFavoritedListsByUserIdQueryRequest request,
        CancellationToken cancellationToken)
    {
        return await queries.GetByUserIdAsync(request.UserId, cancellationToken);
    }
}