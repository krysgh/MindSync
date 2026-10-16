using FluentAssertions;
using MindSync.Application.Common.Exceptions;
using MindSync.Application.Common.Interfaces;
using MindSync.Application.Features.StressSessions.Queries.GetStressSessionsByUserId;
using MindSync.Domain.Entities;
using MindSync.Domain.Enums;
using MindSync.Domain.Interfaces;
using NSubstitute;

namespace MindSync.Tests.UnitTests.Features.StressSessions;

public class GetStressSessionsByUserIdQueryHandlerTests
{
    private readonly IStressSessionRepository _repositoryMock = Substitute.For<IStressSessionRepository>();
    private readonly ICurrentUserService _currentUserServiceMock = Substitute.For<ICurrentUserService>();
    private readonly GetStressSessionsByUserIdQueryHandler _handler;

    public GetStressSessionsByUserIdQueryHandlerTests()
    {
        _handler = new GetStressSessionsByUserIdQueryHandler(_repositoryMock, _currentUserServiceMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnSessions_WhenUserIdIsTheSameAsTheLoggedInUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessions = new List<StressSession>
        {
            new(Guid.NewGuid(), userId, DateTime.UtcNow, StressLevel.Moderate, "Theta (6 Hz) - Meditação")
        };

        _currentUserServiceMock.UserId.Returns(userId);
        _repositoryMock.GetByUserIdAsync(userId).Returns(sessions);

        // Act
        var result = await _handler.Handle(new GetStressSessionsByUserIdQueryRequest(userId), CancellationToken.None);

        // Assert
        result.Should().ContainSingle();
        result.First().TargetFrequency.Should().Be("Theta (6 Hz) - Meditação");
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenUserIdIsOfAnotherUser()
    {
        // Arrange
        var realOwner = Guid.NewGuid();
        var attacker = Guid.NewGuid();

        _currentUserServiceMock.UserId.Returns(attacker);

        // Act
        Func<Task> act = async () => await _handler.Handle(new GetStressSessionsByUserIdQueryRequest(realOwner), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        await _repositoryMock.DidNotReceive().GetByUserIdAsync(Arg.Any<Guid>());
    }
}