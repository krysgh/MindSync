using FluentAssertions;
using MindSync.Application.Common.Exceptions;
using MindSync.Application.Common.Interfaces;
using MindSync.Application.Features.FavoritedSessionLists.Commands.UpdateFavoritedSession;
using MindSync.Domain.Entities;
using MindSync.Domain.Exceptions;
using MindSync.Domain.Interfaces;
using NSubstitute;

namespace MindSync.Tests.UnitTests.Features.FavoritedSessionLists;

public class UpdateFavoritedSessionCommandHandlerTests
{
    private readonly IFavoritedListRepository _repositoryMock = Substitute.For<IFavoritedListRepository>();
    private readonly ICurrentUserService _currentUserServiceMock = Substitute.For<ICurrentUserService>();
    private readonly UpdateFavoritedSessionCommandHandler _handler;

    public UpdateFavoritedSessionCommandHandlerTests()
    {
        _handler = new UpdateFavoritedSessionCommandHandler(_repositoryMock, _currentUserServiceMock);
    }

    private (FavoritedList list, FavoritedSession item) CreateOwnedListWithItem(Guid userId, string customName = "Nome Antigo")
    {
        var list = new FavoritedList(userId, "Relaxing");
        var item = new FavoritedSession(list.Id, Guid.NewGuid(), customName);
        return (list, item);
    }

    [Fact]
    public async Task Handle_ShouldRename_WhenNameIsUniqueInListAndOwnerIsLoggedInUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var (list, item) = CreateOwnedListWithItem(userId);
        var command = new UpdateFavoritedSessionCommandRequest(item.Id, "Nome Novo");

        _currentUserServiceMock.UserId.Returns(userId);
        _repositoryMock.GetSessionByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);
        _repositoryMock.GetByIdAsync(list.Id, Arg.Any<CancellationToken>()).Returns(list);
        _repositoryMock.ExistsByCustomNameAndListIdAsync("Nome Novo", list.Id, item.Id, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        item.CustomName.Should().Be("Nome Novo");
        await _repositoryMock.Received(1).UpdateSessionAsync(item, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenNameAlreadyExistsInTheSameList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var (list, item) = CreateOwnedListWithItem(userId);
        var command = new UpdateFavoritedSessionCommandRequest(item.Id, "Nome Repetido");

        _currentUserServiceMock.UserId.Returns(userId);
        _repositoryMock.GetSessionByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);
        _repositoryMock.GetByIdAsync(list.Id, Arg.Any<CancellationToken>()).Returns(list);
        _repositoryMock.ExistsByCustomNameAndListIdAsync("Nome Repetido", list.Id, item.Id, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
                 .WithMessage("Já existe uma sessão chamada 'Nome Repetido' nesta lista.");

        await _repositoryMock.DidNotReceive().UpdateSessionAsync(Arg.Any<FavoritedSession>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenListBelongsToAnotherUser()
    {
        // Arrange
        var realOwner = Guid.NewGuid();
        var attacker = Guid.NewGuid();
        var (list, item) = CreateOwnedListWithItem(realOwner);
        var command = new UpdateFavoritedSessionCommandRequest(item.Id, "Nome Novo");

        _currentUserServiceMock.UserId.Returns(attacker);
        _repositoryMock.GetSessionByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);
        _repositoryMock.GetByIdAsync(list.Id, Arg.Any<CancellationToken>()).Returns(list);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        item.CustomName.Should().Be("Nome Antigo");
    }
}