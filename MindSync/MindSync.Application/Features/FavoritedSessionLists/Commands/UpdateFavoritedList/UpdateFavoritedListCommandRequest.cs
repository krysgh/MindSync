using MediatR;

namespace MindSync.Application.Features.FavoritedSessionLists.Commands.UpdateFavoritedList;

public record UpdateFavoritedListCommandRequest(
    Guid Id,
    string Name) : IRequest<Unit>;