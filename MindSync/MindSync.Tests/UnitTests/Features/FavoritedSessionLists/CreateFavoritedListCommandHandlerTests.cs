using FluentAssertions;
using MindSync.Application.Common.Interfaces;
using MindSync.Application.Features.FavoritedSessionLists.Commands.CreateFavoritedList;
using MindSync.Domain.Entities;
using MindSync.Domain.Exceptions;
using MindSync.Domain.Interfaces;
using NSubstitute;

namespace MindSync.Tests.UnitTests.Features.FavoritedSessionLists;

public class CreateFavoritedListCommandHandlerTests
{
    private readonly IFavoritedListRepository _repositoryMock;
    private readonly ICurrentUserService _currentUserServiceMock;
    private readonly CreateFavoritedListCommandHandler _handler;

    public CreateFavoritedListCommandHandlerTests()
    {
        _repositoryMock = Substitute.For<IFavoritedListRepository>();
        _currentUserServiceMock = Substitute.For<ICurrentUserService>();
        _handler = new CreateFavoritedListCommandHandler(_repositoryMock, _currentUserServiceMock);
    }

    [Fact]
    public async Task Handle_ShouldCreateList_WhenNameIsUniqueForUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateFavoritedListCommandRequest("Nova Lista Foco");

        _currentUserServiceMock.UserId.Returns(userId);
        _repositoryMock.ExistsByNameAndUserIdAsync(command.Name, userId, Arg.Any<CancellationToken>())
                       .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        await _repositoryMock.Received(1).AddAsync(
            Arg.Is<FavoritedList>(l => l.Name == command.Name && l.UserId == userId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenNameAlreadyExistsForUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateFavoritedListCommandRequest("Lista Duplicada");

        _currentUserServiceMock.UserId.Returns(userId);
        _repositoryMock.ExistsByNameAndUserIdAsync(command.Name, userId, Arg.Any<CancellationToken>())
                       .Returns(true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
                 .WithMessage($"Você já possui uma lista de favoritos com o nome '{command.Name}'.");

        await _repositoryMock.DidNotReceive().AddAsync(Arg.Any<FavoritedList>(), Arg.Any<CancellationToken>());
    }
}