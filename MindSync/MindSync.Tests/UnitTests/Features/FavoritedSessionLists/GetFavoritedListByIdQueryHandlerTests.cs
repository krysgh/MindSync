using FluentAssertions;
using MindSync.Application.Common.Exceptions;
using MindSync.Application.Common.Interfaces;
using MindSync.Application.Features.FavoritedSessionLists.Interfaces;
using MindSync.Application.Features.FavoritedSessionLists.Queries.GetFavoritedListById;
using NSubstitute;

namespace MindSync.Tests.UnitTests.Features.FavoritedSessionLists;

public class GetFavoritedListByIdQueryHandlerTests
{
    private readonly IFavoritedListQueries _queriesMock = Substitute.For<IFavoritedListQueries>();
    private readonly ICurrentUserService _currentUserServiceMock = Substitute.For<ICurrentUserService>();
    private readonly GetFavoritedListByIdQueryHandler _handler;

    public GetFavoritedListByIdQueryHandlerTests()
    {
        _handler = new GetFavoritedListByIdQueryHandler(_queriesMock, _currentUserServiceMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnList_WhenItBelongsToTheLoggedInUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var listId = Guid.NewGuid();
        var response = new GetFavoritedListByIdQueryResponse(listId, userId, "Relaxing", DateTime.UtcNow);

        _currentUserServiceMock.UserId.Returns(userId);
        _queriesMock.GetByIdAsync(listId, Arg.Any<CancellationToken>()).Returns(response);

        // Act
        var result = await _handler.Handle(new GetFavoritedListByIdQueryRequest(listId), CancellationToken.None);

        // Assert
        result.Should().Be(response);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenListBelongsToAnotherUser()
    {
        // Arrange
        var realOwner = Guid.NewGuid();
        var attacker = Guid.NewGuid();
        var listId = Guid.NewGuid();
        var response = new GetFavoritedListByIdQueryResponse(listId, realOwner, "Relaxing", DateTime.UtcNow);

        _currentUserServiceMock.UserId.Returns(attacker);
        _queriesMock.GetByIdAsync(listId, Arg.Any<CancellationToken>()).Returns(response);

        // Act
        Func<Task> act = async () => await _handler.Handle(new GetFavoritedListByIdQueryRequest(listId), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenListDoesNotExist()
    {
        // Arrange
        var listId = Guid.NewGuid();
        _queriesMock.GetByIdAsync(listId, Arg.Any<CancellationToken>()).Returns((GetFavoritedListByIdQueryResponse?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(new GetFavoritedListByIdQueryRequest(listId), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}