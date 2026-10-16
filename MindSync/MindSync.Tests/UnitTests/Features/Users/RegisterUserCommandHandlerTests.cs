using FluentAssertions;
using MindSync.Application.Common.Interfaces;
using MindSync.Application.Features.Users.Commands.RegisterUser;
using MindSync.Domain.Entities;
using MindSync.Domain.Exceptions;
using MindSync.Domain.Interfaces;
using MindSync.Domain.ValueObjects;
using NSubstitute;

namespace MindSync.Tests.UnitTests.Features.Users;

public class RegisterUserCommandHandlerTests
{
    private readonly IUserRepository _userRepositoryMock = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasherMock = Substitute.For<IPasswordHasher>();
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _handler = new RegisterUserCommandHandler(_userRepositoryMock, _passwordHasherMock);
    }

    [Fact]
    public async Task Handle_ShouldCreateUser_WhenEmailDoesntExistYet()
    {
        // Arrange
        var command = new RegisterUserCommandRequest("Ana", "Silva", "ana@teste.com", "SenhaForte123");

        _userRepositoryMock.ExistsByEmailAsync(Arg.Any<Email>()).Returns(false);
        _passwordHasherMock.Hash(command.Password).Returns("hash-seguro");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        await _userRepositoryMock.Received(1).InsertAsync(
            Arg.Is<User>(u => u.FirstName == "Ana" && u.LastName == "Silva" && u.PasswordHash == "hash-seguro"));
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WithNeutralMessage_WhenEmailAlreadyExists()
    {
        // Arrange
        var command = new RegisterUserCommandRequest("Ana", "Silva", "ana@teste.com", "SenhaForte123");
        _userRepositoryMock.ExistsByEmailAsync(Arg.Any<Email>()).Returns(true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
                 .WithMessage("Não foi possível concluir o cadastro com os dados informados.");

        await _userRepositoryMock.DidNotReceive().InsertAsync(Arg.Any<User>());
    }
}