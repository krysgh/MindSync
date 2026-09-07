using FluentAssertions;
using MindSync.Application.Common.Exceptions;
using MindSync.Application.Common.Interfaces;
using MindSync.Application.Features.StressSessions.Commands.CompleteStressSession;
using MindSync.Domain.Entities;
using MindSync.Domain.Enums;
using MindSync.Domain.Interfaces;
using NSubstitute;

namespace MindSync.Tests.UnitTests.Features.StressSessions;

public class CompleteStressSessionCommandHandlerTests
{
    private readonly IStressSessionRepository _repositoryMock = Substitute.For<IStressSessionRepository>();
    private readonly ICurrentUserService _currentUserServiceMock = Substitute.For<ICurrentUserService>();
    private readonly CompleteStressSessionCommandHandler _handler;

    public CompleteStressSessionCommandHandlerTests()
    {
        _handler = new CompleteStressSessionCommandHandler(_repositoryMock, _currentUserServiceMock);
    }

    [Fact]
    public async Task Handle_ShouldEndSession_WhenBelongingToTheLoggedInUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var session = new StressSession(Guid.NewGuid(), userId, DateTime.UtcNow, StressLevel.High, "Theta (6 Hz) - Meditação");
        var command = new CompleteStressSessionCommandRequest(session.Id, StressLevel.Low);

        _currentUserServiceMock.UserId.Returns(userId);
        _repositoryMock.GetSessionByIdAsync(session.Id, Arg.Any<CancellationToken>()).Returns(session);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        session.IsCompleted.Should().BeTrue();
        await _repositoryMock.Received(1).UpdateAsync(session, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenSessionBelongsToAnotherUser()
    {
        // Arrange
        var realOwner = Guid.NewGuid();
        var attacker = Guid.NewGuid();
        var session = new StressSession(Guid.NewGuid(), realOwner, DateTime.UtcNow, StressLevel.High, "Theta (6 Hz) - Meditação");
        var command = new CompleteStressSessionCommandRequest(session.Id, StressLevel.Low);

        _currentUserServiceMock.UserId.Returns(attacker);
        _repositoryMock.GetSessionByIdAsync(session.Id, Arg.Any<CancellationToken>()).Returns(session);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        session.IsCompleted.Should().BeFalse();
        await _repositoryMock.DidNotReceive().UpdateAsync(Arg.Any<StressSession>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenSessionDoesNotExist()
    {
        // Arrange
        var command = new CompleteStressSessionCommandRequest(Guid.NewGuid(), StressLevel.Low);
        _repositoryMock.GetSessionByIdAsync(command.SessionId, Arg.Any<CancellationToken>()).Returns((StressSession?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}