using MediatR;

namespace MindSync.Application.Features.FavoritedSessionLists.Queries.GetFavoritedListstByUserId;

public record GetFavoritedListsByUserIdQueryRequest(Guid UserId)
    : IRequest<IEnumerable<GetFavoritedListsByUserIdQueryResponse>>;