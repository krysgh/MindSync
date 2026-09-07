using FluentAssertions;
using MindSync.Domain.Entities;
using MindSync.Domain.Enums;
using MindSync.Domain.Exceptions;

namespace MindSync.Tests.UnitTests.Entities;

public class StressSessionTests
{
    [Fact]
    public void Constructor_ShouldCreateSession_WithValidData()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        // Act
        var session = new StressSession(id, userId, createdAt, StressLevel.Moderate, "Theta (6 Hz) - Meditação");

        // Assert
        session.Id.Should().Be(id);
        session.UserId.Should().Be(userId);
        session.StressLevelBefore.Should().Be(StressLevel.Moderate);
        session.StressLevelAfter.Should().BeNull();
        session.IsCompleted.Should().BeFalse();
        session.EndedAt.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShouldThrowDomainException_WhenUserIdIsEmpty()
    {
        // Act
        Action act = () => new StressSession(Guid.NewGuid(), Guid.Empty, DateTime.UtcNow, StressLevel.Low, "Alpha (10 Hz) - Relaxamento");

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("O identificador do utilizador (UserId) não pode ser vazio.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowDomainException_WhenTargetFrequencyIsInvalid(string? targetFrequency)
    {
        // Act
        Action act = () => new StressSession(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, StressLevel.Low, targetFrequency!);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("A frequência alvo (TargetFrequency) deve ser preenchida.");
    }

    [Fact]
    public void CompleteSession_ShouldBeMarkedAsComplete_WhenNotYetFinished()
    {
        // Arrange
        var session = new StressSession(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, StressLevel.High, "Beta (20 Hz) - Foco e Atenção");

        // Act
        session.CompleteSession(StressLevel.Low);

        // Assert
        session.IsCompleted.Should().BeTrue();
        session.StressLevelAfter.Should().Be(StressLevel.Low);
        session.EndedAt.Should().NotBeNull();
    }

    [Fact]
    public void CompleteSession_ShouldThrowDomainException_WhenYetFinished()
    {
        // Arrange
        var session = new StressSession(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, StressLevel.High, "Beta (20 Hz) - Foco e Atenção");
        session.CompleteSession(StressLevel.Low);

        // Act
        Action act = () => session.CompleteSession(StressLevel.None);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Esta sessão já foi finalizada e avaliada.");
    }
}