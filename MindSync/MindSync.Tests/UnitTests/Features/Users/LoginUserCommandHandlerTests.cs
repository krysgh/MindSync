using FluentAssertions;
using MindSync.Application.Common.Exceptions;
using MindSync.Application.Common.Interfaces;
using MindSync.Application.Features.Users.Commands.LoginUser;
using MindSync.Domain.Entities;
using MindSync.Domain.Interfaces;
using MindSync.Domain.ValueObjects;
using NSubstitute;

namespace MindSync.Tests.UnitTests.Features.Users;

public class LoginUserCommandHandlerTests
{
    private readonly IUserRepository _userRepositoryMock = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasherMock = Substitute.For<IPasswordHasher>();
    private readonly ITokenService _tokenServiceMock = Substitute.For<ITokenService>();
    private readonly LoginUserCommandHandler _handler;

    public LoginUserCommandHandlerTests()
    {
        _handler = new LoginUserCommandHandler(_userRepositoryMock, _passwordHasherMock, _tokenServiceMock);
    }

    private static User CreateUser(string passwordHash = "hash-valido") =>
        new(Guid.NewGuid(), "Ana", "Silva", new Email("ana@teste.com"), passwordHash, DateTime.UtcNow);

    [Fact]
    public async Task Handle_ShouldReturnTokenAndUserId_WhenCredentialsAreValid()
    {
        // Arrange
        var user = CreateUser();
        var command = new LoginUserCommandRequest("ana@teste.com", "senha-correta");

        _userRepositoryMock.GetByEmailAsync(Arg.Any<Email>()).Returns(user);
        _passwordHasherMock.Verify(command.Password, user.PasswordHash).Returns(true);
        _tokenServiceMock.GenerateToken(user).Returns("jwt-gerado");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Token.Should().Be("jwt-gerado");
        result.UserId.Should().Be(user.Id.ToString());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WithGenericMessage_WhenUserDoesNotExist()
    {
        // Arrange
        var command = new LoginUserCommandRequest("naoexiste@teste.com", "qualquer-senha");
        _userRepositoryMock.GetByEmailAsync(Arg.Any<Email>()).Returns((User?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
                 .WithMessage("E-mail ou senha incorretos.");
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WithTheSameMessage_WhenPasswordIsIncorrect()
    {
        // Arrange
        var user = CreateUser();
        var command = new LoginUserCommandRequest("ana@teste.com", "senha-errada");

        _userRepositoryMock.GetByEmailAsync(Arg.Any<Email>()).Returns(user);
        _passwordHasherMock.Verify(command.Password, user.PasswordHash).Returns(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
                 .WithMessage("E-mail ou senha incorretos.");

        _tokenServiceMock.DidNotReceive().GenerateToken(Arg.Any<User>());
    }
}