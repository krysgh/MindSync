using FluentAssertions;
using MindSync.Application.Common.Exceptions;
using MindSync.Application.Common.Interfaces;
using MindSync.Application.Features.FavoritedSessionLists.Commands.UpdateFavoritedList;
using MindSync.Domain.Entities;
using MindSync.Domain.Exceptions;
using MindSync.Domain.Interfaces;
using NSubstitute;

namespace MindSync.Tests.UnitTests.Features.FavoritedSessionLists;

public class UpdateFavoritedListCommandHandlerTests
{
    private readonly IFavoritedListRepository _repositoryMock;
    private readonly ICurrentUserService _currentUserServiceMock;
    private readonly UpdateFavoritedListCommandHandler _handler;

    public UpdateFavoritedListCommandHandlerTests()
    {
        _repositoryMock = Substitute.For<IFavoritedListRepository>();
        _currentUserServiceMock = Substitute.For<ICurrentUserService>();
        _handler = new UpdateFavoritedListCommandHandler(_repositoryMock, _currentUserServiceMock);
    }

    [Fact]
    public async Task Handle_ShouldUpdateName_WhenListBelongsToUserAndNameIsUnique()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var list = new FavoritedList(userId, "Nome Antigo");
        var command = new UpdateFavoritedListCommandRequest(list.Id, "Novo Nome Atualizado");

        _currentUserServiceMock.UserId.Returns(userId);
        _repositoryMock.GetByIdAsync(list.Id, Arg.Any<CancellationToken>()).Returns(list);
        _repositoryMock.ExistsByNameAndUserIdAsync(command.Name, userId, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        list.Name.Should().Be("Novo Nome Atualizado");
        await _repositoryMock.Received(1).UpdateAsync(list, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenListBelongsToAnotherUser()
    {
        // Arrange
        var ownerUserId = Guid.NewGuid();
        var loggedUserId = Guid.NewGuid();

        var listOfAnotherUser = new FavoritedList(ownerUserId, "Lista Privada");
        var command = new UpdateFavoritedListCommandRequest(listOfAnotherUser.Id, "Tentativa de Alteracao");

        _currentUserServiceMock.UserId.Returns(loggedUserId);
        _repositoryMock.GetByIdAsync(listOfAnotherUser.Id, Arg.Any<CancellationToken>()).Returns(listOfAnotherUser);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
                 .WithMessage("Lista de favoritos não encontrada.");

        await _repositoryMock.DidNotReceive().UpdateAsync(Arg.Any<FavoritedList>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenNewNameAlreadyExistsForTheUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var list = new FavoritedList(userId, "Nome Atual");
        var command = new UpdateFavoritedListCommandRequest(list.Id, "Nome Já Existente");

        _currentUserServiceMock.UserId.Returns(userId);
        _repositoryMock.GetByIdAsync(list.Id, Arg.Any<CancellationToken>()).Returns(list);
        _repositoryMock.ExistsByNameAndUserIdAsync(command.Name, userId, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
                 .WithMessage($"Você já possui uma lista de favoritos com o nome '{command.Name}'.");

        await _repositoryMock.DidNotReceive().UpdateAsync(Arg.Any<FavoritedList>(), Arg.Any<CancellationToken>());
    }
}