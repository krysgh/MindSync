using FluentAssertions;
using MindSync.Domain.Entities;
using MindSync.Domain.Exceptions;

namespace MindSync.Tests.UnitTests.Entities;

public class AudioTrackTests
{
    private static AudioTrack CreateValidTrack(int min = 0, int max = 8) =>
        new(Guid.NewGuid(), "Textura Calma", "Texture", null, min, max, "texture/calm.mp3", 600, DateTime.UtcNow);

    [Fact]
    public void Constructor_ShouldCreateRange_WithValidData()
    {
        // Act
        var track = CreateValidTrack();

        // Assert
        track.Name.Should().Be("Textura Calma");
        track.Role.Should().Be("Texture");
        track.TargetFrequency.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShouldThrowDomainException_IdIsNotEmpty()
    {
        // Act
        Action act = () => new AudioTrack(Guid.Empty, "Nome", "Texture", null, 0, 8, "arquivo.mp3", 600, DateTime.UtcNow);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("O identificador da faixa de áudio (Id) não pode ser vazio.");
    }

    [Fact]
    public void Constructor_ShouldThrowDomainException_WhenMinStressLevelIsGreaterThanMax()
    {
        // Act
        Action act = () => new AudioTrack(Guid.NewGuid(), "Nome", "Texture", null, 6, 2, "arquivo.mp3", 600, DateTime.UtcNow);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("O nível mínimo de estresse não pode ser maior que o máximo.");
    }

    [Fact]
    public void Constructor_ShouldThrowDomainException_WhenDurationIsZeroOrNegative()
    {
        // Act
        Action act = () => new AudioTrack(Guid.NewGuid(), "Nome", "Texture", null, 0, 8, "arquivo.mp3", 0, DateTime.UtcNow);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("A duração da faixa de áudio deve ser maior que zero.");
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(8, true)]
    [InlineData(4, true)]
    [InlineData(9, false)]
    public void MatchesStressLevel_ShouldRespectInclusiveLimits(
        int level,
        bool expected)
    {
        // Arrange
        var track = CreateValidTrack(min: 0, max: 8);

        // Act
        var result = track.MatchesStressLevel(level);

        // Assert
        result.Should().Be(expected);
    }
}