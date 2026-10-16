using MediatR;

namespace MindSync.Application.Features.FavoritedSessionLists.Commands.RemoveFavoritedSession;

public record RemoveFavoritedSessionCommandRequest(Guid FavoritedSessionId) : IRequest<Unit>;