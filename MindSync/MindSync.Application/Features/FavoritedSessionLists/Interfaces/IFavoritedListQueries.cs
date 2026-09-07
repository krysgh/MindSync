using MindSync.Application.Features.FavoritedSessionLists.Queries.GetFavoritedListById;
using MindSync.Application.Features.FavoritedSessionLists.Queries.GetFavoritedListstByUserId;

namespace MindSync.Application.Features.FavoritedSessionLists.Interfaces;

public interface IFavoritedListQueries
{
    Task<IEnumerable<GetFavoritedListsByUserIdQueryResponse>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<GetFavoritedListByIdQueryResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}