using FluentAssertions;
using MindSync.Domain.Entities;
using MindSync.Domain.Exceptions;

namespace MindSync.Tests.UnitTests.Entities;

public class SessionAudioMixLayerTests
{
    [Fact]
    public void Constructor_ShouldCreateLayer_WithValidData()
    {
        // Act
        var layer = new SessionAudioMixLayer(Guid.NewGuid(), Guid.NewGuid(), 0.5m);

        // Assert
        layer.Id.Should().NotBeEmpty();
        layer.Volume.Should().Be(0.5m);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void Constructor_ShouldThrowDomainException_WhenVolumeIsOutOfRange(decimal volume)
    {
        // Act
        Action act = () => new SessionAudioMixLayer(Guid.NewGuid(), Guid.NewGuid(), volume);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("O volume da camada deve estar entre 0 e 1.");
    }

    [Fact]
    public void Constructor_ShouldThrowDomainException_WhenAudioTrackIdIdIsEmpty()
    {
        // Act
        Action act = () => new SessionAudioMixLayer(Guid.NewGuid(), Guid.Empty, 0.5m);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("O identificador da faixa de áudio (AudioTrackId) não pode ser vazio.");
    }
}