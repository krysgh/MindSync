using MediatR;

namespace MindSync.Application.Features.FavoritedSessionLists.Commands.UpdateFavoritedSession;

public record UpdateFavoritedSessionCommandRequest(
    Guid Id,
    string CustomName) : IRequest<Unit>;