using MediatR;

namespace MindSync.Application.Features.FavoritedSessionLists.Queries.GetFavoritedListById;

public record GetFavoritedListByIdQueryRequest(Guid Id) : IRequest<GetFavoritedListByIdQueryResponse?>;