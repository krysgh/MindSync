namespace MindSync.Application.Features.FavoritedSessionLists.Queries.GetFavoritedListById;

public record FavoritedSessionItemResponse(
    Guid Id,
    Guid StressSessionId,
    string CustomName,
    string TargetFrequency,
    int StressLevelBefore,
    int? StressLevelAfter,
    DateTime FavoritedAt);

public record GetFavoritedListByIdQueryResponse(
    Guid Id,
    Guid UserId,
    string Name,
    DateTime CreatedAt,
    List<FavoritedSessionItemResponse> Items)
{
    public GetFavoritedListByIdQueryResponse(Guid id, Guid userId, string name, DateTime createdAt)
        : this(id, userId, name, createdAt, []) { }
}