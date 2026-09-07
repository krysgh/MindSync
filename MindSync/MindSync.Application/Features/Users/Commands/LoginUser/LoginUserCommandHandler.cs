using MediatR;
using MindSync.Application.Common.Exceptions;
using MindSync.Application.Common.Interfaces;
using MindSync.Domain.Interfaces;
using MindSync.Domain.ValueObjects;

namespace MindSync.Application.Features.Users.Commands.LoginUser;

public class LoginUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService) : IRequestHandler<LoginUserCommandRequest, LoginUserCommandResponse>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly ITokenService _tokenService = tokenService;

    public async Task<LoginUserCommandResponse> Handle(
        LoginUserCommandRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(new Email(request.Email)) ??
                   throw new NotFoundException("E-mail ou senha incorretos.");

        var passwordValid = _passwordHasher.Verify(request.Password, user.PasswordHash);

        if (!passwordValid)
            throw new NotFoundException("E-mail ou senha incorretos.");

        var token = _tokenService.GenerateToken(user);

        return new LoginUserCommandResponse(token, user.Id.ToString());
    }
}