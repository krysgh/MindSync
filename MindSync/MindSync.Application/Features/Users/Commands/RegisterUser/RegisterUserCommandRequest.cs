using MediatR;

namespace MindSync.Application.Features.Users.Commands.RegisterUser;

public record RegisterUserCommandRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password
) : IRequest<Guid>;
