using MediatR;

namespace MindSync.Application.Features.FavoritedSessionLists.Commands.CreateFavoritedList;

public record CreateFavoritedListCommandRequest(string Name) : IRequest<Guid>;