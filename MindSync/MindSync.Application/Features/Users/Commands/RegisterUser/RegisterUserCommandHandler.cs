using MediatR;
using MindSync.Application.Common.Interfaces;
using MindSync.Domain.Entities;
using MindSync.Domain.Exceptions;
using MindSync.Domain.Interfaces;
using MindSync.Domain.ValueObjects;

namespace MindSync.Application.Features.Users.Commands.RegisterUser;

public class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher) : IRequestHandler<RegisterUserCommandRequest, Guid>
{
    public async Task<Guid> Handle(
        RegisterUserCommandRequest request,
        CancellationToken cancellationToken)
    {
        var emailVo = new Email(request.Email);

        var emailExists = await userRepository.ExistsByEmailAsync(emailVo);
        if (emailExists)
            throw new DomainException("Não foi possível concluir o cadastro com os dados informados.");

        var passwordHash = passwordHasher.Hash(request.Password);

        var user = new User(
            id: Guid.NewGuid(),
            firstName: request.FirstName,
            lastName: request.LastName,
            email: emailVo,
            passwordHash: passwordHash,
            createdAt: DateTime.UtcNow
        );

        await userRepository.InsertAsync(user);

        return user.Id;
    }
}
