using MediatR;

namespace MindSync.Application.Features.FavoritedSessionLists.Commands.DeleteFavoritedList;

public record DeleteFavoritedListCommandRequest(Guid Id) : IRequest<Unit>;