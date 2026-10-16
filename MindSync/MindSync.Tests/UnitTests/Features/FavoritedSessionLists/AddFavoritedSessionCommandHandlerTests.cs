using FluentAssertions;
using MindSync.Application.Common.Exceptions;
using MindSync.Application.Common.Interfaces;
using MindSync.Application.Features.FavoritedSessionLists.Commands.AddFavoritedSession;
using MindSync.Domain.Entities;
using MindSync.Domain.Exceptions;
using MindSync.Domain.Interfaces;
using NSubstitute;

namespace MindSync.Tests.UnitTests.Features.FavoritedSessionLists;

public class AddFavoritedSessionCommandHandlerTests
{
    private readonly IFavoritedListRepository _repositoryMock;
    private readonly ICurrentUserService _currentUserServiceMock;
    private readonly AddFavoritedSessionCommandHandler _handler;

    public AddFavoritedSessionCommandHandlerTests()
    {
        _repositoryMock = Substitute.For<IFavoritedListRepository>();
        _currentUserServiceMock = Substitute.For<ICurrentUserService>();
        _handler = new AddFavoritedSessionCommandHandler(_repositoryMock, _currentUserServiceMock);
    }

    [Fact]
    public async Task Handle_ShouldAddSession_WhenListBelongsToLoggedInUserAndNameIsUnique()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var list = new FavoritedList(userId, "Lista Pessoal");
        var command = new AddFavoritedSessionCommandRequest(list.Id, Guid.NewGuid(), "Nome Customizado");

        _currentUserServiceMock.UserId.Returns(userId);
        _repositoryMock.GetByIdAsync(list.Id, Arg.Any<CancellationToken>()).Returns(list);
        _repositoryMock.ExistsByCustomNameAndListIdAsync(command.CustomName, list.Id, null, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var sessionId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        sessionId.Should().NotBeEmpty();
        await _repositoryMock.Received(1).AddSessionAsync(
            Arg.Is<FavoritedSession>(s => s.FavoritedListId == list.Id && s.CustomName == command.CustomName),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenListBelongsToAnotherUser()
    {
        // Arrange
        var ownerUserId = Guid.NewGuid();
        var loggedUserId = Guid.NewGuid();

        var listOfAnotherUser = new FavoritedList(ownerUserId, "Lista Privada");
        var command = new AddFavoritedSessionCommandRequest(listOfAnotherUser.Id, Guid.NewGuid(), "Tentativa Invasão");

        _currentUserServiceMock.UserId.Returns(loggedUserId);
        _repositoryMock.GetByIdAsync(listOfAnotherUser.Id, Arg.Any<CancellationToken>()).Returns(listOfAnotherUser);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
                 .WithMessage("Lista de favoritos não encontrada.");

        await _repositoryMock.DidNotReceive().AddSessionAsync(Arg.Any<FavoritedSession>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenASessionWithTheSameNameAlreadyExistsInTheList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var list = new FavoritedList(userId, "Lista Pessoal");
        var command = new AddFavoritedSessionCommandRequest(list.Id, Guid.NewGuid(), "Nome Repetido");

        _currentUserServiceMock.UserId.Returns(userId);
        _repositoryMock.GetByIdAsync(list.Id, Arg.Any<CancellationToken>()).Returns(list);
        _repositoryMock.ExistsByCustomNameAndListIdAsync(command.CustomName, list.Id, null, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
                 .WithMessage("Já existe uma sessão chamada 'Nome Repetido' nesta lista.");

        await _repositoryMock.DidNotReceive().AddSessionAsync(Arg.Any<FavoritedSession>(), Arg.Any<CancellationToken>());
    }
}