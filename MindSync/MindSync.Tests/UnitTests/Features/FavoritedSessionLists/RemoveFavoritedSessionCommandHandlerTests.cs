using FluentAssertions;
using MindSync.Application.Common.Exceptions;
using MindSync.Application.Common.Interfaces;
using MindSync.Application.Features.FavoritedSessionLists.Commands.RemoveFavoritedSession;
using MindSync.Domain.Entities;
using MindSync.Domain.Interfaces;
using NSubstitute;

namespace MindSync.Tests.UnitTests.Features.FavoritedSessionLists;

public class RemoveFavoritedSessionCommandHandlerTests
{
    private readonly IFavoritedListRepository _repositoryMock = Substitute.For<IFavoritedListRepository>();
    private readonly ICurrentUserService _currentUserServiceMock = Substitute.For<ICurrentUserService>();
    private readonly RemoveFavoritedSessionCommandHandler _handler;

    public RemoveFavoritedSessionCommandHandlerTests()
    {
        _handler = new RemoveFavoritedSessionCommandHandler(_repositoryMock, _currentUserServiceMock);
    }

    [Fact]
    public async Task Handle_ShouldRemove_WhenListBelongsToLoggedInUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var list = new FavoritedList(userId, "Relaxing");
        var item = new FavoritedSession(list.Id, Guid.NewGuid(), "Sessão de Sexta");
        var command = new RemoveFavoritedSessionCommandRequest(item.Id);

        _currentUserServiceMock.UserId.Returns(userId);
        _repositoryMock.GetSessionByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);
        _repositoryMock.GetByIdAsync(list.Id, Arg.Any<CancellationToken>()).Returns(list);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _repositoryMock.Received(1).RemoveSessionAsync(item.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenListBelongsToAnotherUser()
    {
        // Arrange
        var realOwner = Guid.NewGuid();
        var attacker = Guid.NewGuid();
        var list = new FavoritedList(realOwner, "Relaxing");
        var item = new FavoritedSession(list.Id, Guid.NewGuid(), "Sessão de Sexta");
        var command = new RemoveFavoritedSessionCommandRequest(item.Id);

        _currentUserServiceMock.UserId.Returns(attacker);
        _repositoryMock.GetSessionByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);
        _repositoryMock.GetByIdAsync(list.Id, Arg.Any<CancellationToken>()).Returns(list);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        await _repositoryMock.DidNotReceive().RemoveSessionAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenItemDoesNotExist()
    {
        // Arrange
        var command = new RemoveFavoritedSessionCommandRequest(Guid.NewGuid());
        _repositoryMock.GetSessionByIdAsync(command.FavoritedSessionId, Arg.Any<CancellationToken>()).Returns((FavoritedSession?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}