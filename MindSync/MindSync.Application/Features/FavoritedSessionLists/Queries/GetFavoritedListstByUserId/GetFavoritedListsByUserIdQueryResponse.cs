namespace MindSync.Application.Features.FavoritedSessionLists.Queries.GetFavoritedListstByUserId;

public record GetFavoritedListsByUserIdQueryResponse(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    int TotalItems);