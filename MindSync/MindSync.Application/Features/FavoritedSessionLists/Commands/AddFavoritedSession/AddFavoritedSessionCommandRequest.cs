using MediatR;

namespace MindSync.Application.Features.FavoritedSessionLists.Commands.AddFavoritedSession;

public record AddFavoritedSessionCommandRequest(
    Guid FavoritedListId,
    Guid StressSessionId,
    string CustomName) : IRequest<Guid>;
