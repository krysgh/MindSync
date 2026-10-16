using FluentAssertions;
using MindSync.Application.Common.Exceptions;
using MindSync.Application.Common.Interfaces;
using MindSync.Application.Features.FavoritedSessionLists.Commands.DeleteFavoritedList;
using MindSync.Domain.Entities;
using MindSync.Domain.Interfaces;
using NSubstitute;

namespace MindSync.Tests.UnitTests.Features.FavoritedSessionLists;

public class DeleteFavoritedListCommandHandlerTests
{
    private readonly IFavoritedListRepository _repositoryMock;
    private readonly ICurrentUserService _currentUserServiceMock;
    private readonly DeleteFavoritedListCommandHandler _handler;

    public DeleteFavoritedListCommandHandlerTests()
    {
        _repositoryMock = Substitute.For<IFavoritedListRepository>();
        _currentUserServiceMock = Substitute.For<ICurrentUserService>();
        _handler = new DeleteFavoritedListCommandHandler(_repositoryMock, _currentUserServiceMock);
    }

    [Fact]
    public async Task Handle_ShouldDeleteList_WhenTheListBelongsToLoggedInUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var list = new FavoritedList(userId, "Lista Para Deletar");
        var command = new DeleteFavoritedListCommandRequest(list.Id);

        _currentUserServiceMock.UserId.Returns(userId);
        _repositoryMock.GetByIdAsync(list.Id, Arg.Any<CancellationToken>()).Returns(list);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _repositoryMock.Received(1).DeleteAsync(list.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenListBelongsToAnotherUser()
    {
        // Arrange
        var ownerUserId = Guid.NewGuid();
        var loggedUserId = Guid.NewGuid();

        var list = new FavoritedList(ownerUserId, "Lista Alheia");
        var command = new DeleteFavoritedListCommandRequest(list.Id);

        _currentUserServiceMock.UserId.Returns(loggedUserId);
        _repositoryMock.GetByIdAsync(list.Id, Arg.Any<CancellationToken>()).Returns(list);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
                 .WithMessage("Lista de favoritos não encontrada.");

        await _repositoryMock.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}