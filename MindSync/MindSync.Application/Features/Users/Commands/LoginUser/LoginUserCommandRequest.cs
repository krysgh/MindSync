using MediatR;

namespace MindSync.Application.Features.Users.Commands.LoginUser;

public record LoginUserCommandRequest(
    string Email,
    string Password
) : IRequest<LoginUserCommandResponse>;
